using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ClientProfileService.Core.Entities;

namespace Tailly.ClientProfileService.Infrastructure.Configurations.Entities;

public class BreedEntityConfiguration : IEntityTypeConfiguration<BreedEntity>
{
    public void Configure(EntityTypeBuilder<BreedEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
               .IsRequired();

        builder.Property(x => x.Title)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => new { x.Type, x.Title });
    }
}