namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>A kind of content placed in the curriculum model (<c>CurriculumContentTopics</c>). Stored: never renumber.</summary>
    public sealed class CurriculumContentKind : Enumeration<CurriculumContentKind, byte>
    {
        [Display]
        public static readonly CurriculumContentKind PastPaper = new(nameof(PastPaper), 0);

        [Display]
        public static readonly CurriculumContentKind Tutorial = new(nameof(Tutorial), 1);

        [Display]
        public static readonly CurriculumContentKind Exam = new(nameof(Exam), 2);

        public CurriculumContentKind()
        {
        }

        public CurriculumContentKind(string name, byte value)
            : base(name, value)
        {
        }
    }
}
