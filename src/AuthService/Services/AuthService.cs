global using BC = BCrypt.Net.BCrypt;
global using AuthServiceData = AuthService.Data;
global using AuthServiceModels = AuthService.Models;

namespace AuthService.Services;

/// <summary>
/// T021, T022: Authentification service
/// Responsabilités : Registration (BCrypt), Login (password validation)
/// </summary>
public class AuthenticationService : IAuthService
{
    private readonly AuthServiceData.AuthDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(AuthServiceData.AuthDbContext context, IJwtService jwtService, ILogger<AuthenticationService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// T021: Enregistrer un nouvel utilisateur
    /// 1. Vérifier email unique
    /// 2. Hasher mdp avec BCrypt
    /// 3. Sauvegarder en DB
    /// </summary>
    public async Task<AuthServiceModels.User> Register(string email, string password)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters", nameof(password));

        // Vérifier email unique
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
            throw new InvalidOperationException($"User with email '{email}' already exists");

        // T021: Hasher mdp avec BCrypt
        var passwordHash = BC.HashPassword(password);

        // Créer entity
        var user = new AuthServiceModels.User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        // Sauvegarder en DB
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered: {Email}", email);
        return user;
    }

    /// <summary>
    /// T022: Authentifier utilisateur et générer token
    /// 1. Trouver user par email
    /// 2. Valider password avec BCrypt
    /// 3. Générer JWT token (1h expiration)
    /// </summary>
    public async Task<string?> Login(string email, string password)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        // Trouver user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return null;

        // T022: Valider password BCrypt
        if (!BC.Verify(password, user.PasswordHash))
            return null;

        // Générer token JWT
        var token = _jwtService.GenerateToken(user.Id, user.Email, "user");

        _logger.LogInformation("User logged in: {Email}", email);
        return token;
    }

    /// <summary>
    /// Get user by email for retrieving user ID
    /// </summary>
    public async Task<AuthServiceModels.User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
