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
    /// A kind of resource that is not a past paper: worksheets, predicted papers, notes, study guides. The copied
    /// classifications that are not papers (gama-api's <c>is_paper</c> false) become these; names in other languages are
    /// <c>ContentLocalizations</c> with the content type <c>CurriculumResourceType</c>.
    /// </summary>
    [Table(nameof(CurriculumResourceType))]
    public class CurriculumResourceType : IEntity<CurriculumResourceType, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied classification's id; null for one added here.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        [Column(nameof(Slug), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? Slug { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(200)]
        [Required]
        public string? Name { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumResourceType> builder)
        {
            _ = builder.HasIndex(t => t.Slug).IsUnique(true);
            _ = builder.HasIndex(t => t.LegacyId).IsUnique(true).HasFilter("[LegacyId] IS NOT NULL");
        }
    }
}
