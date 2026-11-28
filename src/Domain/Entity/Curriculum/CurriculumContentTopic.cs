namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// A topic a past paper, tutorial or exam is about, as a real key: what the content's "+"-joined lookup topic ids mean
    /// in the model (kept filled by <c>LinkCurriculumContent</c>), so lists can find content by topic with an index, and
    /// topics made here (no lookup id) can be used by content too.
    /// </summary>
    [Table(nameof(CurriculumContentTopic))]
    public class CurriculumContentTopic : IEntity<CurriculumContentTopic, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        [Column(nameof(ContentKind), DataType.Byte)]
        [Required]
        public CurriculumContentKind ContentKind { get; set; } = CurriculumContentKind.PastPaper;

        [Column(nameof(ContentId), DataType.Long)]
        [Required]
        public long ContentId { get; set; }

        [Column(nameof(TopicId), DataType.Int)]
        [Required]
        public int TopicId { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumContentTopic> builder)
        {
            _ = builder.OwnEnumeration<CurriculumContentTopic, CurriculumContentKind, byte>(t => t.ContentKind);
            _ = builder.HasIndex(t => new { t.ContentKind, t.ContentId, t.TopicId }).IsUnique(true);
            _ = builder.HasIndex(t => new { t.TopicId, t.ContentKind });
            _ = builder.HasOne<CurriculumTopic>().WithMany().HasForeignKey(t => t.TopicId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
