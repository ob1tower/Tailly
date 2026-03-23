using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Tailly.AuthService.Configurations.Options;

namespace Tailly.AuthService.Service.Email;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.Email));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient
            {
                Timeout = 45000
            };

            await smtp.ConnectAsync(_settings.Server, _settings.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_settings.Email, _settings.Password, ct);
            await smtp.SendAsync(email, ct);
            await smtp.DisconnectAsync(true, ct);

            _logger.LogInformation("Real email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Real send failed to {To}", to);
            throw;
        }
    }
}
