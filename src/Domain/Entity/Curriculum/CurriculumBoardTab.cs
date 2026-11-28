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
    /// How far one of a board's content tabs follows topic links (<see cref="CurriculumLinkScope"/>). A tab with no row uses
    /// the default: past papers only the board's own, every other tab its own, then GAMA's, then the other linked boards'
    /// (<c>CurriculumLinkRules.DefaultScope</c>).
    /// </summary>
    [Table(nameof(CurriculumBoardTab))]
    public class CurriculumBoardTab : IEntity<CurriculumBoardTab, int>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        [Column(nameof(Tab), DataType.Byte)]
        [Required]
        public CurriculumContentTab Tab { get; set; } = CurriculumContentTab.PastPapers;

        [Column(nameof(Scope), DataType.Byte)]
        [Required]
        public CurriculumLinkScope Scope { get; set; } = CurriculumLinkScope.Own;

        public void Configure([NotNull] EntityTypeBuilder<CurriculumBoardTab> builder)
        {
            _ = builder.OwnEnumeration<CurriculumBoardTab, CurriculumContentTab, byte>(t => t.Tab);
            _ = builder.OwnEnumeration<CurriculumBoardTab, CurriculumLinkScope, byte>(t => t.Scope);
            _ = builder.HasIndex(t => new { t.BoardId, t.Tab }).IsUnique(true);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
