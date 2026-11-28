namespace GamaEdtech.Application.Interface
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Curriculum;

    /// <summary>
    /// Phase 2 of the curriculum model: fills the <c>Curriculum*</c> tables from this backend's own copy of the lookup lists
    /// (<c>LegacyTaxonomyItems</c>, <c>LegacyTaxonomyRelations</c>) and the school boards, by hand (<c>CopyCurriculum</c> in
    /// /hangfire, or the admin's button). Rerunnable: rows are matched by their old id, never deleted because the source
    /// lost them, and what can't be placed without guessing goes to the review list. Refused (<c>notNative</c>) once
    /// <c>Migration:WriteRoutes:Curriculum</c> is <c>Native</c>: the model is then edited here only. See
    /// docs/business/curriculum.md, "The copy".
    /// </summary>
    [Injectable]
    public interface ICurriculumCopyService
    {
        Task<ResultData<CurriculumCopyResultDto>> CopyAsync();

        /// <summary>Queues <see cref="CopyAsync"/> to run now in the background (the admin's button).</summary>
        Task<ResultData<bool>> StartCopyAsync();

        /// <summary>Each copied list against its source, and how much of the content finds its place in the model.</summary>
        Task<ResultData<CurriculumCheckDto>> CheckAsync();

        Task<ResultData<IReadOnlyList<CurriculumCopyIssueDto>>> GetIssuesAsync(string? kind);
    }
}
