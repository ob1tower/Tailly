namespace Tailly.AuthService.Core.Models;

public class AdminUserResult
{
    public List<AdminUser> Items { get; set; } = [];
    public int Total { get; set; }
    public int ClientsCount { get; set; }
    public int SpecialistsCount { get; set; }
}