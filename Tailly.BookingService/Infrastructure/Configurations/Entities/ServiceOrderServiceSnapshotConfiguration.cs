using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.BookingService.Core.Entities;

namespace Tailly.BookingService.Infrastructure.Configurations.Entities;

public class ServiceOrderServiceSnapshotConfiguration : IEntityTypeConfiguration<ServiceOrderServiceSnapshotEntity>
{
    public void Configure(EntityTypeBuilder<ServiceOrderServiceSnapshotEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.OrderId)
               .IsUnique();

        builder.HasOne(x => x.Order)
               .WithOne(x => x.ServiceSnapshot)
               .HasForeignKey<ServiceOrderServiceSnapshotEntity>(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ServiceId);

        builder.Property(x => x.Title)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.Price)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.PriceUnit)
               .IsRequired();
    }
}