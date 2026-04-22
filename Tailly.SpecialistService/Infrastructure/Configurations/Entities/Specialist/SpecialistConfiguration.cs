using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.District)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Rating)
            .HasColumnType("decimal(3,2)");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.AvailabilityWeekdays)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.Services)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.Availabilities)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.BookedSlots)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.Reviews)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.Gallery)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.Advantages)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.PetTypes)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.PetSizes)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);

        builder.HasMany(x => x.PetAges)
            .WithOne(x => x.Specialist)
            .HasForeignKey(x => x.SpecialistId);
    }
}