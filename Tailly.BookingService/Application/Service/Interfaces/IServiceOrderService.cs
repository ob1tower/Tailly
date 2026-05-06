using CSharpFunctionalExtensions;
using Tailly.BookingService.Core.Common;
using Tailly.BookingService.Core.Models;

namespace Tailly.BookingService.Application.Service.Interfaces
{
    public interface IServiceOrderService
    {
        Task<Result> CancelAsync(Guid orderId, Guid clientId, string? reason = null);
        Task<Result> CompleteAsync(Guid orderId, Guid specialistId);
        Task<Result> ConfirmAsync(Guid orderId, Guid specialistId);
        Task<Result<List<ServiceOrder>, Error>> GetBySpecialistIdAsync(Guid specialistId, string? statusFilter = null, int page = 1, int limit = 20);
        Task<Result<ServiceOrder, Error>> CreateAsync(CreateServiceOrder model);
        Task<Result<ServiceOrder, Error>> GetByIdAsync(Guid orderId, Guid? clientId, Guid? specialistId);
        Task<Result<List<ServiceOrder>, Error>> GetMyOrdersAsync(Guid clientId, string? statusFilter = null, int page = 1, int limit = 20);
        Task<Result<ServiceOrderReview, Error>> LeaveReviewAsync(Guid orderId, Guid clientId, int rating, string comment, List<string>? photos = null);
        Task<Result<RepeatServiceOrderDraft, Error>> RepeatAsync(Guid orderId, Guid clientId);
        Task<Result> StartAsync(Guid orderId, Guid specialistId);
    }
}