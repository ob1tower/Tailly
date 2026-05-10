using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class ProductReviewImageConfiguration: IEntityTypeConfiguration<ProductReviewImageEntity>
{
    public void Configure(EntityTypeBuilder<ProductReviewImageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
               .HasMaxLength(1000)
               .IsRequired();

        builder.HasOne(x => x.Review)
               .WithMany(x => x.Images)
               .HasForeignKey(x => x.ReviewId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ReviewId);
    }
}