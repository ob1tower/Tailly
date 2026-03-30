namespace Tailly.AuthService.Infrastructure.Configurations.Options;

public sealed class EmailSettings
{
    public required string Server { get; set; }
    public required int Port { get; set; }
    public required string SenderName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}