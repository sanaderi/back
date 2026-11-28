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
    /// One value of a variant kind: zone 1/2/3, TZ1/TZ2, set 65/2/3.
    /// </summary>
    [Table(nameof(CurriculumVariantOption))]
    public class CurriculumVariantOption : IEntity<CurriculumVariantOption, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item it came from (a variant, 7814-7816); shared by every board on gama-api, so several options can carry the same one.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        [Column(nameof(VariantKindId), DataType.Int)]
        [Required]
        public int VariantKindId { get; set; }

        [Column(nameof(Label), DataType.UnicodeString)]
        [StringLength(50)]
        [Required]
        public string? Label { get; set; }

        /// <summary>What <c>{variant}</c> is in a code (2).</summary>
        [Column(nameof(Code), DataType.String)]
        [StringLength(20)]
        public string? Code { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumVariantOption> builder)
        {
            _ = builder.HasIndex(t => new { t.VariantKindId, t.ListOrder });
            _ = builder.HasIndex(t => t.LegacyId);
            _ = builder.HasOne<CurriculumVariantKind>().WithMany().HasForeignKey(t => t.VariantKindId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
