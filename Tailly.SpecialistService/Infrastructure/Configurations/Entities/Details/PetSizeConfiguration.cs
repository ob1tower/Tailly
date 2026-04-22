using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class PetSizeConfiguration : IEntityTypeConfiguration<PetSizeEntity>
{
    public void Configure(EntityTypeBuilder<PetSizeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PetSize)
            .IsRequired();

        builder.HasIndex(x => new { x.SpecialistId, x.PetSize })
            .IsUnique();
    }
}