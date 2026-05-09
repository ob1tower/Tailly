using Tailly.BookingService.Application.Dtos.Internal;

namespace Tailly.BookingService.Infrastructure.Clients
{
    public interface IInternalApiClient
    {
        Task<PetInternalDto?> GetPetAsync(Guid petId);
        Task<ServiceInternalDto?> GetServiceAsync(Guid serviceId);
        Task<SpecialistInternalDto?> GetSpecialistAsync(Guid specialistId);
    }
}