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

    public async Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid userId)
    {
        if (file == null || file.Length == 0)
            return Result.Failure<string, Error>(MediaErrors.InvalidFile);

        if (file.Length > MaxFileSize)
            return Result.Failure<string, Error>(MediaErrors.FileTooLarge);

        if (string.IsNullOrWhiteSpace(mediaType) || (mediaType != "avatar" && mediaType != "pet"))
        {
            _logger.LogWarning("Invalid mediaType: '{MediaType}' for user {UserId}", mediaType, userId);
            return Result.Failure<string, Error>(MediaErrors.InvalidMediaType);
        }

        try
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                _logger.LogWarning("WebRootPath is empty. Using ContentRootPath instead.");
            }

            var uploadsFolder = Path.Combine("uploads", mediaType.ToLower(), userId.ToString());
            var physicalPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, uploadsFolder);

            Directory.CreateDirectory(physicalPath);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(physicalPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/uploads/{mediaType.ToLower()}/{userId}/{fileName}";

            _logger.LogInformation("File uploaded successfully. UserId: {UserId}, MediaType: {MediaType}, Path: {FullPath}",
                userId, mediaType, fullPath);

            return Result.Success<string, Error>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file for user {UserId}, MediaType: {MediaType}. Error: {Message}",
                userId, mediaType, ex.Message);

            return Result.Failure<string, Error>(MediaErrors.UploadFailed);
        }
    }
}