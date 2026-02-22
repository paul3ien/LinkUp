// T042 - gRPC Chat Service: real Subscribe & Broadcast implementation
using Grpc.Core;
using LinkUp.Chat;
using Google.Protobuf.WellKnownTypes;

namespace NotificationService.Services;

/// <summary>
/// T042: ChatService gRPC implementation
/// Subscribe: open stream to receive messages
/// BroadcastMessage: receive message from BusinessService and send to subscribers
/// </summary>
public class ChatGrpcService : ChatService.ChatServiceBase
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<ChatGrpcService> _logger;

    public ChatGrpcService(IConnectionManager connectionManager, ILogger<ChatGrpcService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    /// <summary>
    /// T042: Subscribe - client opens stream to receive real-time messages
    /// Keeps connection alive until client disconnects or error
    /// </summary>
    public override async Task Subscribe(
        SubscribeRequest request,
        IServerStreamWriter<Message> responseStream,
        ServerCallContext context)
    {
        var channelId = Guid.Parse(request.ChannelId);
        var userId = request.UserId;

        _logger.LogInformation("T042: Client subscribing - User {User} → Channel {Channel}", userId, channelId);

        // T042: Register stream writer for this channel
        await _connectionManager.RegisterSubscriberAsync(channelId, userId, responseStream);

        try
        {
            // T042: Keep connection alive waiting for channel cancellation
            await Task.Delay(Timeout.Infinite, context.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("T042: Client disconnected - User {User} from Channel {Channel}", userId, channelId);
        }
        finally
        {
            // T042: Cleanup on disconnect
            await _connectionManager.UnregisterSubscriberAsync(channelId, userId);
        }
    }

    /// <summary>
    /// T033/T042: Receive broadcast from BusinessService and send to all subscribers
    /// </summary>
    public override async Task<Empty> BroadcastMessage(
        Message request,
        ServerCallContext context)
    {
        var channelId = Guid.Parse(request.ChannelId);

        _logger.LogInformation(
            "T042: Processing broadcast in channel {Channel}: '{Content}' from {User}",
            channelId, request.Content, request.UserId);

        // T042: Send to all subscribers in channel
        await _connectionManager.BroadcastAsync(channelId, request);

        return new Empty();
    }
}

