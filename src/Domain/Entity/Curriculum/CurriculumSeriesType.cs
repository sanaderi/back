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
    /// When a board's exams are sat: May/June (code M/J, file letter s), Oct/Nov, NSC's November, Konkur's Tir. A paper's
    /// series is one of these plus its year.
    /// </summary>
    [Table(nameof(CurriculumSeriesType))]
    public class CurriculumSeriesType : IEntity<CurriculumSeriesType, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        /// <summary>Only for this qualification (Cambridge IGCSE has Feb/March, Pre-U does not); null for all of the board's.</summary>
        [Column(nameof(QualificationId), DataType.Int)]
        public int? QualificationId { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(100)]
        [Required]
        public string? Name { get; set; }

        /// <summary>Short code in a paper's code (<c>{seriesCode}</c>: M/J).</summary>
        [Column(nameof(Code), DataType.UnicodeString)]
        [StringLength(20)]
        public string? Code { get; set; }

        /// <summary>The letter in the board's file names (s for May/June).</summary>
        [Column(nameof(FileCode), DataType.String)]
        [StringLength(5)]
        public string? FileCode { get; set; }

        /// <summary>The months it covers, comma separated (5,6): what the copied papers' month maps from.</summary>
        [Column(nameof(Months), DataType.String)]
        [StringLength(40)]
        public string? Months { get; set; }

        /// <summary>A resit or second sitting (2nd Annual, Supplementary).</summary>
        [Column(nameof(IsSupplementary), DataType.Boolean)]
        [Required]
        public bool IsSupplementary { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumSeriesType> builder)
        {
            _ = builder.HasIndex(t => new { t.BoardId, t.QualificationId, t.ListOrder });
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
            _ = builder.HasOne<CurriculumQualification>().WithMany().HasForeignKey(t => t.QualificationId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
