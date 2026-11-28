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
    /// A reviewed link between a GAMA topic and another board's topic. Links only ever go through GAMA, so Cambridge reaches
    /// Edexcel in one hop (Cambridge - GAMA - Edexcel), never through an endless chain. Only <see cref="CurriculumLinkKind.Exact"/>
    /// links bring content into lists. Approving a topic match (<c>TopicMatches</c>) makes one.
    /// </summary>
    [Table(nameof(CurriculumTopicLink))]
    public class CurriculumTopicLink : IEntity<CurriculumTopicLink, int>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The GAMA topic.</summary>
        [Column(nameof(MasterTopicId), DataType.Int)]
        [Required]
        public int MasterTopicId { get; set; }

        /// <summary>The other board's topic.</summary>
        [Column(nameof(TopicId), DataType.Int)]
        [Required]
        public int TopicId { get; set; }

        [Column(nameof(Kind), DataType.Byte)]
        [Required]
        public CurriculumLinkKind Kind { get; set; } = CurriculumLinkKind.Exact;

        /// <summary>The copied link it came from (<c>LegacyTaxonomyRelations</c>); null for one made here.</summary>
        [Column(nameof(LegacyRelationId), DataType.Int)]
        public int? LegacyRelationId { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        [Column(nameof(CreationUserId), DataType.Long)]
        public long? CreationUserId { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumTopicLink> builder)
        {
            _ = builder.OwnEnumeration<CurriculumTopicLink, CurriculumLinkKind, byte>(t => t.Kind);
            _ = builder.HasIndex(t => new { t.MasterTopicId, t.TopicId }).IsUnique(true);
            _ = builder.HasIndex(t => t.TopicId);
            _ = builder.HasIndex(t => t.LegacyRelationId).IsUnique(true).HasFilter("[LegacyRelationId] IS NOT NULL");
            _ = builder.HasOne<CurriculumTopic>().WithMany().HasForeignKey(t => t.MasterTopicId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne<CurriculumTopic>().WithMany().HasForeignKey(t => t.TopicId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
