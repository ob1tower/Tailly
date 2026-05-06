using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Gallery;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Gallery;

public class SpecialistGalleryConfiguration : IEntityTypeConfiguration<SpecialistGalleryEntity>
{
    public void Configure(EntityTypeBuilder<SpecialistGalleryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.Alt)
               .HasMaxLength(200);

        builder.Property(x => x.Order)
               .IsRequired();

        builder.HasIndex(x => new { x.SpecialistId, x.Order });
    }
}