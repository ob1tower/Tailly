using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImageEntity>
{
    public void Configure(EntityTypeBuilder<ProductImageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(x => x.Alt)
               .HasMaxLength(300)
               .IsRequired();

        builder.HasOne(x => x.Product)
               .WithMany(x => x.Images)
               .HasForeignKey(x => x.ProductId);
    }
}