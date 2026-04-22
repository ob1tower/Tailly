using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Details;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;

public class AdvantageConfiguration : IEntityTypeConfiguration<AdvantageEntity>
{
    public void Configure(EntityTypeBuilder<AdvantageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();
    }
}