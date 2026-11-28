namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>How closely a board's topic matches a GAMA topic (see docs/business/curriculum.md). Stored: never renumber.</summary>
    public sealed class CurriculumLinkKind : Enumeration<CurriculumLinkKind, byte>
    {
        /// <summary>The same content: lists bring in each other's items.</summary>
        [Display]
        public static readonly CurriculumLinkKind Exact = new(nameof(Exact), 0);

        /// <summary>Overlapping content: shown only as related on a detail page, never in a list.</summary>
        [Display]
        public static readonly CurriculumLinkKind Partial = new(nameof(Partial), 1);

        public CurriculumLinkKind()
        {
        }

        public CurriculumLinkKind(string name, byte value)
            : base(name, value)
        {
        }
    }
}
