// T041 - Connection Manager: manages subscribers per channel
using Grpc.Core;
using LinkUp.Chat;

namespace NotificationService.Services;

/// <summary>
/// T041: Thread-safe subscriber management for gRPC streaming
/// Maintains: Guid (channelId) → Dictionary (userId → IAsyncStreamWriter)
/// </summary>
public class ConnectionManager : IConnectionManager
{
    private readonly Dictionary<Guid, Dictionary<string, IServerStreamWriter<Message>>> _subscribers = new();
    private readonly object _lock = new();
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// T041: Register subscriber for channel - replaces existing if userid already connected
    /// </summary>
    public Task RegisterSubscriberAsync(Guid channelId, string userId, IServerStreamWriter<Message> writer)
    {
        lock (_lock)
        {
            if (!_subscribers.ContainsKey(channelId))
            {
                _subscribers[channelId] = new();
            }

            _subscribers[channelId][userId] = writer;
            int count = _subscribers[channelId].Count;
            
            _logger.LogInformation(
                "T041: User {User} subscribed to channel {Channel}. Total: {Count}",
                userId, channelId, count);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// T041: Unregister subscriber - cleanup on disconnect
    /// </summary>
    public Task UnregisterSubscriberAsync(Guid channelId, string userId)
    {
        lock (_lock)
        {
            if (_subscribers.ContainsKey(channelId))
            {
                _subscribers[channelId].Remove(userId);

                if (_subscribers[channelId].Count == 0)
                {
                    _subscribers.Remove(channelId);
                }

                _logger.LogInformation(
                    "T041: User {User} unsubscribed from channel {Channel}",
                    userId, channelId);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// T042: Broadcast message to all subscribers in channel
    /// </summary>
    public async Task BroadcastAsync(Guid channelId, Message message)
    {
        List<(string userId, IServerStreamWriter<Message> writer)> targets = new();

        lock (_lock)
        {
            if (_subscribers.ContainsKey(channelId))
            {
                targets = _subscribers[channelId]
                    .Select(kvp => (kvp.Key, kvp.Value))
                    .ToList();
            }
        }

        _logger.LogInformation("T042: Broadcasting to {Count} subscribers in channel {Channel}",
            targets.Count, channelId);

        // T042: Fire and forget with error handling per subscriber
        var tasks = targets.Select(async (target) =>
        {
            try
            {
                await target.writer.WriteAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "T042: Failed to broadcast to user {User} in channel {Channel}",
                    target.userId, channelId);
                // Remove dead connection
                await UnregisterSubscriberAsync(channelId, target.userId);
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// T041: Monitoring - get subscriber count
    /// </summary>
    public int GetSubscriberCount(Guid channelId)
    {
        lock (_lock)
        {
            return _subscribers.ContainsKey(channelId) ? _subscribers[channelId].Count : 0;
        }
    }
}
