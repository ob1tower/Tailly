using CSharpFunctionalExtensions;
using Tailly.BookingService.Core.Common;

namespace Tailly.BookingService.Application.Service.Interfaces
{
    public interface IMediaService
    {
        Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid clientId);
    }
}