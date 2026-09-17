using FluentValidation;
using Tailly.ShopService.Application.Dtos.Requests.Media;

namespace Tailly.ShopService.Application.Validators;

public class UploadMediaRequestValidator : AbstractValidator<UploadMediaRequest>
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedMediaTypes =
    [
        "reviews"
    ];

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public UploadMediaRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.")

            .Must(x => x!.Length > 0)
            .WithMessage("File cannot be empty.")

            .Must(x => x!.Length <= MaxFileSizeInBytes)
            .WithMessage("File size exceeds allowed limit.")

            .Must(file =>
            {
                var extension =
                    Path.GetExtension(file!.FileName)
                        .ToLowerInvariant();

                return AllowedExtensions.Contains(extension);
            })
            .WithMessage("Invalid file type.");

        RuleFor(x => x.MediaType)
            .NotEmpty()
            .WithMessage("MediaType is required.")

            .Must(AllowedMediaTypes.Contains)
            .WithMessage("Invalid media type.");
    }
}