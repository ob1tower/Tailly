using FluentValidation;
using Tailly.PostsService.Application.Dtos.Requests;
using Tailly.PostsService.Application.Mappers;

namespace Tailly.PostsService.Application.Validators;

public class SaveBannerRequestValidator : AbstractValidator<SaveBannerRequest>
{
    private const int MAX_TITLE_LENGTH = 200;
    private const int MAX_DESCRIPTION_LENGTH = 1000;
    private const int MAX_URL_LENGTH = 1000;

    public SaveBannerRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(MAX_TITLE_LENGTH).WithMessage($"Title must not exceed {MAX_TITLE_LENGTH} characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(MAX_DESCRIPTION_LENGTH).WithMessage($"Description must not exceed {MAX_DESCRIPTION_LENGTH} characters.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => BannerMapper.TryParseStatus(status, out _))
            .WithMessage("Status must be one of: draft, published, archived.");

        RuleFor(x => x.Placement)
            .NotEmpty().WithMessage("Placement is required.")
            .Must(placement => BannerMapper.TryParsePlacement(placement, out _))
            .WithMessage("Placement must be one of: home_hero, posts, specialists, shop.");

        RuleFor(x => x.LinkTarget)
            .NotEmpty().WithMessage("LinkTarget is required.")
            .Must(linkTarget => BannerMapper.TryParseLinkTarget(linkTarget, out _))
            .WithMessage("LinkTarget must be one of: home, posts, specialists, shop, profile.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(MAX_URL_LENGTH).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage($"ImageUrl must not exceed {MAX_URL_LENGTH} characters.");

        RuleFor(x => x.LinkUrl)
            .MaximumLength(MAX_URL_LENGTH).When(x => !string.IsNullOrEmpty(x.LinkUrl))
            .WithMessage($"LinkUrl must not exceed {MAX_URL_LENGTH} characters.");
    }
}