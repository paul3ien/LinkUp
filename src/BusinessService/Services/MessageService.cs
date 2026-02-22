// T031 - Message Service Implementation
using BusinessService.Data;
using BusinessService.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessService.Services;

public class MessageService : IMessageService
{
    private readonly BusinessDbContext _context;
    private readonly ILogger<MessageService> _logger;
    private readonly IChatClient _chatClient;

    public MessageService(BusinessDbContext context, ILogger<MessageService> logger, IChatClient chatClient)
    {
        _context = context;
        _logger = logger;
        _chatClient = chatClient;
    }

    /// <summary>
    /// T031: Get messages by channel with pagination
    /// </summary>
    public async Task<List<Message>> GetMessagesByChannelAsync(Guid channelId, int page = 1, int pageSize = 20)
    {
        try
        {
            var channel = await _context.Channels.FindAsync(channelId);
            if (channel == null)
                throw new InvalidOperationException($"Channel {channelId} not found");

            var skip = (page - 1) * pageSize;
            var messages = await _context.Messages
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            
            _logger.LogInformation("Retrieved {Count} messages from channel {ChannelId}", messages.Count, channelId);
            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for channel {ChannelId}", channelId);
            throw;
        }
    }

    /// <summary>
    /// T031: Get message by ID
    /// </summary>
    public async Task<Message?> GetMessageByIdAsync(Guid messageId)
    {
        try
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            
            if (message == null)
            {
                _logger.LogWarning("Message {MessageId} not found", messageId);
            }
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving message {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// T031: Create new message in channel
    /// </summary>
    public async Task<Message> CreateMessageAsync(Guid channelId, string userId, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Message content cannot be empty");

            var channel = await _context.Channels.FindAsync(channelId);
            if (channel == null)
                throw new InvalidOperationException($"Channel {channelId} not found");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChannelId = channelId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Message {MessageId} created in channel {ChannelId} by {User}", 
                message.Id, channelId, userId);
            
            // T033: Notify notification service about new message via gRPC
            var broadcastSuccess = await _chatClient.BroadcastMessageAsync(channelId, message.Id, userId, content);
            if (!broadcastSuccess)
            {
                _logger.LogWarning("Failed to broadcast message {MessageId}", message.Id);
            }
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating message in channel {ChannelId}", channelId);
            throw;
        }
    }

    /// <summary>
    /// T031: Delete message
    /// </summary>
    public async Task<bool> DeleteMessageAsync(Guid messageId)
    {
        try
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
                return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Message {MessageId} deleted", messageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            throw;
        }
    }
}
