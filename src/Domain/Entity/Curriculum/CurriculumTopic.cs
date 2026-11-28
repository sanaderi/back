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
    /// A syllabus's topic tree: units (gama-api's seasons) and their chapters, in order. The copied topics become these.
    /// </summary>
    [Table(nameof(CurriculumTopic))]
    public class CurriculumTopic : IEntity<CurriculumTopic, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item's id (a topic), so content tagged with it keeps its topic; null for one added here.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        [Column(nameof(SyllabusId), DataType.Int)]
        [Required]
        public int SyllabusId { get; set; }

        /// <summary>The unit a chapter is in; null for a unit.</summary>
        [Column(nameof(ParentTopicId), DataType.Int)]
        public int? ParentTopicId { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(500)]
        [Required]
        public string? Name { get; set; }

        /// <summary>Heads a section of the syllabus.</summary>
        [Column(nameof(IsUnit), DataType.Boolean)]
        [Required]
        public bool IsUnit { get; set; }

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

        public void Configure([NotNull] EntityTypeBuilder<CurriculumTopic> builder)
        {
            _ = builder.HasIndex(t => new { t.SyllabusId, t.ParentTopicId, t.ListOrder });
            _ = builder.HasIndex(t => t.LegacyId).IsUnique(true).HasFilter("[LegacyId] IS NOT NULL");
            _ = builder.HasOne<CurriculumSyllabus>().WithMany().HasForeignKey(t => t.SyllabusId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne<CurriculumTopic>().WithMany().HasForeignKey(t => t.ParentTopicId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
