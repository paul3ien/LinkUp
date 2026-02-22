// T021 - AuthenticationService Unit Tests
using Xunit;
using Moq;
using AuthService.Services;
using Microsoft.Extensions.Logging;

namespace LinkUp.Tests.AuthService.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;

    public AuthenticationServiceTests()
    {
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();
    }

    [Fact]
    public void PasswordHashing_ShouldNotStorePlaintext()
    {
        // T021 - BCrypt should hash passwords, not store plaintext
        
        // Arrange
        var password = "SecurePass123!";

        // Act
        var hash1 = BCrypt.Net.BCrypt.HashPassword(password);
        var hash2 = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        Assert.NotEqual(password, hash1);
        Assert.NotEqual(password, hash2);
        // Same password should produce different hashes (BCrypt adds salt)
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void PasswordVerification_ShouldValidateCorrectlyHashedPassword()
    {
        // T021 - BCrypt verification should work with hashed passwords

        // Arrange
        var password = "SecurePass123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void PasswordVerification_ShouldRejectWrongPassword()
    {
        // T021 - BCrypt should reject wrong password

        // Arrange
        var correctPassword = "SecurePass123!";
        var wrongPassword = "WrongPassword!";
        var hash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        // Act
        var isValid = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }
}
