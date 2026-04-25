using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Configurations.Entities;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
               .HasMaxLength(64)
               .IsRequired();

        builder.Property(x => x.Created)
               .IsRequired();

        builder.Property(x => x.Expires)
               .IsRequired();

        builder.Property(x => x.Revoked)
               .IsRequired(false);

        builder.Property(x => x.UserId)
               .IsRequired();

        builder.Property(x => x.RoleId)
               .IsRequired(false);

        builder.HasOne(x => x.User)
               .WithMany(x => x.RefreshTokens)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}