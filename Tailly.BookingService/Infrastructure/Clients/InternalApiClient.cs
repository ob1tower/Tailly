using Tailly.BookingService.Application.Dtos.Internal;

namespace Tailly.BookingService.Infrastructure.Clients;

public class InternalApiClient : IInternalApiClient
{
    private readonly HttpClient _httpClient;

    public InternalApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PetInternalDto?> GetPetAsync(Guid petId)
    {
        return await _httpClient.GetFromJsonAsync<PetInternalDto>(
            $"http://clientprofile:8080/internal/pets/{petId}");
    }

    public async Task<ServiceInternalDto?> GetServiceAsync(Guid serviceId)
    {
        return await _httpClient.GetFromJsonAsync<ServiceInternalDto>(
            $"http://specialist:8080/internal/services/{serviceId}");
    }

    public async Task<SpecialistInternalDto?> GetSpecialistAsync(Guid specialistId)
    {
        return await _httpClient.GetFromJsonAsync<SpecialistInternalDto>(
            $"http://specialist:8080/internal/specialists/{specialistId}");
    }

    public async Task<AvailabilityCheckResponseDto?> CheckAvailabilityAsync(CheckAvailabilityRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://specialist:8080/internal/calendar/check-availability",
            request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<AvailabilityCheckResponseDto>();
    }

    public async Task CreateBookedSlotAsync(CreateBookedSlotRequestDto request)
    {
        await _httpClient.PostAsJsonAsync(
            "http://specialist:8080/internal/calendar/booked-slots",
            request);
    }

    public async Task DeleteBookedSlotAsync(Guid orderId)
    {
        await _httpClient.DeleteAsync(
            $"http://specialist:8080/internal/calendar/booked-slots/order/{orderId}");
    }
}