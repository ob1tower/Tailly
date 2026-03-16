using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Entities;

namespace Tailly.AuthService.Configurations.Entities;

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

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
