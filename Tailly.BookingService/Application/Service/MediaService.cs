using CSharpFunctionalExtensions;
using Tailly.BookingService.Application.Errors;
using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Core.Common;

namespace Tailly.BookingService.Application.Service;

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

    public async Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid clientId)
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
                clientId.ToString());

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(
                uploadsFolder,
                fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream);

            var url = $"/booking-uploads/{mediaType}/{clientId}/{fileName}";

            _logger.LogInformation(
                "File uploaded successfully. ClientId: {ClientId}, MediaType: {MediaType}, FileName: {FileName}",
                clientId,
                mediaType,
                fileName);

            return Result.Success<string, Error>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload file for client {ClientId}",
                clientId);

            return Result.Failure<string, Error>(MediaErrors.UploadFailed);
        }
    }
}