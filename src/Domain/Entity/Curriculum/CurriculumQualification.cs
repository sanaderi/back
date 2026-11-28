namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Common.Localization;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// What a board awards, or the level its exams are sat at: IGCSE, AS &amp; A Level, IB Diploma, CSEE (Form 4), Class 12.
    /// The copied grades become these.
    /// </summary>
    [Table(nameof(CurriculumQualification))]
    public class CurriculumQualification : IEntity<CurriculumQualification, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item's id (a grade), so old links keep working; null for one added here.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        [Column(nameof(Slug), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? Slug { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(200)]
        [Required]
        public string? Name { get; set; }

        [Column(nameof(ShortName), DataType.UnicodeString)]
        [StringLength(50)]
        public string? ShortName { get; set; }

        /// <summary>Overrides the board's language.</summary>
        [Column(nameof(LanguageId), DataType.Int)]
        public int? LanguageId { get; set; }

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

        public void Configure([NotNull] EntityTypeBuilder<CurriculumQualification> builder)
        {
            _ = builder.HasIndex(t => new { t.BoardId, t.Slug }).IsUnique(true);
            _ = builder.HasIndex(t => t.LegacyId).IsUnique(true).HasFilter("[LegacyId] IS NOT NULL");
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne<Language>().WithMany().HasForeignKey(t => t.LanguageId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
