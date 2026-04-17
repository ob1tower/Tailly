using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReviewEntity>
{
    public void Configure(EntityTypeBuilder<ProductReviewEntity> builder)
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

        builder.HasOne(x => x.Product)
               .WithMany(x => x.Reviews)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Reply)                    
               .WithOne(x => x.Review)                 
               .HasForeignKey<ProductReviewReplyEntity>(r => r.ReviewId)  
               .IsRequired(false)                       
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.OrderId, x.ProductId }); 
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.OrderId);
    }
}