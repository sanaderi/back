namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;

    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// A row the curriculum copy could not place without guessing (a grade under two boards, a link between two non-GAMA
    /// topics, a school board with no curriculum board...). Each run replaces the list; an admin fixes the source or
    /// enters the row by hand.
    /// </summary>
    [Table(nameof(CurriculumCopyIssue))]
    public class CurriculumCopyIssue : IEntity<CurriculumCopyIssue, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        /// <summary>What was being copied: board, grade, subject, topic, paper, variant, relation, school.</summary>
        [Column(nameof(Kind), DataType.String)]
        [StringLength(30)]
        [Required]
        public string? Kind { get; set; }

        /// <summary>The source row's id.</summary>
        [Column(nameof(SourceId), DataType.Long)]
        public long? SourceId { get; set; }

        [Column(nameof(Message), DataType.UnicodeString)]
        [StringLength(500)]
        [Required]
        public string? Message { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumCopyIssue> builder) => _ = builder.HasIndex(t => new { t.Kind, t.SourceId });
    }
}
