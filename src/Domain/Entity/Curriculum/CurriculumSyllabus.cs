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
    /// One subject of a qualification with its board's code: Cambridge IGCSE Mathematics (0580), NECTA CSEE Physics (031).
    /// The copied subjects become these.
    /// </summary>
    [Table(nameof(CurriculumSyllabus))]
    public class CurriculumSyllabus : IEntity<CurriculumSyllabus, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item's id (a subject), so old links keep working; null for one added here.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        [Column(nameof(QualificationId), DataType.Int)]
        [Required]
        public int QualificationId { get; set; }

        /// <summary>The subject across boards.</summary>
        [Column(nameof(SubjectId), DataType.Int)]
        public int? SubjectId { get; set; }

        /// <summary>The board's code (0580, 9709, WMA11, 031), shown and searched.</summary>
        [Column(nameof(Code), DataType.String)]
        [StringLength(30)]
        public string? Code { get; set; }

        [Column(nameof(Slug), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? Slug { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(200)]
        [Required]
        public string? Name { get; set; }

        /// <summary>First exam year of this syllabus, when known.</summary>
        [Column(nameof(YearFrom), DataType.Short)]
        public short? YearFrom { get; set; }

        /// <summary>Last exam year, when it has been replaced.</summary>
        [Column(nameof(YearTo), DataType.Short)]
        public short? YearTo { get; set; }

        [Column(nameof(Picture), DataType.String)]
        [StringLength(300)]
        public string? Picture { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        [Column(nameof(CreationUserId), DataType.Long)]
        public long? CreationUserId { get; set; }

        [Column(nameof(LastModifyDate), DataType.DateTimeOffset)]
        public DateTimeOffset? LastModifyDate { get; set; }

        [Column(nameof(LastModifyUserId), DataType.Long)]
        public long? LastModifyUserId { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumSyllabus> builder)
        {
            _ = builder.HasIndex(t => new { t.QualificationId, t.Slug }).IsUnique(true);
            _ = builder.HasIndex(t => t.Code);
            _ = builder.HasIndex(t => t.LegacyId).IsUnique(true).HasFilter("[LegacyId] IS NOT NULL");
            _ = builder.HasOne<CurriculumQualification>().WithMany().HasForeignKey(t => t.QualificationId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne<CurriculumSubject>().WithMany().HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
