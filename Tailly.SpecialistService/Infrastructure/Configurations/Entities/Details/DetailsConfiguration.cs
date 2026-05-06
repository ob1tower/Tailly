using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class DetailsConfiguration : IEntityTypeConfiguration<DetailsEntity>
{
    public void Configure(EntityTypeBuilder<DetailsEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.About)
               .HasMaxLength(2000)
               .IsRequired();

        builder.Property(x => x.HousingType)
               .IsRequired();

        builder.Property(x => x.HasChildrenUnderTen)
               .IsRequired();
    }
}