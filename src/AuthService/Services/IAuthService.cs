namespace AuthService.Services;

/// <summary>
/// T021, T022: Interface pour services d'authentification
/// Responsabilités : Registration, Login, Token validation
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// T021: Créer un nouvel utilisateur avec email et mot de passe
    /// Le mot de passe est hashé avec BCrypt avant persistence
    /// </summary>
    /// <param name="email">Email unique de l'utilisateur</param>
    /// <param name="password">Mot de passe en clair</param>
    /// <returns>User créé ou exception si email existe déjà</returns>
    Task<Models.User> Register(string email, string password);

    /// <summary>
    /// T022: Authentifier un utilisateur et générer un token JWT
    /// </summary>
    /// <param name="email">Email utilisateur</param>
    /// <param name="password">Mot de passe en clair</param>
    /// <returns>JWT token string ou null si credentials invalides</returns>
    Task<string?> Login(string email, string password);
}
