namespace GamaEdtech.Data.Dto.Curriculum
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using GamaEdtech.Domain.Enumeration;

    /// <summary>One reviewed link: a GAMA topic, another board's topic (and that board), and how closely they match.</summary>
    public readonly record struct CurriculumLinkEdge(int MasterTopicId, int TopicId, int TopicBoardId, CurriculumLinkKind Kind);

    /// <summary>Another board's topics a list brings in, in that board's place.</summary>
    public sealed record CurriculumBoardTopics(int BoardId, IReadOnlyList<int> TopicIds);

    /// <summary>
    /// The topics whose content a board's tab lists, in the order the groups are shown: the board's own, then GAMA's, then
    /// each other linked board's. <see cref="Related"/> are the partial matches, shown only as "related" on a detail page.
    /// </summary>
    public sealed record CurriculumLinkedTopics(
        IReadOnlyList<int> Own,
        IReadOnlyList<int> Master,
        IReadOnlyList<CurriculumBoardTopics> OtherBoards,
        IReadOnlyList<int> Related);

    /// <summary>
    /// How lists follow topic links between boards (decided 2026-11-28, see docs/business/curriculum.md):
    /// <list type="bullet">
    /// <item>Links go through GAMA only, one hop: Cambridge reaches Edexcel through the GAMA topic both are linked to, never
    /// further.</item>
    /// <item>Only exact links bring content into a list; partial ones are <see cref="CurriculumLinkedTopics.Related"/>.</item>
    /// <item>Each tab of a board has a <see cref="CurriculumLinkScope"/>; by default past papers are the board's own only
    /// and every other tab takes GAMA's and the other boards' too. An admin changes it per board and tab.</item>
    /// <item>The order is always own, GAMA, then the other boards: first those the board lists (its
    /// <c>CurriculumBoardLinkOrders</c>), then the rest by their list order.</item>
    /// </list>
    /// </summary>
    public static class CurriculumLinkRules
    {
        /// <summary>The tabs that follow links, in the order the admin shows them.</summary>
        public static readonly IReadOnlyList<CurriculumContentTab> Tabs =
            [CurriculumContentTab.PastPapers, CurriculumContentTab.StudyMaterials, CurriculumContentTab.Exams, CurriculumContentTab.Tutorials];

        /// <summary>A tab's scope when its board has not set one.</summary>
        public static CurriculumLinkScope DefaultScope(CurriculumContentTab tab)
        {
            ArgumentNullException.ThrowIfNull(tab);
            return tab == CurriculumContentTab.PastPapers ? CurriculumLinkScope.Own : CurriculumLinkScope.All;
        }

        /// <summary>
        /// The topics a list of <paramref name="boardId"/>'s <paramref name="topicIds"/> shows, grouped and ordered.
        /// </summary>
        /// <param name="boardId">The board the visitor chose.</param>
        /// <param name="boardIsMaster">It is GAMA: its topics are the hub, so there is no GAMA group.</param>
        /// <param name="topicIds">The board's topics the list is for (a topic, or every topic of a subject).</param>
        /// <param name="links">The links touching those topics or their GAMA topics (more is fine).</param>
        /// <param name="scope">The tab's scope.</param>
        /// <param name="preferredBoardOrder">The board's own order of the other boards.</param>
        /// <param name="boardListOrder">Every board's list order, for the boards it does not list.</param>
        public static CurriculumLinkedTopics Resolve(int boardId, bool boardIsMaster, IReadOnlyCollection<int> topicIds,
            IEnumerable<CurriculumLinkEdge> links, CurriculumLinkScope scope, IReadOnlyList<int> preferredBoardOrder,
            IReadOnlyDictionary<int, int> boardListOrder)
        {
            ArgumentNullException.ThrowIfNull(topicIds);
            ArgumentNullException.ThrowIfNull(links);
            ArgumentNullException.ThrowIfNull(scope);
            ArgumentNullException.ThrowIfNull(preferredBoardOrder);
            ArgumentNullException.ThrowIfNull(boardListOrder);

            List<int> own = [.. topicIds.Distinct()];
            HashSet<int> ownSet = [.. own];
            var edges = links.ToList();

            // The GAMA topics the list is about: the board's own when it is GAMA, else those its topics link to.
            HashSet<int> exactMasters = [.. edges.Where(t => t.Kind == CurriculumLinkKind.Exact && ownSet.Contains(t.TopicId)).Select(t => t.MasterTopicId)];
            HashSet<int> anyMasters = [.. edges.Where(t => ownSet.Contains(t.TopicId)).Select(t => t.MasterTopicId)];
            if (boardIsMaster)
            {
                exactMasters = ownSet;
                anyMasters = ownSet;
            }

            List<int> master = !boardIsMaster && scope != CurriculumLinkScope.Own ? [.. exactMasters.Order()] : [];

            List<CurriculumBoardTopics> others = [];
            if (scope == CurriculumLinkScope.All || (boardIsMaster && scope != CurriculumLinkScope.Own))
            {
                others = [.. edges
                    .Where(t => t.Kind == CurriculumLinkKind.Exact && exactMasters.Contains(t.MasterTopicId) && t.TopicBoardId != boardId)
                    .GroupBy(t => t.TopicBoardId)
                    .OrderBy(t => Rank(t.Key, preferredBoardOrder, boardListOrder))
                    .ThenBy(t => t.Key)
                    .Select(t => new CurriculumBoardTopics(t.Key, [.. t.Select(e => e.TopicId).Distinct().Order()]))];
            }

            // Related: reached through GAMA with a partial link on the way, and not already listed.
            HashSet<int> listed = [.. own, .. master, .. others.SelectMany(t => t.TopicIds)];
            HashSet<int> related = [];
            foreach (var edge in edges.Where(t => anyMasters.Contains(t.MasterTopicId)))
            {
                var partialToMaster = !boardIsMaster && !exactMasters.Contains(edge.MasterTopicId);
                if (!boardIsMaster && !listed.Contains(edge.MasterTopicId) && partialToMaster)
                {
                    _ = related.Add(edge.MasterTopicId);
                }

                if (edge.TopicBoardId != boardId && (partialToMaster || edge.Kind == CurriculumLinkKind.Partial) && !listed.Contains(edge.TopicId))
                {
                    _ = related.Add(edge.TopicId);
                }
            }

            return new(own, master, others, [.. related.Order()]);
        }

        private static (int, int) Rank(int boardId, IReadOnlyList<int> preferred, IReadOnlyDictionary<int, int> listOrder)
        {
            for (var i = 0; i < preferred.Count; i++)
            {
                if (preferred[i] == boardId)
                {
                    return (0, i);
                }
            }

            return (1, listOrder.TryGetValue(boardId, out var order) ? order : int.MaxValue);
        }
    }
}
