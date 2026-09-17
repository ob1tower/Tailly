using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Configurations.Entities;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserRoles)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
               .WithMany(x => x.UserRoles)
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.SoftDeletedAt)
               .IsRequired(false);

        builder.Property(x => x.RestoreUntil)
               .IsRequired(false);

        builder.Property(x => x.IsBlocked)
               .IsRequired();

        builder.Property(x => x.IsPermanentBlock)
               .IsRequired();

        builder.Property(x => x.BlockedUntil)
               .IsRequired(false);

        builder.Property(x => x.BlockReason)
               .HasMaxLength(500)
               .IsRequired(false);
    }
}