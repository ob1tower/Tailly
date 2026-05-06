namespace Tailly.AuthService.Core.Models;

public class ManagedAdminResult
{
    public List<ManagedAdmin> Items { get; set; } = [];
    public int Total { get; set; }
    public int AdminsCount { get; set; }
}