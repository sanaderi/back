namespace GamaEdtech.Application.Interface
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    /// <summary>
    /// What the curriculum model answers: a board's filter panel, the topics a tab brings in through the links, and a page's
    /// SEO title and description. In phase 1 only the admin's previews use it; the search and the pages switch to it through
    /// <c>Migration:Routes:Curriculum</c> (Shadow, then Native) in phase 3. See docs/business/curriculum.md.
    /// </summary>
    [Injectable]
    public interface ICurriculumService
    {
        /// <summary>The board's enabled filters in order, with their labels, styles and the options the model knows for the
        /// chosen qualification and syllabus.</summary>
        Task<ResultData<IReadOnlyList<CurriculumPanelFilterDto>>> GetFilterPanelAsync(int boardId, int? qualificationId, int? syllabusId);

        /// <summary>The topics a board's tab lists for one of its topics: own, then GAMA's, then the other boards', with the
        /// partial matches apart.</summary>
        Task<ResultData<CurriculumLinkedTopicsDto>> GetLinkedTopicsAsync(int boardId, int topicId, CurriculumContentTab tab);

        /// <summary>A page's title and description from the board's template, else the default, filled with
        /// <paramref name="values"/> (see <c>CurriculumSeoDefaults</c> for the names).</summary>
        Task<ResultData<CurriculumSeoResultDto>> GetSeoAsync(int? boardId, CurriculumSeoPage page, IReadOnlyDictionary<string, string?> values);
    }
}
