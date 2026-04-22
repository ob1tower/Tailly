using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Services;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Services;

public class ServiceConfiguration : IEntityTypeConfiguration<ServiceEntity>
{
    public void Configure(EntityTypeBuilder<ServiceEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.LocationLabel)
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.Availabilities)
            .WithOne(x => x.Service)
            .HasForeignKey(x => x.ServiceId);

        builder.HasMany(x => x.BookedSlots)
            .WithOne(x => x.Service)
            .HasForeignKey(x => x.ServiceId);
    }
}