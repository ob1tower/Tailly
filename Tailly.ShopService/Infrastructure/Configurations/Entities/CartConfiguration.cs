using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Cart;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class CartConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.HasMany(x => x.Items)
               .WithOne(x => x.Cart)
               .HasForeignKey(x => x.CartId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
               .IsUnique()
               .HasFilter("\"UserId\" IS NOT NULL");

        builder.HasIndex(x => x.SessionId)
               .IsUnique()
               .HasFilter("\"SessionId\" IS NOT NULL");
    }
}