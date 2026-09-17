using Tailly.ClientProfileService.Core.Common;

namespace Tailly.ClientProfileService.Application.Errors;

public static class MediaErrors
{
    public static readonly Error InvalidFile =
        new("Media.InvalidFile", "Invalid file.");

    public static readonly Error FileTooLarge =
        new("Media.FileTooLarge", "The file is too big (maximum 10 MB).");

    public static readonly Error InvalidMediaType = 
        new("Media.InvalidMediaType", "The MediaType should be 'avatar' or 'pet'.");

    public static readonly Error UploadFailed =
        new("Media.UploadFailed", "Failed to upload file.");
}