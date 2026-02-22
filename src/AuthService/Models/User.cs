namespace AuthService.Models;

/// <summary>
/// T020: User entity pour authentification
/// Propriétés : Id, Email, PasswordHash
/// Persistée dans linkup_auth_db via AuthDbContext (EF Core Code-First)
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
