namespace GamaEdtech.Domain.Entity
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Common.Localization;
    using GamaEdtech.Domain.Entity.Identity;

    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    [Table(nameof(ContentLocalization))]
    public class ContentLocalization : VersionableEntity<ApplicationUser, int, int?>, IEntity<ContentLocalization, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        [Column(nameof(ContentId), DataType.Long)]
        [Required]
        public long ContentId { get; set; }

        [Column(nameof(ContentType), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? ContentType { get; set; }

        [Column(nameof(Name), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? Name { get; set; }

        [Column(nameof(Value), DataType.UnicodeMaxString)]
        [Required]
        public string? Value { get; set; }

        [Column(nameof(LanguageId), DataType.Int)]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<ContentLocalization> builder) => _ = builder.HasIndex(t => new { t.LanguageId, t.ContentType, t.ContentId, t.Name }).IsUnique(true);
    }
}
