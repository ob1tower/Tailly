using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Configurations.Entities;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
               .HasMaxLength(256)
               .IsRequired();
        
        builder.HasIndex(x => x.Email)
               .IsUnique();

        builder.Property(x => x.PasswordHash)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.EmailConfirmed)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.IsBlocked)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.BlockReason)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.IsPermanentBlock)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.BlockedUntil)
               .IsRequired(false);

        builder.Property(x => x.SoftDeletedAt)
               .IsRequired(false);

        builder.Property(x => x.RestoreUntil)
               .IsRequired(false);

        builder.Property(x => x.SpecialistSlug)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.FirstName)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.LastName)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.MiddleName)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.HasMany(x => x.UserRoles)
               .WithOne(x => x.User)
               .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.RefreshTokens)
               .WithOne(x => x.User)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AdminProfile)
               .WithOne(ap => ap.User)
               .HasForeignKey<AdminProfileEntity>(ap => ap.UserId)   
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.SpecialistId)
               .IsRequired(false);

        builder.Property(x => x.AdminId)
               .IsRequired(false);
    }
}