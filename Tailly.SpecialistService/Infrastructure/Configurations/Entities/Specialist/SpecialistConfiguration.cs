using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Specialist;

public class SpecialistConfiguration : IEntityTypeConfiguration<SpecialistEntity>
{
    public void Configure(EntityTypeBuilder<SpecialistEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
               .HasMaxLength(100)
               .IsRequired();

        builder.HasIndex(x => x.Slug)
               .IsUnique();

        builder.Property(x => x.FirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.LastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.MiddleName)
               .HasMaxLength(100);

        builder.Property(x => x.City)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(x => x.District)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(x => x.Phone)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(x => x.AvatarUrl)
               .HasMaxLength(500);

        builder.Property(x => x.ExperienceYears)
               .IsRequired();

        builder.Property(x => x.Rating)
               .HasPrecision(3, 1)
               .IsRequired();

        builder.Property(x => x.Latitude)
               .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
               .HasPrecision(9, 6);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasOne(x => x.Details)
               .WithOne(x => x.Specialist)
               .HasForeignKey<DetailsEntity>(x => x.SpecialistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Services)
               .WithOne(x => x.Specialist)
               .HasForeignKey(x => x.SpecialistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reviews)
               .WithOne(x => x.Specialist)
               .HasForeignKey(x => x.SpecialistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SpecialistGallery)
               .WithOne(x => x.Specialist)
               .HasForeignKey(x => x.SpecialistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Calendar)
               .WithOne(x => x.Specialist)
               .HasForeignKey<CalendarEntity>(x => x.SpecialistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}