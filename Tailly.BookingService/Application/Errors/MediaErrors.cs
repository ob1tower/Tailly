using Tailly.BookingService.Core.Common;

namespace Tailly.BookingService.Application.Errors;

public static class MediaErrors
{
    public static readonly Error InvalidFile =
        new("Media.InvalidFile", "Invalid file.");

    public static readonly Error UploadFailed =
        new("Media.UploadFailed", "Failed to upload file.");
}