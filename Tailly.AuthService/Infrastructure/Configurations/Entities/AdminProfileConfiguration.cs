using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Configurations.Entities;

public class AdminProfileConfiguration : IEntityTypeConfiguration<AdminProfileEntity>
{
    public void Configure(EntityTypeBuilder<AdminProfileEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
               .IsRequired();

        builder.Property(x => x.BirthDate)
               .IsRequired(false);

        builder.Property(x => x.Phone)
               .HasMaxLength(32)
               .IsRequired(false);

        builder.Property(x => x.Department)
               .IsRequired(false);

        builder.Property(x => x.LastLoginAt)
               .IsRequired(false);

        builder.Property(x => x.PasswordAttemptsLockUntil)
               .IsRequired(false);

        builder.Property(x => x.FailedPasswordAttempts)
               .IsRequired();

        builder.HasIndex(x => x.UserId)
               .IsUnique();

        builder.HasOne(x => x.User)
               .WithOne(u => u.AdminProfile)
               .HasForeignKey<AdminProfileEntity>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}