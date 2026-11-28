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
    /// A kind of file a paper comes with: question paper, mark scheme, examiner report, insert, audio, confidential
    /// instructions. Replaces the website's labels by type id (IN, SF, CI, PM...).
    /// </summary>
    [Table(nameof(CurriculumDocumentType))]
    public class CurriculumDocumentType : IEntity<CurriculumDocumentType, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item's id (an extra file type), or null for the question paper and mark scheme, which gama-api has as columns.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        /// <summary>In file names and codes (qp, ms, in, sf).</summary>
        [Column(nameof(Code), DataType.String)]
        [StringLength(20)]
        [Required]
        public string? Code { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(100)]
        [Required]
        public string? Name { get; set; }

        [Column(nameof(ShortLabel), DataType.UnicodeString)]
        [StringLength(20)]
        public string? ShortLabel { get; set; }

        /// <summary>A full icon name (mdi-file-document).</summary>
        [Column(nameof(Icon), DataType.String)]
        [StringLength(100)]
        public string? Icon { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumDocumentType> builder)
        {
            _ = builder.HasIndex(t => t.Code).IsUnique(true);
            _ = builder.HasIndex(t => t.LegacyId).IsUnique(true).HasFilter("[LegacyId] IS NOT NULL");
        }
    }
}
