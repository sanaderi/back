namespace GamaEdtech.Domain.Entity
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Domain.Entity.Identity;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    [Table(nameof(Connection))]
    public class Connection : IEntity<Connection, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Long)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }

        [Column(nameof(SourceUserId), DataType.Long)]
        [Required]
        public long SourceUserId { get; set; }
        public ApplicationUser SourceUser { get; set; }

        [Column(nameof(DestinationUserId), DataType.Long)]
        [Required]
        public long DestinationUserId { get; set; }
        public ApplicationUser DestinationUser { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        [Column(nameof(Status), DataType.Byte)]
        [Required]
        public ConnectionStatus Status { get; set; }

        [Column(nameof(SubscribeToActivityFeed), DataType.Boolean)]
        [Required]
        public bool SubscribeToActivityFeed { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<Connection> builder)
        {
            _ = builder.OwnEnumeration<Connection, ConnectionStatus, byte>(t => t.Status);
            _ = builder.HasIndex(t => new { t.SourceUserId, t.DestinationUserId });

            _ = builder.HasOne(t => t.SourceUser).WithMany().HasForeignKey(t => t.SourceUserId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne(t => t.DestinationUser).WithMany().HasForeignKey(t => t.DestinationUserId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
