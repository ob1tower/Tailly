using FluentValidation;
using Tailly.PostsService.Application.Dtos.Requests;

namespace Tailly.PostsService.Application.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    private const int MAX_TITLE_LENGTH = 200;
    private const int MAX_CONTENT_LENGTH = 10000;

    private const int MAX_TAGS_COUNT = 20;
    private const int MAX_IMAGES_COUNT = 10;

    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(MAX_TITLE_LENGTH).WithMessage($"Title must not exceed {MAX_TITLE_LENGTH} characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(MAX_CONTENT_LENGTH).WithMessage($"Content must not exceed {MAX_CONTENT_LENGTH} characters.");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= MAX_TAGS_COUNT)
            .WithMessage($"Maximum {MAX_TAGS_COUNT} tags allowed.");

        RuleFor(x => x.ImageUrls)
            .Must(images => images == null || images.Count <= MAX_IMAGES_COUNT)
            .WithMessage($"Maximum {MAX_IMAGES_COUNT} images allowed.");
    }
}