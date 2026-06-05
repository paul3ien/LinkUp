// T033 - Chat gRPC Client Implementation
using Grpc.Net.Client;
using LinkUp.Chat;
using Google.Protobuf.WellKnownTypes;

namespace BusinessService.Services;

public class ChatClient : IChatClient
{
    private readonly ILogger<ChatClient> _logger;
    private readonly string _notificationServiceUrl;

    public ChatClient(ILogger<ChatClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _notificationServiceUrl = configuration["NotificationService:GrpcUrl"] ?? "http://localhost:7002";
    }

    /// <summary>
    /// T033: Broadcast message to notification service via gRPC
    /// </summary>
    public async Task<bool> BroadcastMessageAsync(Guid channelId, Guid messageId, string userId, string content)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_notificationServiceUrl, new GrpcChannelOptions
            {
                HttpHandler = new HttpClientHandler()
            });
            var client = new ChatService.ChatServiceClient(channel);

            var request = new Message
            {
                Id = messageId.ToString(),
                ChannelId = channelId.ToString(),
                UserId = userId,
                Content = content,
                CreatedAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
            };

            await client.BroadcastMessageAsync(request);
            _logger.LogInformation("T033: Broadcasted message {MessageId} to channel {ChannelId}", messageId, channelId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "T033: Failed to broadcast message {MessageId} (NotificationService unreachable?)", messageId);
            // Don't throw – broadcasting failure must not block message persistence
            return false;
        }
    }
}
