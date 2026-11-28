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
    /// How a kind of page's title and description are written (<see cref="CurriculumSeoPage"/>), so the backend writes them
    /// for the website, the sitemaps and the MCP alike, and an admin can change them without a deploy. A row with no board is
    /// the default; a board's own row wins over it. See <c>CurriculumTemplate</c> for the placeholders, the optional parts and the
    /// <c>||</c> alternatives (the first whose values are all there is used, so the last can be a plain fallback).
    /// </summary>
    [Table(nameof(CurriculumSeoTemplate))]
    public class CurriculumSeoTemplate : IEntity<CurriculumSeoTemplate, int>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>Null for the default of every board.</summary>
        [Column(nameof(BoardId), DataType.Int)]
        public int? BoardId { get; set; }

        [Column(nameof(Page), DataType.Byte)]
        [Required]
        public CurriculumSeoPage Page { get; set; } = CurriculumSeoPage.PastPaperList;

        [Column(nameof(TitleTemplate), DataType.UnicodeString)]
        [StringLength(500)]
        [Required]
        public string? TitleTemplate { get; set; }

        [Column(nameof(DescriptionTemplate), DataType.UnicodeString)]
        [StringLength(1000)]
        [Required]
        public string? DescriptionTemplate { get; set; }

        [Column(nameof(LastModifyDate), DataType.DateTimeOffset)]
        public DateTimeOffset? LastModifyDate { get; set; }

        [Column(nameof(LastModifyUserId), DataType.Long)]
        public long? LastModifyUserId { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumSeoTemplate> builder)
        {
            _ = builder.OwnEnumeration<CurriculumSeoTemplate, CurriculumSeoPage, byte>(t => t.Page);
            _ = builder.HasIndex(t => new { t.BoardId, t.Page }).IsUnique(true).HasFilter(null);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
