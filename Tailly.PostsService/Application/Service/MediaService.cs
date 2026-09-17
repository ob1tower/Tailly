using Microsoft.AspNetCore.Mvc.Formatters;
using Tailly.PostsService.Application.Service.Interfaces;

namespace Tailly.PostsService.Application.Service;

public class MediaService : IMediaService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaService> _logger;

    private const long MAX_FILE_SIZE = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public MediaService(IWebHostEnvironment env,
                            ILogger<MediaService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

            if (file.Length > MAX_FILE_SIZE)
                throw new Exception("File too large.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new Exception("Invalid file type.");

            var root = _env.WebRootPath;

            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                _logger.LogWarning("WebRootPath is null, fallback used: {Root}", root);
            }

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
                _logger.LogInformation("Created root folder: {Root}", root);
            }

            var uploadsPath = Path.Combine(root, "uploads", folder);

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                _logger.LogInformation("Created upload folder: {Path}", uploadsPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            _logger.LogInformation("Saving file to: {Path}", filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/post-uploads/{folder}/{fileName}";

            _logger.LogInformation("File uploaded: {Url}", url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed: {Message}", ex.Message);

            throw new Exception(ex.Message);
        }
    }
}