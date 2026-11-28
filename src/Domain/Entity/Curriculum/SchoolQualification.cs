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
    /// A board (and, when known, a qualification) a school teaches. Replaces <c>SchoolBoards</c>, which point at the native
    /// <c>Boards</c> list that has nothing to do with the papers: copied 1:1 through the native board's code (the lookup
    /// list's board id). Every school page can then link to its boards' papers, and every board page to its schools.
    /// </summary>
    [Table(nameof(SchoolQualification))]
    public class SchoolQualification : IEntity<SchoolQualification, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        [Column(nameof(SchoolId), DataType.Long)]
        [Required]
        public long SchoolId { get; set; }

        [Column(nameof(BoardId), DataType.Int)]
        [Required]
        public int BoardId { get; set; }

        /// <summary>Null when only the board is known (every copied row).</summary>
        [Column(nameof(QualificationId), DataType.Int)]
        public int? QualificationId { get; set; }

        /// <summary>The <c>SchoolBoards</c> row it was copied from; null for one added here.</summary>
        [Column(nameof(LegacySchoolBoardId), DataType.Long)]
        public long? LegacySchoolBoardId { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        [Column(nameof(CreationUserId), DataType.Long)]
        public long? CreationUserId { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<SchoolQualification> builder)
        {
            // One row per school, board and qualification; only one with no qualification (SQL Server counts NULLs as equal).
            _ = builder.HasIndex(t => new { t.SchoolId, t.BoardId, t.QualificationId }).IsUnique(true).HasFilter(null);
            _ = builder.HasIndex(t => new { t.BoardId, t.QualificationId });
            _ = builder.HasIndex(t => t.LegacySchoolBoardId).IsUnique(true).HasFilter("[LegacySchoolBoardId] IS NOT NULL");
            _ = builder.HasOne<School>().WithMany().HasForeignKey(t => t.SchoolId).OnDelete(DeleteBehavior.Cascade);
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne<CurriculumQualification>().WithMany().HasForeignKey(t => t.QualificationId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
