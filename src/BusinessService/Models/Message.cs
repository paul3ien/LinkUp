namespace BusinessService.Models;

/// <summary>
/// T032: Message entity pour persistance chat
/// Propriétés : Id, ChannelId, UserId, Content, CreatedAt
/// Relation : Many-to-One avec Channel
/// Utilisation : POST /channels/{id}/messages, gRPC broadcast
/// </summary>
public class Message
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // T032: Relation Many-to-One
#pragma warning disable CS8618
    public Channel Channel { get; set; }
#pragma warning restore CS8618
}
