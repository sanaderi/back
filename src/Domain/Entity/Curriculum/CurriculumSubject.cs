namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;

    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// A subject across boards ("Mathematics"): what Cambridge 0580, Edexcel 4MA1 and IB Maths AA have in common, so a
    /// student can move between boards and the AI and search know they are the same subject.
    /// </summary>
    [Table(nameof(CurriculumSubject))]
    public class CurriculumSubject : IEntity<CurriculumSubject, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(Slug), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? Slug { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(200)]
        [Required]
        public string? Name { get; set; }

        [Column(nameof(Picture), DataType.String)]
        [StringLength(300)]
        public string? Picture { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumSubject> builder) => _ = builder.HasIndex(t => t.Slug).IsUnique(true);
    }
}
