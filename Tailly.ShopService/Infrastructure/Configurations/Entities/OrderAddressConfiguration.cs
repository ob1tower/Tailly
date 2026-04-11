using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Order;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class OrderAddressConfiguration : IEntityTypeConfiguration<OrderAddressEntity>
{
    public void Configure(EntityTypeBuilder<OrderAddressEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.City)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Street)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.House)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Apartment)
               .HasMaxLength(50);

        builder.Property(x => x.Comment)
               .HasMaxLength(500);

        builder.HasOne(x => x.Order)
               .WithOne(x => x.Address)
               .HasForeignKey<OrderAddressEntity>(x => x.OrderId);
    }
}