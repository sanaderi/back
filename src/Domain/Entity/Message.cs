namespace GamaEdtech.Domain.Entity
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Domain.Entity.Identity;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    [Table(nameof(Message))]
    public class Message : IEntity<Message, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        [Column(nameof(SenderId), DataType.Int)]
        [Required]
        public int SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        [Column(nameof(ReceiverId), DataType.Int)]
        [Required]
        public int ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        [Column(nameof(Body), DataType.UnicodeMaxString)]
        public string? Body { get; set; }

        [Column(nameof(IsRead), DataType.Boolean)]
        public bool IsRead { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<Message> builder)
        {
            _ = builder.HasOne(t => t.Sender).WithMany().HasForeignKey(t => t.SenderId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne(t => t.Receiver).WithMany().HasForeignKey(t => t.ReceiverId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
