using Tailly.ClientProfileService.Core.Common;

namespace Tailly.ClientProfileService.Application.Errors;

public static class ClientProfileErrors
{
    public static readonly Error ProfileNotFound =
        new("ClientProfile.ProfileNotFound", "Client profile not found.");

    public static readonly Error PetNotFound =
        new("ClientProfile.PetNotFound", "Pet not found.");

    public static readonly Error AccessDenied =
        new("ClientProfile.AccessDenied", "You do not have access to this resource.");

    public static readonly Error ProfileAlreadyExists =
        new("ClientProfile.ProfileAlreadyExists", "Client profile already exists.");
}