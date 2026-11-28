namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// One filter a board's panel shows (level, subject, year, series, paper, variant, tier, language), with the board's own
    /// label for it ("Exam" for NSC's series), its style and place. A filter with no row, or not enabled, is not shown.
    /// </summary>
    [Table(nameof(CurriculumBoardFilter))]
    public class CurriculumBoardFilter : IEntity<CurriculumBoardFilter, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        /// <summary>One of <c>CurriculumFilterKeys</c>.</summary>
        [Column(nameof(Key), DataType.String)]
        [StringLength(30)]
        [Required]
        public string? Key { get; set; }

        [Column(nameof(Label), DataType.UnicodeString)]
        [StringLength(100)]
        [Required]
        public string? Label { get; set; }

        [Column(nameof(Style), DataType.Byte)]
        [Required]
        public CurriculumFilterStyle Style { get; set; } = CurriculumFilterStyle.Select;

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        [Column(nameof(IsEnabled), DataType.Boolean)]
        [Required]
        public bool IsEnabled { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumBoardFilter> builder)
        {
            _ = builder.OwnEnumeration<CurriculumBoardFilter, CurriculumFilterStyle, byte>(t => t.Style);
            _ = builder.HasIndex(t => new { t.BoardId, t.Key }).IsUnique(true);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
