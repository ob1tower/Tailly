using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Configurations.Entities;

public class AccountDeletionTokenConfiguration : IEntityTypeConfiguration<AccountDeletionTokenEntity>
{
    public void Configure(EntityTypeBuilder<AccountDeletionTokenEntity> builder)
    {
        builder.HasKey(x => x.Token);

        builder.Property(x => x.Token)
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(x => x.UserId)
               .IsRequired();

        builder.Property(x => x.ExpiresAt)
               .IsRequired();


        builder.Property(x => x.RoleId)
               .IsRequired();

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}