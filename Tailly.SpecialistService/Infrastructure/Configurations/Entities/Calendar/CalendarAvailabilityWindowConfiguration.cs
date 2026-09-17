using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;

public class CalendarAvailabilityWindowConfiguration : IEntityTypeConfiguration<CalendarAvailabilityWindowEntity>
{
    public void Configure(EntityTypeBuilder<CalendarAvailabilityWindowEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
               .IsRequired();

        builder.Property(x => x.StartTime)
               .IsRequired();

        builder.Property(x => x.EndTime)
               .IsRequired();

        builder.HasIndex(x => new { x.CalendarId, x.Date });

        builder.HasOne(x => x.Calendar)
               .WithMany(x => x.AvailabilityWindows)
               .HasForeignKey(x => x.CalendarId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}