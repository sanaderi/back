namespace GamaEdtech.Domain.Entity.Identity
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using System.Diagnostics.CodeAnalysis;

    [Table(nameof(ApplicationUserPasskey))]
    public class ApplicationUserPasskey : IdentityUserPasskey<long>, IEntity<ApplicationUserPasskey, long>
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(CredentialId), TypeName = "varbinary(1024)")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public override byte[] CredentialId { get; set; }

        [Column(nameof(Data), TypeName = "nvarchar(max)")]
        [Required]
        public override IdentityPasskeyData Data { get; set; }

        [Column(nameof(UserId), DataType.Long)]
        [Required]
        public override long UserId { get; set; }
        public ApplicationUser? User { get; set; }

        long IIdentifiable<long>.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Configure([NotNull] EntityTypeBuilder<ApplicationUserPasskey> builder)
        {
            _ = builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            _ = builder.OwnsOne(t => t.Data, t => t.ToJson());
        }
    }
}
