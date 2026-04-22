using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class PetAgeConfiguration : IEntityTypeConfiguration<PetAgeEntity>
{
    public void Configure(EntityTypeBuilder<PetAgeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PetAge)
            .IsRequired();

        builder.HasIndex(x => new { x.SpecialistId, x.PetAge })
            .IsUnique();
    }
}