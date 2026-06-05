global using BC = BCrypt.Net.BCrypt;
global using AuthServiceData = AuthService.Data;
global using AuthServiceModels = AuthService.Models;

namespace AuthService.Services;

/// <summary>Word pool for auto-generated usernames (word + 4-digit number)</summary>
internal static class UsernameWordPool
{
    internal static readonly string[] Words =
    [
        "Dragon", "Phoenix", "Wolf", "Tiger", "Eagle", "Falcon", "Shadow", "Storm",
        "Blaze", "Frost", "Viper", "Hawk", "Raven", "Cobra", "Lynx", "Panda",
        "Comet", "Nova", "Pixel", "Neon", "Pulse", "Spark", "Volt", "Glitch",
        "Nebula", "Orbit", "Quasar", "Zenith", "Apex", "Cipher", "Echo", "Flux",
        "Ghost", "Hyper", "Icon", "Jade", "Karma", "Laser", "Mirage", "Nexus"
    ];
}

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

        // Générer pseudo unique
        var username = await GenerateUniqueUsernameAsync();

        // Créer entity
        var user = new AuthServiceModels.User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            Username = username,
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
        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Username, "user");

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

    public async Task<AuthServiceModels.User?> GetUserById(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<AuthServiceModels.User> ChangeUsername(Guid userId, string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername) || newUsername.Length > 50)
            throw new ArgumentException("Invalid username");

        // Check uniqueness
        var taken = await _context.Users.AnyAsync(u => u.Username == newUsername && u.Id != userId);
        if (taken)
            throw new InvalidOperationException("Username already taken");

        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        user.Username = newUsername;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Username changed for user {UserId}: {Username}", userId, newUsername);
        return user;
    }

    public async Task ChangePassword(Guid userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new ArgumentException("New password must be at least 6 characters");

        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        if (!BC.Verify(currentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        user.PasswordHash = BC.HashPassword(newPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password changed for user {UserId}", userId);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private async Task<string> GenerateUniqueUsernameAsync()
    {
        string username;
        do
        {
            var word = UsernameWordPool.Words[Random.Shared.Next(UsernameWordPool.Words.Length)];
            var number = Random.Shared.Next(1000, 10000);
            username = $"{word}{number}";
        }
        while (await _context.Users.AnyAsync(u => u.Username == username));

        return username;
    }
}
