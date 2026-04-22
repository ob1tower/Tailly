using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Gallery;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Gallery;

public class GalleryConfiguration : IEntityTypeConfiguration<GalleryEntity>
{
    public void Configure(EntityTypeBuilder<GalleryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Alt)
            .HasMaxLength(200);
    }
}