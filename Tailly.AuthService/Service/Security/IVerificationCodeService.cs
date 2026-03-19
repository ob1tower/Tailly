namespace Tailly.AuthService.Service.Security
{
    public interface IVerificationCodeService
    {
        Task<string?> GetCodeAsync(string email);
        Task RemoveCodeAsync(string email);
        Task SetCodeAsync(string email, string code);
        Task<bool> VerifyCodeAsync(string email, string code);
    }
}