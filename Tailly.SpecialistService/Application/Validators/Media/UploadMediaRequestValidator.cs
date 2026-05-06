using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Media;

namespace Tailly.SpecialistService.Application.Validators.Media;

public class UploadMediaRequestValidator : AbstractValidator<UploadMediaRequest>
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedMediaTypes = { "avatar", "specialist_gallery" };

    public UploadMediaRequestValidator()
    {
        RuleFor(r => r.File)
            .NotNull().WithMessage("File is required.")
            .Must(file => file?.Length > 0).WithMessage("File cannot be empty.")
            .Must(file => file?.Length <= MaxFileSizeInBytes)
                .WithMessage($"File size must not exceed {MaxFileSizeInBytes / (1024 * 1024)} MB.");

        RuleFor(r => r.MediaType)
            .NotEmpty().WithMessage("MediaType is required.")
            .Must(mt => AllowedMediaTypes.Contains(mt))
            .WithMessage($"MediaType must be one of: {string.Join(", ", AllowedMediaTypes)}.");
    }
}