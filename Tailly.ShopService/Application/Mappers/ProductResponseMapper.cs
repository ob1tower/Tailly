using Tailly.ShopService.Application.Dtos.Responses.Product;
using Tailly.ShopService.Core.Models.Products;

namespace Tailly.ShopService.Application.Mappers;

public static class ProductResponseMapper
{
    public static ProductResponse ToResponse(this Product product)
    {
        if (product == null)
            return null!;

        return new ProductResponse
        {
            Id = product.Id.ToString(),
            Slug = product.Slug,
            Title = product.Title,
            CategoryId = product.CategoryId.ToString(),
            CategoryTitle = product.CategoryTitle,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Rating = product.Rating,
            ReviewsCount = product.ReviewsCount,
            IsAvailable = product.IsAvailable,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,

            Images = product.Images.Select(i => i.ToResponse()).ToList(),

            Reviews = product.Reviews.Select(r => r.ToResponse()).ToList()
        };
    }

    public static ProductImageResponse ToResponse(this ProductImage image)
    {
        return new ProductImageResponse
        {
            Id = image.Id.ToString(),
            Url = image.Url,
            Alt = image.Alt
        };
    }

    public static ProductReviewResponse ToResponse(this ProductReview review)
    {
        return new ProductReviewResponse
        {
            Id = review.Id.ToString(),
            AuthorName = review.AuthorName,
            Rating = review.Rating,
            Text = review.Text,
            CreatedAt = review.CreatedAt,
            siteReply = review.Reply?.ToResponse()
        };
    }

    public static ProductReviewReplyResponse ToResponse(this ProductReviewReply reply)
    {
        return new ProductReviewReplyResponse
        {
            AuthorName = reply.AuthorName,
            Text = reply.Text,
            CreatedAt = reply.CreatedAt
        };
    }

    public static CategoryResponse ToResponse(this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id.ToString(),
            Slug = category.Slug,
            Title = category.Title
        };
    }

    public static ProductResponse ToCatalogResponse(this Product product)
    {
        return new ProductResponse
        {
            Id = product.Id.ToString(),
            Slug = product.Slug,
            Title = product.Title,
            CategoryId = product.CategoryId.ToString(),
            CategoryTitle = product.CategoryTitle,
            ShortDescription = product.ShortDescription,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Rating = product.Rating,
            ReviewsCount = product.ReviewsCount,
            IsAvailable = product.IsAvailable,
            StockQuantity = product.StockQuantity,

            Images = product.Images.Take(1).Select(i => i.ToResponse()).ToList()
        };
    }
}