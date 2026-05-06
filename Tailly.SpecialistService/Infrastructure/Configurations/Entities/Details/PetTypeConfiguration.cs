using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class PetTypeConfiguration : IEntityTypeConfiguration<PetTypeEntity>
{
    public void Configure(EntityTypeBuilder<PetTypeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Details)
               .WithMany(x => x.PetTypes)
               .HasForeignKey(x => x.DetailsId);
    }
}