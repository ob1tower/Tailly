using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
               .HasMaxLength(200)
               .IsRequired();

        builder.HasIndex(x => x.Slug)
               .IsUnique();

        builder.Property(x => x.Title)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.ShortDescription)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.Price)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.OldPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Rating)
               .HasColumnType("decimal(3,2)");

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.HasMany(x => x.Reviews)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
               .WithMany(x => x.Products)
               .HasForeignKey(x => x.CategoryId);

        builder.HasMany(x => x.Images)
               .WithOne(x => x.Product)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Price);
        builder.HasIndex(x => x.IsAvailable);
        builder.HasIndex(x => x.Rating);
    }
}