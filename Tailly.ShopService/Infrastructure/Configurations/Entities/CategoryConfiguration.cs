using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
               .HasMaxLength(200)
               .IsRequired();

        builder.HasIndex(x => x.Slug)
               .IsUnique();

        builder.Property(x => x.Title)
               .HasMaxLength(200)
               .IsRequired();
    }
}