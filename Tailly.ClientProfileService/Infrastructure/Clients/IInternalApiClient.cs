using Tailly.ClientProfileService.Application.Clients;

namespace Tailly.ClientProfileService.Infrastructure.Clients
{
    public interface IInternalApiClient
    {
        Task<PetInternalDto?> GetPetAsync(Guid petId);
        Task<ServiceInternalDto?> GetServiceAsync(Guid serviceId);
        Task<SpecialistInternalDto?> GetSpecialistAsync(Guid specialistId);
    }
}