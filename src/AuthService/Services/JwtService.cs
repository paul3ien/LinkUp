global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

/// <summary>
/// T022: JWT Token service
/// Responsabilités : Générer tokens signés avec SymmetricSecurityKey
/// Claims : sub (userId), email, role
/// Expiration : 1h
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration config, ILogger<JwtService> logger)
    {
        // T022: Charger configuration depuis appsettings.json
        var jwtSection = config.GetSection("Jwt");
        _secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
        _issuer = jwtSection["Issuer"] ?? "LinkUp";
        _audience = jwtSection["Audience"] ?? "LinkUpClients";
        _expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? "60");
        _logger = logger;
    }

    /// <summary>
    /// T022: Générer JWT token signé
    /// Claims : sub (userId), email, role
    /// Expiration : 1h (configurable)
    /// </summary>
    public string GenerateToken(Guid userId, string email, string role = "user")
    {
        // T022: SymmetricSecurityKey
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // T022: Claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  // sub
            new Claim(ClaimTypes.Email, email),                       // email
            new Claim(ClaimTypes.Role, role)                          // role
        };

        // T022: Exiration 1h
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("JWT token generated for user: {Email}", email);
        return tokenString;
    }

    /// <summary>
    /// T022: Valider JWT token
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
