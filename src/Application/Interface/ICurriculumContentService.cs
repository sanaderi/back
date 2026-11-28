namespace GamaEdtech.Application.Interface
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Curriculum;

    /// <summary>
    /// Phase 5 of the curriculum model: keeps past papers, tutorials and exams placed in the model with real keys (board,
    /// qualification, syllabus, and for a paper its paper, variant and session; topics in <c>CurriculumContentTopics</c>),
    /// worked out from the lookup ids they carry. <c>LinkCurriculumContent</c> runs every 15 minutes and places what is new
    /// or changed since its last pass. See docs/business/curriculum.md, "The write switch".
    /// </summary>
    [Injectable]
    public interface ICurriculumContentService
    {
        /// <summary>Places up to <paramref name="maxRows"/> new or changed rows of each kind; answers how many.</summary>
        Task<ResultData<int>> LinkAsync(int maxRows = 20000);

        /// <summary>Which of the curriculum's routes are Native, for the website (reads) and the admin (writes).</summary>
        CurriculumStatusDto GetStatus();
    }
}
