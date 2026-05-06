using FluentValidation;

namespace Tailly.BookingService.Application.Validators;

public class MediaUploadValidator : AbstractValidator<IFormFile>
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public MediaUploadValidator()
    {
        RuleFor(x => x)
            .NotNull().WithMessage("The file is not selected.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("The file is empty.")
            .LessThanOrEqualTo(MaxFileSize).WithMessage("The file is too big (maximum 5 MB).");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("The file name cannot be empty.");

        RuleFor(x => Path.GetExtension(x.FileName).ToLowerInvariant())
            .Must(ext => ext is ".jpg" or ".jpeg" or ".png" or ".webp")
            .WithMessage("Only images are allowed: .jpg, .jpeg, .png, .webp");
    }
}