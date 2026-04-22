using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class PetTypeConfiguration : IEntityTypeConfiguration<PetTypeEntity>
{
    public void Configure(EntityTypeBuilder<PetTypeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PetType)
               .IsRequired();

        builder.HasIndex(x => new { x.SpecialistId, x.PetType })
               .IsUnique();
    }
}