namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>A content list that follows topic links (a search tab). Stored: never renumber.</summary>
    public sealed class CurriculumContentTab : Enumeration<CurriculumContentTab, byte>
    {
        /// <summary>Past papers (classifications that are papers).</summary>
        [Display]
        public static readonly CurriculumContentTab PastPapers = new(nameof(PastPapers), 0);

        /// <summary>Worksheets, notes and other non-paper resources.</summary>
        [Display]
        public static readonly CurriculumContentTab StudyMaterials = new(nameof(StudyMaterials), 1);

        /// <summary>Exam Hub.</summary>
        [Display]
        public static readonly CurriculumContentTab Exams = new(nameof(Exams), 2);

        /// <summary>Tutorials, and the subject's lesson tree.</summary>
        [Display]
        public static readonly CurriculumContentTab Tutorials = new(nameof(Tutorials), 3);

        public CurriculumContentTab()
        {
        }

        public CurriculumContentTab(string name, byte value)
            : base(name, value)
        {
        }
    }
}
