using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;

public class CalendarDayOverrideConfiguration : IEntityTypeConfiguration<CalendarDayOverrideEntity>
{
    public void Configure(EntityTypeBuilder<CalendarDayOverrideEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.CalendarId, x.Date });
    }
}