using Tailly.BookingService.Core.Models;

namespace Tailly.BookingService.Application.Service.Interfaces
{
    public interface IMediaService
    {
        Task<UploadResult> SaveAsync(IFormFile file, string folder = "reviews");
        Task<List<UploadResult>> SaveMultipleAsync(IEnumerable<IFormFile> files, string folder = "reviews");
    }
}