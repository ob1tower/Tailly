using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;

public class CalendarConfiguration : IEntityTypeConfiguration<CalendarEntity>
{
    public void Configure(EntityTypeBuilder<CalendarEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.DayOverrides)
               .WithOne(x => x.Calendar)
               .HasForeignKey(x => x.CalendarId);

        builder.HasMany(x => x.BookedSlots)
            .WithOne(x => x.Calendar)
            .HasForeignKey(x => x.CalendarId);

        builder.HasMany(x => x.AvailabilityWindows)
            .WithOne(x => x.Calendar)
            .HasForeignKey(x => x.CalendarId);

        builder.HasOne(x => x.BookingSettings)
            .WithOne(x => x.Calendar)
            .HasForeignKey<CalendarBookingSettingsEntity>(x => x.CalendarId);
    }
}