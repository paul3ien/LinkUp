// T030 - Channels Controller (CRUD endpoints)
using BusinessService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // T032: Require authentication for all endpoints
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly ILogger<ChannelsController> _logger;

    public ChannelsController(IChannelService channelService, ILogger<ChannelsController> logger)
    {
        _channelService = channelService;
        _logger = logger;
    }

    /// <summary>
    /// T030: GET /api/channels - List all channels
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetChannels([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var channels = await _channelService.GetChannelsAsync(page, pageSize);
            return Ok(new { data = channels, page, pageSize, total = channels.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channels");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T030: GET /api/channels/{id} - Get channel by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChannelById(Guid id)
    {
        try
        {
            var channel = await _channelService.GetChannelByIdAsync(id);
            if (channel == null)
                return NotFound(new { error = "Channel not found" });

            return Ok(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel {ChannelId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T030: POST /api/channels - Create new channel
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "Channel name is required" });

            // T030: Get userId from claims (should be set by auth middleware)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

            var channel = await _channelService.CreateChannelAsync(request.Name, userId);
            return CreatedAtAction(nameof(GetChannelById), new { id = channel.Id }, channel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T030: PUT /api/channels/{id} - Update channel
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChannel(Guid id, [FromBody] UpdateChannelRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "Channel name is required" });

            var channel = await _channelService.UpdateChannelAsync(id, request.Name);
            return Ok(channel);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T030: DELETE /api/channels/{id} - Delete channel
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChannel(Guid id)
    {
        try
        {
            var deleted = await _channelService.DeleteChannelAsync(id);
            if (!deleted)
                return NotFound(new { error = "Channel not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// T030: Request DTOs
/// </summary>
public class CreateChannelRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateChannelRequest
{
    public string Name { get; set; } = string.Empty;
}
