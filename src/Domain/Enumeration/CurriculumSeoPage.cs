namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>A kind of page whose title and description come from an SEO template. Stored: never renumber.</summary>
    public sealed class CurriculumSeoPage : Enumeration<CurriculumSeoPage, byte>
    {
        /// <summary>The past papers tab of the search.</summary>
        [Display]
        public static readonly CurriculumSeoPage PastPaperList = new(nameof(PastPaperList), 0);

        /// <summary>The study materials tab.</summary>
        [Display]
        public static readonly CurriculumSeoPage StudyMaterialList = new(nameof(StudyMaterialList), 1);

        /// <summary>The Exam Hub tab.</summary>
        [Display]
        public static readonly CurriculumSeoPage ExamList = new(nameof(ExamList), 2);

        /// <summary>The tutorials tab.</summary>
        [Display]
        public static readonly CurriculumSeoPage TutorialList = new(nameof(TutorialList), 3);

        /// <summary>A past paper's page.</summary>
        [Display]
        public static readonly CurriculumSeoPage PastPaper = new(nameof(PastPaper), 4);

        /// <summary>An exam's page.</summary>
        [Display]
        public static readonly CurriculumSeoPage Exam = new(nameof(Exam), 5);

        /// <summary>A tutorial's page.</summary>
        [Display]
        public static readonly CurriculumSeoPage Tutorial = new(nameof(Tutorial), 6);

        /// <summary>A study material's page (a resource that is not a past paper).</summary>
        [Display]
        public static readonly CurriculumSeoPage StudyMaterial = new(nameof(StudyMaterial), 7);

        /// <summary>A board's landing page (<c>/boards/cambridge</c>, phase 4).</summary>
        [Display]
        public static readonly CurriculumSeoPage BoardPage = new(nameof(BoardPage), 8);

        /// <summary>A qualification's landing page (<c>/boards/cambridge/igcse</c>, phase 4).</summary>
        [Display]
        public static readonly CurriculumSeoPage QualificationPage = new(nameof(QualificationPage), 9);

        /// <summary>A syllabus's landing page (<c>/boards/cambridge/igcse/0580</c>, phase 4).</summary>
        [Display]
        public static readonly CurriculumSeoPage SyllabusPage = new(nameof(SyllabusPage), 10);

        public CurriculumSeoPage()
        {
        }

        public CurriculumSeoPage(string name, byte value)
            : base(name, value)
        {
        }
    }
}
