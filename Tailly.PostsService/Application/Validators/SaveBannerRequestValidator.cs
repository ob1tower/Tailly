using FluentValidation;
using Tailly.PostsService.Application.Dtos.Requests;

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

        RuleFor(x => x.ImageUrl)
            .MaximumLength(MAX_URL_LENGTH).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage($"ImageUrl must not exceed {MAX_URL_LENGTH} characters.");

        RuleFor(x => x.LinkUrl)
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .When(x => !string.IsNullOrWhiteSpace(x.LinkUrl))
            .WithMessage("LinkUrl must be a valid URL.");

        RuleFor(x => x)
            .Must(x => !x.StartsAt.HasValue || !x.EndsAt.HasValue || x.EndsAt > x.StartsAt)
            .WithMessage("EndsAt must be greater than StartsAt.");

        RuleFor(x => x.StartsAt)
            .Must(date => !date.HasValue || date.Value >= DateTime.UtcNow)
            .WithMessage("StartsAt cannot be in the past.");

        RuleFor(x => x.EndsAt)
            .Must(date => !date.HasValue || date.Value >= DateTime.UtcNow)
            .WithMessage("EndsAt cannot be in the past.");
    }
}