namespace GamaEdtech.Application.Interface
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using System.Text.Json.Nodes;

    /// <summary>
    /// What the website reads from the curriculum model (phase 3): boards and what is under them, a board's filter panel with
    /// live counts, SEO titles and descriptions, the lesson tree, schools by board, and the landing pages. Every answer says
    /// whether the website should use it (<c>Migration:Routes:Curriculum.Website</c> Native). See docs/business/curriculum.md.
    /// </summary>
    [Injectable]
    public interface ICurriculumPublicService
    {
        Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetBoardsAsync();

        /// <summary>The levels of a board, the subjects of a level or the topics of a subject (<paramref name="kind"/>:
        /// levels, subjects, topics), by the parent's curriculum id.</summary>
        Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetChildrenAsync(string kind, int parentId);

        /// <summary>The forms' lists by today's ids (<paramref name="type"/>: base = a board's levels, lesson = a level's
        /// subjects, topic = a subject's topics), only the rows that have one, so a form saves the same ids as before.</summary>
        Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetTypeListAsync(string type, int parentLegacyId);

        /// <summary>The panel for a board, and level and subject when chosen (the ids today's search takes), with counts of
        /// the papers (<paramref name="isPaper"/>: past papers, or the other resources).</summary>
        Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicFilterDto>>>> GetFiltersAsync(int boardLegacyId, int? levelLegacyId, int? subjectLegacyId, bool isPaper);

        Task<ResultData<CurriculumAnswerDto<CurriculumSeoResultDto>>> GetSeoAsync(CurriculumSeoPage page, int? boardLegacyId, IReadOnlyDictionary<string, string?> values);

        /// <summary>A past paper's (or study material's) title and description, with every value worked out from the model:
        /// its board's paper code, session words, paper and variant, and file list.</summary>
        Task<ResultData<CurriculumAnswerDto<CurriculumSeoResultDto>>> GetPaperSeoAsync(long paperId);

        /// <summary>A subject's units and chapters with their tutorials, linked ones included (the tutorials tab's scope), in
        /// the shape of today's lesson tree.</summary>
        Task<ResultData<CurriculumAnswerDto<JsonObject>>> GetLessonTreeAsync(int subjectLegacyId);

        Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicSchoolDto>>>> GetSchoolsAsync(int boardId, int? cityId, int page, int pageSize);

        /// <summary>A landing page by slugs: a board, its level, the level's subject.</summary>
        Task<ResultData<CurriculumAnswerDto<CurriculumLandingDto>>> GetLandingAsync(string boardSlug, string? levelSlug, string? subjectSlug);
    }
}
