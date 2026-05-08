namespace GamaEdtech.Domain.Entity
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Domain.Entity.Identity;

    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    [Table(nameof(Experience))]
    public class Experience : IEntity<Experience, long>, IUserId<int>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        [Column(nameof(UserId), DataType.Int)]
        [Required]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Column(nameof(StartDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Column(nameof(EndDate), DataType.DateTimeOffset)]
        public DateTimeOffset? EndDate { get; set; }

        [Column(nameof(Title), DataType.UnicodeString)]
        [Required]
        [StringLength(100)]
        public string? Title { get; set; }

        [Column(nameof(Description), DataType.UnicodeString)]
        public string? Description { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<Experience> builder)
        {
        }
    }
}
