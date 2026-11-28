namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>How the filter panel draws a filter. Stored: never renumber.</summary>
    public sealed class CurriculumFilterStyle : Enumeration<CurriculumFilterStyle, byte>
    {
        /// <summary>A dropdown.</summary>
        [Display]
        public static readonly CurriculumFilterStyle Select = new(nameof(Select), 0);

        /// <summary>A row of chips.</summary>
        [Display]
        public static readonly CurriculumFilterStyle Chips = new(nameof(Chips), 1);

        public CurriculumFilterStyle()
        {
        }

        public CurriculumFilterStyle(string name, byte value)
            : base(name, value)
        {
        }
    }
}
