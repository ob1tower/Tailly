using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;

public class CalendarBookedSlotConfiguration : IEntityTypeConfiguration<CalendarBookedSlotEntity>
{
    public void Configure(EntityTypeBuilder<CalendarBookedSlotEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.CalendarId, x.Date });
    }
}