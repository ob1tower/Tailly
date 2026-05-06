using FluentValidation;

namespace Tailly.BookingService.Application.Validators;

public class MediaMultipleUploadValidator : AbstractValidator<List<IFormFile>>
{
    public MediaMultipleUploadValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("No files selected.");

        RuleFor(x => x.Count)
            .LessThanOrEqualTo(10).WithMessage("Maximum of 10 files at a time.");

        RuleForEach(x => x).SetValidator(new MediaUploadValidator());
    }
}
