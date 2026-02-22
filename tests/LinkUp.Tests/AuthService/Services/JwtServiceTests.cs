// T022 - JwtService Unit Tests
using Xunit;
using Moq;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkUp.Tests.AuthService.Services;

public class JwtServiceTests
{
    private readonly JwtService _service;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<JwtService>> _mockLogger;

    public JwtServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtService>>();
        
        // Setup configuration for JWT
        var jwtSettings = new Mock<IConfigurationSection>();
        jwtSettings.Setup(x => x["SecretKey"]).Returns("this-is-a-very-long-secret-key-at-least-32-characters-long");
        jwtSettings.Setup(x => x["Issuer"]).Returns("LinkUp");
        jwtSettings.Setup(x => x["Audience"]).Returns("LinkUpUsers");
        jwtSettings.Setup(x => x["ExpirationMinutes"]).Returns("60");
        
        _mockConfig.Setup(x => x.GetSection("Jwt")).Returns(jwtSettings.Object);
        
        _service = new JwtService(_mockConfig.Object, _mockLogger.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // T022 - JWT token should have 3 parts (header.payload.signature)
        
        // Arrange
        var userId = System.Guid.NewGuid();
        var email = "test@example.com";
        var role = "user";

        // Act
        var token = _service.GenerateToken(userId, email, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        // JWT token should have 3 parts separated by dots
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void GenerateToken_WithDifferentUsers_ShouldReturnDifferentTokens()
    {
        // T022 - Different users should get different tokens

        // Arrange
        var userId1 = System.Guid.NewGuid();
        var userId2 = System.Guid.NewGuid();

        // Act
        var token1 = _service.GenerateToken(userId1, "user1@test.com", "user");
        var token2 = _service.GenerateToken(userId2, "user2@test.com", "user");

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // T022 - JWT validation should work with valid token

        // Arrange
        var userId = System.Guid.NewGuid();
        var email = "test@example.com";
        var token = _service.GenerateToken(userId, email, "user");

        // Act
        var principal = _service.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        // Token contains sub claim (user ID)
        var subClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        Assert.NotNull(subClaim);
        Assert.Equal(userId.ToString(), subClaim.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // T022 - JWT validation should reject invalid token

        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _service.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ShouldReturnNull()
    {
        // T022 - JWT validation should reject empty token

        // Arrange
        var emptyToken = string.Empty;

        // Act
        var principal = _service.ValidateToken(emptyToken);

        // Assert
        Assert.Null(principal);
    }
}
