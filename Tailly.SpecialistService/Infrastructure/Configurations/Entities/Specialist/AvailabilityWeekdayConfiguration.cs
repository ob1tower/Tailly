using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Specialist;

public class AvailabilityWeekdayConfiguration : IEntityTypeConfiguration<AvailabilityWeekdayEntity>
{
    public void Configure(EntityTypeBuilder<AvailabilityWeekdayEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Weekday)
            .IsRequired();

        builder.HasOne(x => x.Specialist)
            .WithMany(x => x.AvailabilityWeekdays)
            .HasForeignKey(x => x.SpecialistId);
    }
}