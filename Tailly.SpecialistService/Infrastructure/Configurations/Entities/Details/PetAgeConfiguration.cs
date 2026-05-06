using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class PetAgeConfiguration : IEntityTypeConfiguration<PetAgeEntity>
{
    public void Configure(EntityTypeBuilder<PetAgeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Details)
               .WithMany(x => x.PetAges)
               .HasForeignKey(x => x.DetailsId);
    }
}