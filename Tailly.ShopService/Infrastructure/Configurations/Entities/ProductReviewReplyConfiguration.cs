using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class ProductReviewReplyConfiguration : IEntityTypeConfiguration<ProductReviewReplyEntity>
{
    public void Configure(EntityTypeBuilder<ProductReviewReplyEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Text)
               .HasMaxLength(2000)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();
    }
}