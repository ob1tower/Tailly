using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Services;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Services;

public class AvailabilityServiceConfiguration : IEntityTypeConfiguration<AvailabilityServiceEntity>
{
    public void Configure(EntityTypeBuilder<AvailabilityServiceEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Availability)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.AvailabilityId);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Availabilities)
            .HasForeignKey(x => x.ServiceId);
    }
}