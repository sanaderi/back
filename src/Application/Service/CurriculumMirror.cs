namespace GamaEdtech.Application.Service
{
    using System.Globalization;
    using System.Linq;

    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.Taxonomy;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Entity.Curriculum;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Keeps the old ids working once the curriculum model is the one edited (phase 7). Every past paper, tutorial, exam,
    /// form and list still speaks the lookup lists' ids, so a row made or renamed in Admin › Curriculum after
    /// <c>Migration:WriteRoutes:Curriculum</c> is Native is also written to <c>LegacyTaxonomyItems</c> (a new id after the
    /// highest, as <c>admin/types</c> does), and gets that id as its <c>LegacyId</c>. A board created in the admin can then
    /// have papers, exams, tutorials and schools like any other. It needs <c>WriteRoutes:Types</c> Native too, so gama-api's
    /// copy can never bring the same id. And a school's boards keep its <c>SchoolQualifications</c> in step.
    /// See docs/business/curriculum.md, "After the write switch".
    /// </summary>
    internal static class CurriculumMirror
    {
        /// <summary>Writes one lookup item (new, or the one <paramref name="legacyId"/> names) and answers its id.</summary>
        internal static async Task<int> UpsertAsync(IUnitOfWork uow, int? legacyId, short kind, string title, string parent, int listOrder, string? metadata = null)
        {
            var repository = uow.GetRepository<LegacyTaxonomyItem, int>();
            var item = legacyId is int id
                ? await repository.GetManyQueryable(t => t.Id == id, includes: t => t.Include(p => p.Parents), tracking: true).FirstOrDefaultAsync()
                : null;
            if (item is null)
            {
                item = new()
                {
                    Id = legacyId ?? ((await repository.GetManyQueryable(t => true).MaxAsync(t => (int?)t.Id) ?? 0) + 1),
                    Kind = kind,
                    SyncDate = DateTimeOffset.UtcNow,
                    Parents = [],
                };
                repository.Add(item);
            }

            LegacyTaxonomyAdminService.Apply(item, new TaxonomyItemEditDto { Kind = kind, Title = title, ListOrder = listOrder, Parent = parent, Metadata = metadata });
            item.IsDeleted = false;
            _ = await uow.SaveChangesAsync();
            return item.Id;
        }

        /// <summary>A board's lookup item.</summary>
        internal static async Task MirrorBoardAsync(IUnitOfWork uow, CurriculumBoard board) =>
            board.LegacyId = await UpsertAsync(uow, board.LegacyId, LegacyTaxonomyKinds.Board, board.Name!, string.Empty, board.ListOrder);

        /// <summary>A qualification's lookup item (a grade, under its board's).</summary>
        internal static async Task MirrorQualificationAsync(IUnitOfWork uow, CurriculumQualification level)
        {
            var board = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.Id == level.BoardId).Select(t => t.LegacyId).FirstAsync();
            level.LegacyId = await UpsertAsync(uow, level.LegacyId, LegacyTaxonomyKinds.Grade, level.Name!, Key(board), level.ListOrder);
        }

        /// <summary>A syllabus's lookup item (a subject "Name (Code)", under its grade's as <c>0,grade</c>).</summary>
        internal static async Task MirrorSyllabusAsync(IUnitOfWork uow, CurriculumSyllabus syllabus)
        {
            var grade = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.Id == syllabus.QualificationId).Select(t => t.LegacyId).FirstAsync();
            var title = syllabus.Code is null ? syllabus.Name! : $"{syllabus.Name} ({syllabus.Code})";
            syllabus.LegacyId = await UpsertAsync(uow, syllabus.LegacyId, LegacyTaxonomyKinds.Subject, title, $"0,{Key(grade)}", syllabus.ListOrder);
        }

        /// <summary>
        /// A syllabus's topics: gama-api lists them flat (a unit, then its chapters, by list order), so every topic of the
        /// syllabus is numbered again in tree order, and the ones with no lookup item get one.
        /// </summary>
        internal static async Task MirrorTopicsAsync(IUnitOfWork uow, int syllabusId)
        {
            var subject = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.Id == syllabusId).Select(t => t.LegacyId).FirstAsync();
            var topics = await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.SyllabusId == syllabusId, tracking: true).ToListAsync();
            List<CurriculumTopic> ordered = [];
            foreach (var unit in topics.Where(t => t.ParentTopicId is null).OrderBy(t => t.ListOrder).ThenBy(t => t.Id))
            {
                ordered.Add(unit);
                ordered.AddRange(topics.Where(t => t.ParentTopicId == unit.Id).OrderBy(t => t.ListOrder).ThenBy(t => t.Id));
            }

            for (var i = 0; i < ordered.Count; i++)
            {
                var topic = ordered[i];
                topic.LegacyId = await UpsertAsync(uow, topic.LegacyId, LegacyTaxonomyKinds.Topic, topic.Name!, Key(subject), i + 1,
                    topic.ParentTopicId is null && topic.IsUnit ? "{\"season\":true}" : null);
            }
        }

        /// <summary>A paper's lookup item: its board's classification of that title ("Paper 4", shared by the board's
        /// subjects as on gama-api), made when the board has none.</summary>
        internal static async Task MirrorPaperAsync(IUnitOfWork uow, CurriculumPaper paper)
        {
            var board = await (from s in uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.Id == paper.SyllabusId)
                               join q in uow.GetRepository<CurriculumQualification, int>().GetManyQueryable() on s.QualificationId equals q.Id
                               join b in uow.GetRepository<CurriculumBoard, int>().GetManyQueryable() on q.BoardId equals b.Id
                               select b.LegacyId).FirstAsync();
            var key = Key(board);
            paper.LegacyId ??= await uow.GetRepository<LegacyTaxonomyItem, int>()
                .GetManyQueryable(t => t.Kind == LegacyTaxonomyKinds.Classification && !t.IsDeleted && t.IsPaper == true && t.Title == paper.Name && t.Parents!.Any(p => p.ParentKey == key))
                .Select(t => (int?)t.Id).FirstOrDefaultAsync();
            paper.LegacyId = await UpsertAsync(uow, paper.LegacyId, LegacyTaxonomyKinds.Classification, paper.Name!, key, paper.ListOrder, "{\"is_paper\":true}");
        }

        /// <summary>A variant option's lookup item: the variant of that title, shared by every board as on gama-api.</summary>
        internal static async Task MirrorVariantAsync(IUnitOfWork uow, CurriculumVariantOption option)
        {
            var title = option.Code ?? option.Label!;
            option.LegacyId ??= await uow.GetRepository<LegacyTaxonomyItem, int>()
                .GetManyQueryable(t => t.Kind == LegacyTaxonomyKinds.Variant && !t.IsDeleted && t.Title == title).Select(t => (int?)t.Id).FirstOrDefaultAsync();
            option.LegacyId ??= await UpsertAsync(uow, null, LegacyTaxonomyKinds.Variant, title, string.Empty, option.ListOrder);
        }

        /// <summary>
        /// A school's <c>SchoolQualifications</c> from its <c>SchoolBoards</c>: a board it has gets its board-only row, a board it
        /// no longer has loses the row copied from it. Rows an admin added with a qualification are left alone.
        /// </summary>
        internal static async Task SyncSchoolAsync(IUnitOfWork uow, long schoolId)
        {
            var boards = await (from schoolBoard in uow.GetRepository<SchoolBoard>().GetManyQueryable(t => t.SchoolId == schoolId)
                                join board in uow.GetRepository<Board, int>().GetManyQueryable() on schoolBoard.BoardId equals board.Id
                                join curriculum in uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId != null) on board.Code equals curriculum.LegacyId
                                select new { schoolBoard.Id, BoardId = curriculum.Id, schoolBoard.CreationDate, schoolBoard.CreationUserId }).ToListAsync();
            var repository = uow.GetRepository<SchoolQualification>();
            var rows = await repository.GetManyQueryable(t => t.SchoolId == schoolId, tracking: true).ToListAsync();

            foreach (var gone in rows.Where(t => t.LegacySchoolBoardId is long id && boards.TrueForAll(b => b.Id != id)).ToList())
            {
                repository.Remove(gone);
                _ = rows.Remove(gone);
            }

            foreach (var board in boards.Where(b => !rows.Exists(t => t.LegacySchoolBoardId == b.Id || (t.BoardId == b.BoardId && t.QualificationId == null))))
            {
                repository.Add(new() { SchoolId = schoolId, BoardId = board.BoardId, LegacySchoolBoardId = board.Id, CreationDate = board.CreationDate, CreationUserId = board.CreationUserId });
            }

            _ = await uow.SaveChangesAsync();
        }

        private static string Key(int? legacyId) => legacyId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
