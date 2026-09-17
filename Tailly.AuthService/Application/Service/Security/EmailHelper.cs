namespace Tailly.AuthService.Application.Service.Security;

public static class EmailHelper
{
    public static string Mask(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return email;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 2)
            return "***" + email[atIndex..];

        var visiblePart = email.Substring(0, 2);
        var maskedPart = new string('*', atIndex - 2);
        var domain = email[atIndex..];

        return visiblePart + maskedPart + domain;
    }
}