using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;

public class CalendarBookingSettingsConfiguration : IEntityTypeConfiguration<CalendarBookingSettingsEntity>
{
    public void Configure(EntityTypeBuilder<CalendarBookingSettingsEntity> builder)
    {
        builder.HasKey(x => x.Id);
    }
}