namespace AuthService.Services;

/// <summary>
/// T022: Interface pour génération et validation JWT tokens
/// Responsabilités : Créer tokens signés, extraire claims
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// T022: Générer un JWT token pour un utilisateur
    /// Token contient : sub (userId), email, role
    /// Expiration : 1h
    /// </summary>
    /// <param name="userId">ID utilisateur</param>
    /// <param name="email">Email utilisateur</param>
    /// <param name="role">Rôle utilisateur (défaut: "user")</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(Guid userId, string email, string role = "user");

    /// <summary>
    /// T022: Valider un token JWT et extraire les claims
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>Principal avec claims ou null si invalid</returns>
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
}
