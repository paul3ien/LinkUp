// T031 - Messages Controller (CRUD endpoints)
using BusinessService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessService.Controllers;

[ApiController]
[Route("api/channels/{channelId}/messages")]
[Authorize]  // T032: Require authentication for all endpoints
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    /// <summary>
    /// T031: GET /api/channels/{channelId}/messages - List messages in channel
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMessages(Guid channelId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var messages = await _messageService.GetMessagesByChannelAsync(channelId, page, pageSize);
            return Ok(new { data = messages, page, pageSize, channelId, total = messages.Count });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for channel {ChannelId}", channelId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T031: GET /api/channels/{channelId}/messages/{id} - Get message by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMessageById(Guid channelId, Guid id)
    {
        try
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null || message.ChannelId != channelId)
                return NotFound(new { error = "Message not found" });

            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T031: POST /api/channels/{channelId}/messages - Create message
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMessage(Guid channelId, [FromBody] CreateMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { error = "Message content is required" });

            // T031: Get userId from claims (should be set by auth middleware)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            var message = await _messageService.CreateMessageAsync(channelId, userId, request.Content);
            return CreatedAtAction(nameof(GetMessageById), new { channelId, id = message.Id }, message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating message in channel {ChannelId}", channelId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// T031: DELETE /api/channels/{channelId}/messages/{id} - Delete message
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(Guid channelId, Guid id)
    {
        try
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null || message.ChannelId != channelId)
                return NotFound(new { error = "Message not found" });

            var deleted = await _messageService.DeleteMessageAsync(id);
            if (!deleted)
                return NotFound(new { error = "Message not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// T031: Request DTOs
/// </summary>
public class CreateMessageRequest
{
    public string Content { get; set; } = string.Empty;
}
