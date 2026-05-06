using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Common;

namespace Tailly.SpecialistService.Application.Service;

public class MediaService : IMediaService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaService> _logger;

    private const long MaxFileSize = 10 * 1024 * 1024;

    public MediaService(IWebHostEnvironment environment, ILogger<MediaService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid specialistId)
    {
        if (file == null || file.Length == 0)
            return Result.Failure<string, Error>(MediaErrors.InvalidFile);

        if (file.Length > MaxFileSize)
            return Result.Failure<string, Error>(MediaErrors.FileTooLarge);

        var allowedTypes = new[] { "avatar", "specialist_gallery" };
        if (string.IsNullOrWhiteSpace(mediaType) || !allowedTypes.Contains(mediaType))
        {
            _logger.LogWarning("Invalid mediaType: {MediaType} for specialist {SpecialistId}", mediaType, specialistId);
            return Result.Failure<string, Error>(MediaErrors.InvalidMediaType);
        }

        try
        {
            var basePath = _environment.WebRootPath ?? _environment.ContentRootPath;
            var uploadsFolder = Path.Combine("uploads", mediaType, specialistId.ToString());
            var physicalPath = Path.Combine(basePath, uploadsFolder);

            Directory.CreateDirectory(physicalPath);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(physicalPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/uploads/{mediaType}/{specialistId}/{fileName}";

            _logger.LogInformation("File uploaded successfully. SpecialistId: {SpecialistId}, Type: {MediaType}, File: {FileName}",
                specialistId, mediaType, fileName);

            return Result.Success<string, Error>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file for specialist {SpecialistId}", specialistId);
            return Result.Failure<string, Error>(MediaErrors.UploadFailed);
        }
    }
}