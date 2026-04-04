using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Dtos.Responses;
using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Application.Mappers;

public static class ClientProfileDtoMapper
{
    public static ClientProfileResponse ToResponse(ClientProfile p, string? email)
    {
        return new ClientProfileResponse
        {
            Id = p.Id.ToString(),
            UserId = p.UserId.ToString(),
            Email = email ?? "",
            FirstName = p.FirstName,
            LastName = p.LastName,
            MiddleName = p.MiddleName,
            Phone = p.Phone,
            City = p.City,
            CityId = p.CityId,
            AvatarUrl = p.AvatarUrl ?? string.Empty
        };
    }

    public static ClientProfile ToModel(UpsertClientProfileRequest request)
    {
        return new ClientProfile
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Phone = request.Phone,
            City = request.City,
            CityId = request.CityId,
            AvatarUrl = request.AvatarUrl
        };
    }

    public static ClientProfile ToModel(UpdateClientProfileMainRequest request)
    {
        return new ClientProfile
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            AvatarUrl = request.AvatarUrl
        };
    }

    public static ClientProfile ToModel(UpdateClientProfileContactsRequest request)
    {
        return new ClientProfile
        {
            Phone = request.Phone,
            City = request.City,
            CityId = request.CityId
        };
    }
}