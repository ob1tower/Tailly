using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Cart;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItemEntity>
{
    public void Configure(EntityTypeBuilder<CartItemEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
               .IsRequired();

        builder.HasIndex(x => new { x.CartId, x.ProductId })
               .IsUnique();

        builder.Property(x => x.Price)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.OldPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ProductTitle)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.ProductSlug)
               .HasMaxLength(200);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(1000);

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.HasOne(x => x.Cart)
               .WithMany(x => x.Items)
               .HasForeignKey(x => x.CartId);
    }
}