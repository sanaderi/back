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
    /// Where another linked board's items come in a board's lists, after its own and GAMA's (Cambridge: Edexcel, AQA,
    /// OCR...). Boards with no row come after these, by their own list order.
    /// </summary>
    [Table(nameof(CurriculumBoardLinkOrder))]
    public class CurriculumBoardLinkOrder : IEntity<CurriculumBoardLinkOrder, int>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        [Column(nameof(OtherBoardId), DataType.Int)]
        [Required]
        public int OtherBoardId { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumBoardLinkOrder> builder)
        {
            _ = builder.HasIndex(t => new { t.BoardId, t.OtherBoardId }).IsUnique(true);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.OtherBoardId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
