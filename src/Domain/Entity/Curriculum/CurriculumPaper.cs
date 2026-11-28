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
    /// A component of a syllabus that is sat on its own: "Paper 4" (short label "4"), IB "Paper 1" at HL, NECTA "031/2".
    /// </summary>
    [Table(nameof(CurriculumPaper))]
    public class CurriculumPaper : IEntity<CurriculumPaper, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item it came from (a paper type). gama-api shares one item between subjects, so several papers can carry the same one.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        [Column(nameof(SyllabusId), DataType.Int)]
        [Required]
        public int SyllabusId { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(100)]
        [Required]
        public string? Name { get; set; }

        /// <summary>What the chip shows ("4") and what <c>{paper}</c> is in a code.</summary>
        [Column(nameof(ShortLabel), DataType.UnicodeString)]
        [StringLength(20)]
        [Required]
        public string? ShortLabel { get; set; }

        /// <summary>Core / Extended, SL / HL, Foundation / Higher, when the board has tiers.</summary>
        [Column(nameof(Tier), DataType.UnicodeString)]
        [StringLength(50)]
        public string? Tier { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumPaper> builder)
        {
            _ = builder.HasIndex(t => new { t.SyllabusId, t.ListOrder });
            _ = builder.HasIndex(t => t.LegacyId);
            _ = builder.HasOne<CurriculumSyllabus>().WithMany().HasForeignKey(t => t.SyllabusId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
