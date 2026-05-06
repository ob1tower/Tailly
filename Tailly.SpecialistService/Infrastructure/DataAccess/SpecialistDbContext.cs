using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Core.Entities.Applications;
using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Gallery;
using Tailly.SpecialistService.Core.Entities.Reviews;
using Tailly.SpecialistService.Core.Entities.Services;
using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Applications;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Calendar;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Details;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Gallery;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Reviews;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Services;
using Tailly.SpecialistService.Infrastructure.Configurations.Entities.Specialist;

namespace Tailly.SpecialistService.Infrastructure.DataAccess;

public class SpecialistDbContext : DbContext
{
    public SpecialistDbContext(DbContextOptions<SpecialistDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SpecialistApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new SpecialistConfiguration());
        modelBuilder.ApplyConfiguration(new DetailsConfiguration());
        modelBuilder.ApplyConfiguration(new PetSizeConfiguration());
        modelBuilder.ApplyConfiguration(new PetAgeConfiguration());
        modelBuilder.ApplyConfiguration(new PetTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new SpecialistGalleryConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarDayOverrideConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarAvailabilityWindowConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarBookedSlotConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarBookingSettingsConfiguration());

        SpecialistSeedData.Seed(modelBuilder);
    }

    public DbSet<SpecialistApplicationEntity> SpecialistApplications { get; set; }
    public DbSet<SpecialistEntity> Specialists { get; set; }
    public DbSet<DetailsEntity> Details { get; set; }
    public DbSet<PetSizeEntity> PetSizes { get; set; }
    public DbSet<PetAgeEntity> PetAges { get; set; }
    public DbSet<PetTypeEntity> PetTypes { get; set; }
    public DbSet<ServiceEntity> Services { get; set; }
    public DbSet<SpecialistGalleryEntity> SpecialistGalleries { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<CalendarEntity> Calendars { get; set; }
    public DbSet<CalendarDayOverrideEntity> CalendarDayOverrides { get; set; }
    public DbSet<CalendarAvailabilityWindowEntity> CalendarAvailabilityWindows { get; set; }
    public DbSet<CalendarBookedSlotEntity> CalendarBookedSlots { get; set; }
    public DbSet<CalendarBookingSettingsEntity> CalendarBookingSettings { get; set; }
}