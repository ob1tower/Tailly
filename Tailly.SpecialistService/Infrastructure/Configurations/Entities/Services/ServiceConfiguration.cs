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
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(1000);

        builder.Property(x => x.Price)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(x => x.PriceUnit)
               .IsRequired();

        builder.HasOne(x => x.Specialist)
               .WithMany(x => x.Services)
               .HasForeignKey(x => x.SpecialistId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}