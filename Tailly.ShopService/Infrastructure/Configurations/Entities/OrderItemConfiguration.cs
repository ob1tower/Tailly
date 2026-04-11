using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Order;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductTitle)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.ProductSlug)
               .HasMaxLength(200);

        builder.Property(x => x.Price)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.OldPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.LineTotal)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.Quantity)
               .IsRequired();

        builder.HasOne(x => x.Order)
               .WithMany(x => x.Items)
               .HasForeignKey(x => x.OrderId);
    }
}