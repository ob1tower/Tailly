using Tailly.ShopService.Core.Entities.Product;
using Tailly.ShopService.Core.Models.Products;

namespace Tailly.ShopService.Infrastructure.Mappers;

public static class ProductEntityMapper
{
    public static Product ToDomain(ProductEntity entity)
    {
        if (entity == null)
            return null!;

        return new Product
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Title = entity.Title,
            CategoryId = entity.CategoryId,
            CategoryTitle = entity.Category?.Title ?? string.Empty,
            ShortDescription = entity.ShortDescription,
            Description = entity.Description,
            Price = entity.Price,
            OldPrice = entity.OldPrice,
            Rating = entity.Rating,
            ReviewsCount = entity.ReviewsCount,
            IsAvailable = entity.IsAvailable,
            StockQuantity = entity.StockQuantity,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,

            Images = entity.Images.Select(i => new ProductImage
            {
                Id = i.Id,
                Url = i.Url,
                Alt = i.Alt
            }).ToList(),

            Reviews = entity.Reviews.Select(r => new ProductReview
            {
                Id = r.Id,
                AuthorName = r.AuthorName,
                Rating = r.Rating,
                Text = r.Text,
                CreatedAt = r.CreatedAt,

                Reply = r.Reply == null ? null : new ProductReviewReply
                {
                    AuthorName = r.Reply.AuthorName,
                    Text = r.Reply.Text,
                    CreatedAt = r.Reply.CreatedAt
                }
            }).ToList()
        };
    }

    public static Category ToDomain(CategoryEntity entity)
    {
        return new Category
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Title = entity.Title
        };
    }
}