namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>How far a board's tab follows topic links: its own items, then GAMA's, then the other linked boards'. Stored: never renumber.</summary>
    public sealed class CurriculumLinkScope : Enumeration<CurriculumLinkScope, byte>
    {
        /// <summary>Only the board's own items.</summary>
        [Display]
        public static readonly CurriculumLinkScope Own = new(nameof(Own), 0);

        /// <summary>The board's own items, then GAMA's.</summary>
        [Display]
        public static readonly CurriculumLinkScope OwnAndMaster = new(nameof(OwnAndMaster), 1);

        /// <summary>The board's own items, then GAMA's, then the other linked boards'.</summary>
        [Display]
        public static readonly CurriculumLinkScope All = new(nameof(All), 2);

        public CurriculumLinkScope()
        {
        }

        public CurriculumLinkScope(string name, byte value)
            : base(name, value)
        {
        }
    }
}
