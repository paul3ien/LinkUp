// T031 - Message Service Interface
namespace BusinessService.Services;

using BusinessService.Models;

public interface IMessageService
{
    /// <summary>
    /// T031: Get messages by channel (paginated)
    /// </summary>
    Task<List<Message>> GetMessagesByChannelAsync(Guid channelId, int page = 1, int pageSize = 20);

    /// <summary>
    /// T031: Get message by ID
    /// </summary>
    Task<Message?> GetMessageByIdAsync(Guid messageId);

    /// <summary>
    /// T031: Create new message
    /// </summary>
    Task<Message> CreateMessageAsync(Guid channelId, string userId, string content);

    /// <summary>
    /// T031: Delete message
    /// </summary>
    Task<bool> DeleteMessageAsync(Guid messageId);
}
