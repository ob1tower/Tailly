namespace Tailly.AuthService.Application.Service.Security.Interfaces;

public interface IVerificationCodeService
{
    Task RemoveCodeAsync(string email, string purpose = "default");
    Task SetCodeAsync(string email, string code, string purpose = "default");
    Task<(bool Success, string? Error)> VerifyCodeAsync(string email, string code, string expectedPurpose = "default");
    Task<(bool Success, string? Error)> VerifyCodeAsync(string email, string code, string expectedPurpose = "default", bool deleteAfterVerify = true);
}