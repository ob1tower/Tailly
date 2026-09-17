using CSharpFunctionalExtensions;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;

namespace Tailly.ShopService.Application.Service;

public class MediaService : IMediaService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IWebHostEnvironment environment,
                        ILogger<MediaService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid userId)
    {
        if (file == null)
        {
            return Result.Failure<string, Error>(MediaErrors.InvalidFile);
        }

        try
        {
            var rootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                rootPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");
            }

            var uploadsFolder = Path.Combine(
                rootPath,
                "uploads",
                mediaType!,
                userId.ToString());

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(
                uploadsFolder,
                fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream);

            var url = $"/shop-uploads/{mediaType}/{userId}/{fileName}";

            _logger.LogInformation(
                "File uploaded successfully. UserId: {UserId}, MediaType: {MediaType}, FileName: {FileName}",
                userId,
                mediaType,
                fileName);

            return Result.Success<string, Error>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload file for user {UserId}",
                userId);

            return Result.Failure<string, Error>(MediaErrors.UploadFailed);
        }
    }
}