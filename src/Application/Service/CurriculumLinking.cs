namespace GamaEdtech.Application.Service
{
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The content lists' topic links from the curriculum model (phase 3), used instead of gama-api's endless chain
    /// (<see cref="TopicMatching"/>) when <c>Migration:Routes:Curriculum.Website</c> is <c>Native</c>: one hop through GAMA,
    /// exact links only, each board's tab scope, ordered own → GAMA → the other boards. Content still carries the lookup
    /// lists' ids, so the model is joined through each row's <c>LegacyId</c>. Shadow serves the old links: the new ones
    /// differ on purpose, so comparing them would only report the decision. See docs/business/curriculum.md.
    /// </summary>
    internal static class CurriculumLinking
    {
        /// <summary>Added by the search (never taken from a visitor: it is not one of the search's query keys) when the
        /// curriculum route is Native, so the content sources use the model.</summary>
        internal const string Flag = "_curriculum";

        /// <summary>A list's plan: the board's own topic keys when a topic is chosen, and the linked groups in order.</summary>
        internal sealed record Plan(IReadOnlyList<string> OwnTopicKeys, IReadOnlyList<Group> Groups);

        /// <summary>One linked group: its source (<see cref="CurriculumLinkSource"/>), its board, the topic keys it brings in.</summary>
        internal sealed record Group(string LinkSource, int? BoardLegacyId, IReadOnlyList<string> TopicKeys);

        /// <summary>The query with the flag when the curriculum's website route is Native.</summary>
        internal static string Mark(IMigrationRouter router, string query)
        {
            ArgumentNullException.ThrowIfNull(router);
            if (router.GetRoute(MigrationDomain.Curriculum, MigrationFeatures.Website) != MigrationRoute.Native)
            {
                return query;
            }

            var separator = query.Length == 0 || query.EndsWith('&') || query.EndsWith('?') ? string.Empty : "&";
            return $"{query}{separator}{Flag}=1";
        }

        internal static bool IsOn(IReadOnlyDictionary<string, string?> search) => search.GetValueOrDefault(Flag) == "1";

        /// <summary>
        /// The plan of a list filtered by a topic, else a subject, else a level, else a board (the lookup ids the search
        /// takes); null when none is set or the model doesn't know it (the list then shows the board's own content only).
        /// </summary>
        internal static async Task<Plan?> PlanAsync(IUnitOfWork uow, CurriculumContentTab tab, long? section, long? grade, long? lesson, long? topic)
        {
            var topics = uow.GetRepository<CurriculumTopic, int>().GetManyQueryable();
            var syllabi = uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable();
            var qualifications = uow.GetRepository<CurriculumQualification, int>().GetManyQueryable();
            var boards = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable().Select(t => new { t.Id, t.LegacyId, t.IsMaster, t.ListOrder }).ToListAsync();

            // The board's own topics (curriculum ids) the list is about, and the board.
            IQueryable<CurriculumTopic> own;
            int? boardId;
            if (topic is long topicLegacy)
            {
                var row = await topics.Where(t => t.LegacyId == topicLegacy).Select(t => new { t.Id, t.SyllabusId }).FirstOrDefaultAsync();
                if (row is null)
                {
                    return null;
                }

                own = topics.Where(t => t.Id == row.Id || t.ParentTopicId == row.Id);
                boardId = await (from s in syllabi where s.Id == row.SyllabusId join q in qualifications on s.QualificationId equals q.Id select (int?)q.BoardId).FirstOrDefaultAsync();
            }
            else if (lesson is long lessonLegacy)
            {
                var syllabusId = await syllabi.Where(t => t.LegacyId == lessonLegacy).Select(t => (int?)t.Id).FirstOrDefaultAsync();
                own = topics.Where(t => t.SyllabusId == syllabusId);
                boardId = await (from s in syllabi where s.Id == syllabusId join q in qualifications on s.QualificationId equals q.Id select (int?)q.BoardId).FirstOrDefaultAsync();
            }
            else if (grade is long gradeLegacy)
            {
                var qualification = await qualifications.Where(t => t.LegacyId == gradeLegacy).Select(t => new { t.Id, t.BoardId }).FirstOrDefaultAsync();
                own = from t in topics join s in syllabi on t.SyllabusId equals s.Id where s.QualificationId == qualification!.Id select t;
                boardId = qualification?.BoardId;
            }
            else if (section is long sectionLegacy)
            {
                boardId = boards.Find(t => t.LegacyId == sectionLegacy)?.Id;
                own = from t in topics join s in syllabi on t.SyllabusId equals s.Id join q in qualifications on s.QualificationId equals q.Id where q.BoardId == boardId select t;
            }
            else
            {
                return null;
            }

            var board = boards.Find(t => t.Id == boardId);
            if (board is null)
            {
                return null;
            }

            var ownIds = own.Select(t => t.Id);
            var links = uow.GetRepository<CurriculumTopicLink, int>().GetManyQueryable();
            var masters = links.Where(t => ownIds.Contains(t.TopicId)).Select(t => t.MasterTopicId);
            var edges = await (from link in links
                               where board.IsMaster ? ownIds.Contains(link.MasterTopicId) : masters.Contains(link.MasterTopicId)
                               join t in topics on link.TopicId equals t.Id
                               join s in syllabi on t.SyllabusId equals s.Id
                               join q in qualifications on s.QualificationId equals q.Id
                               select new CurriculumLinkEdge(link.MasterTopicId, link.TopicId, q.BoardId, link.Kind)).ToListAsync();

            var scope = await uow.GetRepository<CurriculumBoardTab, int>().GetManyQueryable(t => t.BoardId == board.Id && t.Tab == tab)
                .Select(t => t.Scope).FirstOrDefaultAsync() ?? CurriculumLinkRules.DefaultScope(tab);
            var order = await uow.GetRepository<CurriculumBoardLinkOrder, int>().GetManyQueryable(t => t.BoardId == board.Id)
                .OrderBy(t => t.ListOrder).Select(t => t.OtherBoardId).ToListAsync();

            // Only the own topics that are linked matter to the rules (a board has thousands; most are not linked).
            var ownLinked = board.IsMaster
                ? await links.Where(t => ownIds.Contains(t.MasterTopicId)).Select(t => t.MasterTopicId).Distinct().ToListAsync()
                : await links.Where(t => ownIds.Contains(t.TopicId)).Select(t => t.TopicId).Distinct().ToListAsync();
            var resolved = CurriculumLinkRules.Resolve(board.Id, board.IsMaster, ownLinked, edges, scope, order, boards.ToDictionary(t => t.Id, t => t.ListOrder));

            List<int> wanted = [.. resolved.Master, .. resolved.OtherBoards.SelectMany(t => t.TopicIds)];
            var keys = await topics.Where(t => wanted.Contains(t.Id) && t.LegacyId != null).ToDictionaryAsync(t => t.Id, t => t.LegacyId!.Value.ToString(CultureInfo.InvariantCulture));
            List<string> ownKeys = [];
            if (topic is not null)
            {
                var ownLegacy = await own.Where(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync();
                ownKeys = [.. ownLegacy.Select(t => t.ToString(CultureInfo.InvariantCulture))];
            }

            List<Group> groups = [];
            var master = boards.Find(t => t.IsMaster);
            if (resolved.Master.Count > 0)
            {
                groups.Add(new(CurriculumLinkSource.Master, master?.LegacyId, [.. resolved.Master.Where(keys.ContainsKey).Select(t => keys[t])]));
            }

            groups.AddRange(resolved.OtherBoards.Select(t => new Group(CurriculumLinkSource.OtherBoard, boards.Find(b => b.Id == t.BoardId)?.LegacyId,
                [.. t.TopicIds.Where(keys.ContainsKey).Select(id => keys[id])])));
            return new(ownKeys, [.. groups.Where(t => t.TopicKeys.Count > 0)]);
        }

        /// <summary>A row's place in a linked list: 0 for the board's own, then 1, 2... for each group in order.</summary>
        internal static Expression<Func<T, int>> Rank<T>(Expression<Func<T, bool>> own, IReadOnlyList<Expression<Func<T, bool>>> groups)
        {
            ArgumentNullException.ThrowIfNull(own);
            ArgumentNullException.ThrowIfNull(groups);
            var parameter = own.Parameters[0];
            Expression body = Expression.Constant(groups.Count + 1);
            for (var i = groups.Count - 1; i >= 0; i--)
            {
                body = Expression.Condition(Rebind(groups[i], parameter), Expression.Constant(i + 1), body);
            }

            return Expression.Lambda<Func<T, int>>(Expression.Condition(own.Body, Expression.Constant(0), body), parameter);
        }

        /// <summary>The rows of one rank.</summary>
        internal static Expression<Func<T, bool>> RankIs<T>(Expression<Func<T, int>> rank, int value)
        {
            ArgumentNullException.ThrowIfNull(rank);
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(rank.Body, Expression.Constant(value)), rank.Parameters);
        }

        /// <summary>Orders by rank first (when ranked), then by <paramref name="key"/>.</summary>
        internal static IOrderedQueryable<T> OrderFirst<T, TKey>(IQueryable<T> query, Expression<Func<T, int>>? rank, Expression<Func<T, TKey>> key, bool descending)
        {
            ArgumentNullException.ThrowIfNull(query);
            if (rank is null)
            {
                return descending ? query.OrderByDescending(key) : query.OrderBy(key);
            }

            var ranked = query.OrderBy(rank);
            return descending ? ranked.ThenByDescending(key) : ranked.ThenBy(key);
        }

        /// <summary>The rows the board owns or any group brings in.</summary>
        internal static Expression<Func<T, bool>> Any<T>(Expression<Func<T, bool>> own, IReadOnlyList<Expression<Func<T, bool>>> groups)
        {
            ArgumentNullException.ThrowIfNull(own);
            ArgumentNullException.ThrowIfNull(groups);
            var parameter = own.Parameters[0];
            var body = groups.Aggregate(own.Body, (current, group) => Expression.OrElse(current, Rebind(group, parameter)));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>The source name of a rank (<see cref="Rank"/>): own, then each group's.</summary>
        internal static string Source(Plan plan, int rank) => rank == 0 || rank > plan.Groups.Count ? CurriculumLinkSource.Own : plan.Groups[rank - 1].LinkSource;

        private static Expression Rebind<T>(Expression<Func<T, bool>> lambda, ParameterExpression parameter) =>
            new ParameterSwap(lambda.Parameters[0], parameter).Visit(lambda.Body);

        private sealed class ParameterSwap(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
        }
    }
}
