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
            .HasMaxLength(3000);

        builder.Property(x => x.ExperienceLabel)
            .HasMaxLength(200);

        builder.Property(x => x.HousingType)
            .IsRequired();

        builder.Property(x => x.HasChildrenUnderTen)
            .IsRequired();

        builder.HasOne(x => x.Specialist)
            .WithOne(x => x.Details)
            .HasForeignKey<DetailsEntity>(x => x.SpecialistId);
    }
}
