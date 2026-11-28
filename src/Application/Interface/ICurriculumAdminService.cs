namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Curriculum;

    /// <summary>
    /// The admin screens of the curriculum model: boards and their profile (filters, tab scopes, order of the other boards,
    /// series types, variant kinds, SEO templates), qualifications, subjects, syllabi, papers, document types, topic trees
    /// and topic links. A row copied from the lookup lists (it has a <c>LegacyId</c>) is refused (<c>notNative</c>) until
    /// <c>Migration:WriteRoutes:Curriculum</c> is <c>Native</c>, because the copy would overwrite the edit; rows made here
    /// and the profile settings (which are not copied) can always be edited. See docs/business/curriculum.md.
    /// </summary>
    [Injectable]
    public interface ICurriculumAdminService
    {
        Task<ResultData<IReadOnlyList<CurriculumBoardDto>>> GetBoardsAsync();
        Task<ResultData<CurriculumBoardDetailDto>> GetBoardAsync(int id);
        Task<ResultData<int>> SaveBoardAsync([NotNull] CurriculumBoardDto board, long userId);
        Task<ResultData<bool>> DeleteBoardAsync(int id);
        Task<ResultData<bool>> SaveBoardFiltersAsync(int boardId, [NotNull] IReadOnlyList<CurriculumBoardFilterDto> filters);
        Task<ResultData<bool>> SaveBoardLinksAsync(int boardId, [NotNull] CurriculumBoardLinksDto links);
        Task<ResultData<bool>> SaveSeriesTypesAsync(int boardId, [NotNull] IReadOnlyList<CurriculumSeriesTypeDto> seriesTypes);
        Task<ResultData<bool>> SaveVariantKindsAsync(int boardId, [NotNull] IReadOnlyList<CurriculumVariantKindDto> variantKinds);

        Task<ResultData<IReadOnlyList<CurriculumQualificationDto>>> GetQualificationsAsync(int boardId);
        Task<ResultData<int>> SaveQualificationAsync([NotNull] CurriculumQualificationDto qualification, long userId);
        Task<ResultData<bool>> DeleteQualificationAsync(int id);

        Task<ResultData<IReadOnlyList<CurriculumSubjectDto>>> GetSubjectsAsync();
        Task<ResultData<int>> SaveSubjectAsync([NotNull] CurriculumSubjectDto subject);
        Task<ResultData<bool>> DeleteSubjectAsync(int id);

        Task<ResultData<IReadOnlyList<CurriculumSyllabusDto>>> GetSyllabiAsync(int qualificationId);
        Task<ResultData<int>> SaveSyllabusAsync([NotNull] CurriculumSyllabusDto syllabus, long userId);
        Task<ResultData<bool>> DeleteSyllabusAsync(int id);
        Task<ResultData<IReadOnlyList<CurriculumPaperDto>>> GetPapersAsync(int syllabusId);
        Task<ResultData<bool>> SavePapersAsync(int syllabusId, [NotNull] IReadOnlyList<CurriculumPaperDto> papers);

        Task<ResultData<IReadOnlyList<CurriculumDocumentTypeDto>>> GetDocumentTypesAsync();
        Task<ResultData<int>> SaveDocumentTypeAsync([NotNull] CurriculumDocumentTypeDto documentType);
        Task<ResultData<bool>> DeleteDocumentTypeAsync(int id);

        /// <summary>A syllabus's topics, units first then their chapters, each in order.</summary>
        Task<ResultData<IReadOnlyList<CurriculumTopicDto>>> GetTopicsAsync(int syllabusId);
        Task<ResultData<int>> SaveTopicAsync([NotNull] CurriculumTopicDto topic, long userId);
        Task<ResultData<bool>> DeleteTopicAsync(int id);

        /// <summary>The links of a topic (a GAMA topic's to every board, another board's to GAMA).</summary>
        Task<ResultData<IReadOnlyList<CurriculumTopicLinkDto>>> GetTopicLinksAsync(int topicId);
        Task<ResultData<int>> AddTopicLinkAsync([NotNull] CurriculumTopicLinkAddDto link);
        Task<ResultData<bool>> DeleteTopicLinkAsync(int id);

        /// <summary>The default SEO templates (<paramref name="boardId"/> null) or a board's own.</summary>
        Task<ResultData<IReadOnlyList<CurriculumSeoTemplateDto>>> GetSeoTemplatesAsync(int? boardId);
        Task<ResultData<int>> SaveSeoTemplateAsync([NotNull] CurriculumSeoTemplateDto template, long userId);

        /// <summary>Removes a board's own template (its pages then use the default); a default cannot be removed.</summary>
        Task<ResultData<bool>> DeleteSeoTemplateAsync(int id);
    }
}
