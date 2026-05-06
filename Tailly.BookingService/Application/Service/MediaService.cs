using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Core.Models;

namespace Tailly.BookingService.Application.Service;

public class MediaService : IMediaService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaService> _logger;

    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public MediaService(IWebHostEnvironment env, ILogger<MediaService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<UploadResult> SaveAsync(IFormFile file, string folder = "reviews")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("File is too large");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type");

        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsPath = Path.Combine(rootPath, "uploads", folder);

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/uploads/{folder}/{fileName}";

        _logger.LogInformation("File uploaded: {Url}", url);

        return new UploadResult { Url = url, FileName = fileName };
    }

    public async Task<List<UploadResult>> SaveMultipleAsync(IEnumerable<IFormFile> files, string folder = "reviews")
    {
        var results = new List<UploadResult>();

        foreach (var file in files)
        {
            var result = await SaveAsync(file, folder);
            results.Add(result);
        }

        return results;
    }
}