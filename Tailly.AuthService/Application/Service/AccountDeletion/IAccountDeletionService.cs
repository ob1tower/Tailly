using CSharpFunctionalExtensions;

namespace Tailly.AuthService.Application.Service.AccountDeletion
{
    public interface IAccountDeletionService
    {
        Task<Result<(string email, string role, DateTime restoreUntil)>> GetRestorePreviewAsync(string token);
        Task<Result<DateTime>> RequestDeletionAsync(Guid userId, string password);
        Task<Result> RestoreAsync(string token);
    }
}