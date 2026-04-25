using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Order;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalPrice)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.Number)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.DeliveryMethod)
               .IsRequired();

        builder.Property(x => x.PaymentMethod)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.RecipientFirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.RecipientLastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.RecipientPhone)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.RecipientEmail)
               .HasMaxLength(256)
               .IsRequired();

        builder.HasMany(x => x.Items)
               .WithOne(x => x.Order)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Address)
               .WithOne(x => x.Order)
               .HasForeignKey<OrderAddressEntity>(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OwnerUserId);
        builder.HasIndex(x => x.CreatedAt);
    }
}