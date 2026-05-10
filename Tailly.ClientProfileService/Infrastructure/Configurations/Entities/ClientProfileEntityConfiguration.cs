using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ClientProfileService.Core.Entities;

namespace Tailly.ClientProfileService.Infrastructure.Configurations.Entities;

public class ClientProfileEntityConfiguration : IEntityTypeConfiguration<ClientProfileEntity>
{
    public void Configure(EntityTypeBuilder<ClientProfileEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
               .IsRequired();

        builder.HasIndex(x => x.UserId)
               .IsUnique();

        builder.Property(x => x.FirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.LastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.MiddleName)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.Phone)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.City)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.CityId)
               .HasMaxLength(50)
               .IsRequired(false);

        builder.HasIndex(x => x.CityId);

        builder.Property(x => x.District)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.AvatarUrl)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.HasMany(x => x.Pets)
               .WithOne(x => x.ClientProfile)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}