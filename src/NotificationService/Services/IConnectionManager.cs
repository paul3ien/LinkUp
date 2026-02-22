// T041 - Connection Manager Interface
using Grpc.Core;
using LinkUp.Chat;

namespace NotificationService.Services;

/// <summary>
/// T041: Subscriber connection management contract
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// T041: Register subscriber to receive messages from channel
    /// </summary>
    Task RegisterSubscriberAsync(Guid channelId, string userId, IServerStreamWriter<Message> writer);

    /// <summary>
    /// T041: Remove subscriber (on disconnect)
    /// </summary>
    Task UnregisterSubscriberAsync(Guid channelId, string userId);

    /// <summary>
    /// T042: Send message to all subscribers in channel
    /// </summary>
    Task BroadcastAsync(Guid channelId, Message message);

    /// <summary>
    /// T041: Get active subscriber count for monitoring
    /// </summary>
    int GetSubscriberCount(Guid channelId);
}
