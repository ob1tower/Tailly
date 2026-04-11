using FluentValidation;
using Tailly.ShopService.Application.Dtos.Requests.Catalog;

namespace Tailly.ShopService.Application.Validators;

public class CatalogFilterRequestValidator : AbstractValidator<CatalogFilterRequest>
{
    public CatalogFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(1).WithMessage("Limit must be greater than or equal to 1.")
            .LessThanOrEqualTo(100).WithMessage("Limit cannot exceed 100 items.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue)
            .WithMessage("MinPrice cannot be negative.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue)
            .WithMessage("MaxPrice cannot be negative.");

        RuleFor(x => x.Sort)
           .Must(sort => string.IsNullOrEmpty(sort) || new[] { "popular", "price-asc", "price-desc", "rating-desc", "newest" }
           .Contains(sort.ToLowerInvariant()))
           .WithMessage("Invalid sort type. Allowed: popular, price-asc, price-desc, rating-desc, newest.");
    }
}