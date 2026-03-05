global using Microsoft.AspNetCore.Mvc;
global using AuthService.Services;

namespace AuthService.Controllers;

/// <summary>
/// T021, T022: Authentification controller
/// Endpoints : 
/// - POST /api/auth/register (T021)
/// - POST /api/auth/login (T022)
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// T021: Endpoint POST /api/auth/register
    /// Créer un nouvel utilisateur avec email et mdp
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var user = await _authService.Register(request.Email, request.Password);
            // Generate JWT token after registration
            var token = await _authService.Login(request.Email, request.Password);
            
            return CreatedAtAction(nameof(Register), new { userId = user.Id }, new
            {
                token,
                userId = user.Id,
                email = user.Email,
                tokenType = "Bearer",
                expiresIn = 3600
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid registration data: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// T022: Endpoint POST /api/auth/login
    /// Authentifier et retourner JWT token
    /// Token contient : sub (userId), email, role
    /// Expiration : 1h
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var token = await _authService.Login(request.Email, request.Password);
            
            if (token == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return Unauthorized("Invalid email or password");
            }

            // Get user ID from service
            var user = await _authService.GetUserByEmail(request.Email);
            var userId = user?.Id.ToString() ?? string.Empty;

            return Ok(new
            {
                token,
                userId,
                email = request.Email,
                tokenType = "Bearer",
                expiresIn = 3600 // 1 hour in seconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Login error: {Message}", ex.Message);
            return StatusCode(500, "An error occurred during login");
        }
    }
}

/// <summary>
/// T021: DTO pour registration request
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// T022: DTO pour login request
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
