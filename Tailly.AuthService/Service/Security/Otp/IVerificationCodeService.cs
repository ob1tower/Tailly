namespace Tailly.AuthService.Service.Security.Otp
{
    public interface IVerificationCodeService
    {
        Task RemoveCodeAsync(string email, string purpose = "default");
        Task SetCodeAsync(string email, string code, string purpose = "default");
        Task<(bool Success, string? Error)> VerifyCodeAsync(string email, string code, string expectedPurpose = "default");
    }
}