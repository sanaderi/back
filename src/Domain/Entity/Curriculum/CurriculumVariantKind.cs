namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// A way a board's papers vary within one series: Cambridge's zone, IB's time zone, CBSE's set, Pakistan's group. A
    /// board can have several; a board with none shows no variant filter.
    /// </summary>
    [Table(nameof(CurriculumVariantKind))]
    public class CurriculumVariantKind : IEntity<CurriculumVariantKind, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        /// <summary>Its filter key after <c>variant.</c> (zone, tz, set).</summary>
        [Column(nameof(Key), DataType.String)]
        [StringLength(30)]
        [Required]
        public string? Key { get; set; }

        [Column(nameof(Label), DataType.UnicodeString)]
        [StringLength(100)]
        [Required]
        public string? Label { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumVariantKind> builder)
        {
            _ = builder.HasIndex(t => new { t.BoardId, t.Key }).IsUnique(true);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
