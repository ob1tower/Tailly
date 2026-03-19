namespace Tailly.AuthService.Service.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string html);
    }
}