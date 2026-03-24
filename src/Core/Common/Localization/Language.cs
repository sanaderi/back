namespace GamaEdtech.Common.Localization
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;

    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    [Table(nameof(Language))]
    public class Language : IEntity<Language, int>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(100)]
        [Required]
        public string? Name { get; set; }

        [Column(nameof(Code), DataType.String)]
        [StringLength(20)]
        [Required]
        public string? Code { get; set; }

        [Column(nameof(IsEnable), DataType.Boolean)]
        public bool IsEnable { get; set; }

        [Column(nameof(Icon), DataType.UnicodeMaxString)]
        public string? Icon { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<Language> builder) => _ = builder.HasIndex(t => t.Code).IsUnique(true);
    }
}
