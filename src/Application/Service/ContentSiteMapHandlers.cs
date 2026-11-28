namespace GamaEdtech.Application.Service
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.SiteMap;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Enumeration;

    /// <summary>
    /// The sitemap's past papers, exams and tutorials, from this backend's copy (since 2026-11-23; until then the website served
    /// gama-api's own sitemap files for them, which never list what is made here). Each URL is the page's canonical one,
    /// <c>/{type}/{id}/{title_url}</c>, with gama-api's <c>title_url</c> (<see cref="PastPaperOutput.TitleUrl"/>), so it never
    /// redirects. See docs/architecture/cross-cutting-concerns.md, "Sitemap".
    /// </summary>
    public abstract class ContentSiteMapHandler : ISiteMapHandler
    {
        public abstract ItemType ItemType { get; }

        public abstract IQueryable<SiteMapItemDto> GetSiteMapData([NotNull] IUnitOfWork uow);

        public string? Slug(string? path2)
        {
            var slug = PastPaperOutput.TitleUrl(path2);
            return slug.Length == 0 ? null : Uri.EscapeDataString(slug);
        }
    }

    /// <summary>Published papers.</summary>
    public sealed class PastPaperSiteMapHandler : ContentSiteMapHandler
    {
        public override ItemType ItemType => ItemType.Paper;

        public override IQueryable<SiteMapItemDto> GetSiteMapData([NotNull] IUnitOfWork uow) => uow.GetRepository<PastPaper>()
            .GetManyQueryable(t => t.Status == PastPaperStatus.Confirmed && !t.IsDeleted)
            .Select(t => new SiteMapItemDto { Id = t.Id, Path1 = t.Id.ToString(), Path2 = t.Title, LastModifyDate = t.LastModifyDate ?? t.CreationDate });
    }

    /// <summary>Public exams: confirmed (1), cloned (5) or published by their teacher (7), not deleted.</summary>
    public sealed class ExamSiteMapHandler : ContentSiteMapHandler
    {
        public override ItemType ItemType => ItemType.Exam;

        public override IQueryable<SiteMapItemDto> GetSiteMapData([NotNull] IUnitOfWork uow) => uow.GetRepository<Exam>()
            .GetManyQueryable(t => (t.Status == 1 || t.Status == 5 || t.Status == 7) && !t.IsDeleted)
            .Select(t => new SiteMapItemDto { Id = t.Id, Path1 = t.Id.ToString(), Path2 = t.Title, LastModifyDate = t.LastModifyDate ?? t.CreationDate });
    }

    /// <summary>Confirmed tutorials.</summary>
    public sealed class TutorialSiteMapHandler : ContentSiteMapHandler
    {
        public override ItemType ItemType => ItemType.Tutorial;

        public override IQueryable<SiteMapItemDto> GetSiteMapData([NotNull] IUnitOfWork uow) => uow.GetRepository<Tutorial>()
            .GetManyQueryable(t => t.Status == TutorialStatus.Confirmed)
            .Select(t => new SiteMapItemDto { Id = t.Id, Path1 = t.Id.ToString(), Path2 = t.Title, LastModifyDate = t.LastModifyDate ?? t.CreationDate });
    }

    /// <summary>
    /// The curriculum model's landing pages (<c>/boards/{board}/{level}/{subject}</c>), only once the website reads the model
    /// (<c>Migration:Routes:Curriculum.Website</c> Native). Ids are offset per level of the page so each URL has its own.
    /// </summary>
    public sealed class CurriculumSiteMapHandler(Lazy<IMigrationRouter> migrationRouter) : ISiteMapHandler
    {
        private const string Separator = "/";
        private const long LevelOffset = 1_000_000;
        private const long SubjectOffset = 2_000_000;

        public ItemType ItemType => ItemType.Board;

        public IQueryable<SiteMapItemDto> GetSiteMapData([NotNull] IUnitOfWork uow)
        {
            var boards = uow.GetRepository<Domain.Entity.Curriculum.CurriculumBoard, int>().GetManyQueryable(t => t.IsEnabled);
            if (migrationRouter.Value.GetRoute(MigrationDomain.Curriculum, MigrationFeatures.Website) != MigrationRoute.Native)
            {
                boards = boards.Where(t => false);
            }

            var levels = uow.GetRepository<Domain.Entity.Curriculum.CurriculumQualification, int>().GetManyQueryable();
            var subjects = uow.GetRepository<Domain.Entity.Curriculum.CurriculumSyllabus, int>().GetManyQueryable();
            var boardPages = boards.Select(t => new SiteMapItemDto { Id = t.Id, Path1 = t.Slug, LastModifyDate = t.LastModifyDate ?? t.CreationDate });
            var levelPages = from l in levels
                             join b in boards on l.BoardId equals b.Id
                             select new SiteMapItemDto { Id = LevelOffset + l.Id, Path1 = b.Slug + Separator + l.Slug, LastModifyDate = l.LastModifyDate ?? l.CreationDate };
            var subjectPages = from s in subjects
                               join l in levels on s.QualificationId equals l.Id
                               join b in boards on l.BoardId equals b.Id
                               select new SiteMapItemDto { Id = SubjectOffset + s.Id, Path1 = b.Slug + Separator + l.Slug + Separator + s.Slug, LastModifyDate = s.LastModifyDate ?? s.CreationDate };
            return boardPages.Concat(levelPages).Concat(subjectPages);
        }

        /// <summary>The path is already the slugs; nothing is added.</summary>
        public string? Slug(string? path2) => null;
    }
}
