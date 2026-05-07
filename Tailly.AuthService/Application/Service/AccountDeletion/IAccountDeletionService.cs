using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Application.Service.AccountDeletion;

public interface IAccountDeletionService
{
    Task<Result<DateTime>> RequestDeletionAsync(Guid userId, string password, RoleType role);
    Task<Result> RestoreAsync(string token);
}