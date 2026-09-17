using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;

public class CalendarBookedSlotConfiguration : IEntityTypeConfiguration<CalendarBookedSlotEntity>
{
    public void Configure(EntityTypeBuilder<CalendarBookedSlotEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
               .IsRequired();

        builder.Property(x => x.StartTime)
               .IsRequired();

        builder.Property(x => x.EndTime)
               .IsRequired();

        builder.HasIndex(x => new { x.CalendarId, x.Date });

        builder.Property(x => x.ServiceId)
               .IsRequired(false);

        builder.Property(x => x.OrderId)
               .IsRequired(false);

        builder.HasOne(x => x.Calendar)
               .WithMany(x => x.BookedSlots)
               .HasForeignKey(x => x.CalendarId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}