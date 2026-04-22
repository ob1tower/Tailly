using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Applications;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Applications;

public class SpecialistApplicationConfiguration : IEntityTypeConfiguration<SpecialistApplicationEntity>
{
    public void Configure(EntityTypeBuilder<SpecialistApplicationEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(x => x.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Phone)
               .HasMaxLength(20);

        builder.Property(x => x.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.About)
               .IsRequired()
               .HasMaxLength(2000);

        builder.Property(x => x.ServicesWanted)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(x => x.Status);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Email);
    }
}