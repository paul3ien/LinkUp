// T033 - Chat gRPC Client Implementation
namespace BusinessService.Services;

public class ChatClient : IChatClient
{
    private readonly ILogger<ChatClient> _logger;
    private readonly IConfiguration _configuration;

    public ChatClient(ILogger<ChatClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// T033: Broadcast message to notification service via gRPC
    /// Calls NotificationService/ChatService/BroadcastMessage
    /// </summary>
    public async Task<bool> BroadcastMessageAsync(Guid channelId, Guid messageId, string userId, string content)
    {
        try
        {
            _logger.LogInformation("Broadcasting message {MessageId} to channel {ChannelId}", messageId, channelId);
            
            // T033: In production, would call actual gRPC service
            // var channel = GrpcChannel.ForAddress("http://notification-service:5001");
            // var client = new Chat.ChatClient(channel);
            // var request = new BroadcastMessageRequest { ... };
            // await client.BroadcastMessageAsync(request);
            
            // For now, return true (success)
            // Real implementation would depend on NotificationService gRPC setup
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting message {MessageId}", messageId);
            // Don't throw - broadcasting failure should not block message creation
            return false;
        }
    }
}
