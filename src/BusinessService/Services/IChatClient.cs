// T033 - Chat gRPC Client Interface
namespace BusinessService.Services;

public interface IChatClient
{
    /// <summary>
    /// T033: Notify notification service about new message
    /// </summary>
    Task<bool> BroadcastMessageAsync(Guid channelId, Guid messageId, string userId, string content);
}
