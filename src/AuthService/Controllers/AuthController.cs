global using Microsoft.AspNetCore.Mvc;
global using AuthService.Services;
global using Microsoft.AspNetCore.Authorization;
global using System.Security.Claims;

namespace AuthService.Controllers;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var user = await _authService.Register(request.Email, request.Password);
            var token = await _authService.Login(request.Email, request.Password);

            return CreatedAtAction(nameof(Register), new { userId = user.Id }, new
            {
                token,
                userId = user.Id,
                email = user.Email,
                username = user.Username,
                tokenType = "Bearer",
                expiresIn = 3600
            });
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var token = await _authService.Login(request.Email, request.Password);
            if (token == null) return Unauthorized("Invalid email or password");

            var user = await _authService.GetUserByEmail(request.Email);

            return Ok(new
            {
                token,
                userId = user!.Id,
                email = user.Email,
                username = user.Username,
                tokenType = "Bearer",
                expiresIn = 3600
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Login error: {Message}", ex.Message);
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _authService.GetUserById(userId);
        if (user == null) return NotFound();

        return Ok(new { userId = user.Id, email = user.Email, username = user.Username });
    }

    /// <summary>Public – resolve userId to username for chat display</summary>
    [HttpGet("users/{userId:guid}/username")]
    public async Task<IActionResult> GetUsername(Guid userId)
    {
        var user = await _authService.GetUserById(userId);
        if (user == null) return NotFound();
        return Ok(new { username = user.Username });
    }

    [HttpPut("profile/username")]
    [Authorize]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required");

        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _authService.ChangeUsername(userId, request.Username);
            return Ok(new { username = user.Username });
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("profile/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("Both current and new password are required");

        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.ChangePassword(userId, request.CurrentPassword, request.NewPassword);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }
}

public class RegisterRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class ChangeUsernameRequest { public string Username { get; set; } = string.Empty; }
public class ChangePasswordRequest { public string CurrentPassword { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }

