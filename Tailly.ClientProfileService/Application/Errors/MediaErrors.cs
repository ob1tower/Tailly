using Tailly.ClientProfileService.Core.Common;

namespace Tailly.ClientProfileService.Application.Errors;

public static class MediaErrors
{
    public static readonly Error InvalidFile =
        new("Media.InvalidFile", "Некорректный файл.");

    public static readonly Error FileTooLarge =
        new("Media.FileTooLarge", "Файл слишком большой (максимум 10 МБ).");

    public static readonly Error InvalidMediaType = 
        new("Media.InvalidMediaType", "MediaType должен быть 'avatar' или 'pet'.");

    public static readonly Error UploadFailed =
        new("Media.UploadFailed", "Не удалось загрузить файл.");
}