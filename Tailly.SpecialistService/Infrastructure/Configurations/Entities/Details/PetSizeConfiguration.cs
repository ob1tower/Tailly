using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class PetSizeConfiguration : IEntityTypeConfiguration<PetSizeEntity>
{
    public void Configure(EntityTypeBuilder<PetSizeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Details)
               .WithMany(x => x.PetSizes)
               .HasForeignKey(x => x.DetailsId);
    }
}