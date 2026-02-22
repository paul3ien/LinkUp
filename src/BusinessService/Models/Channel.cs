namespace BusinessService.Models;

/// <summary>
/// T030: Channel entity pour logique métier
/// Propriétés : Id, Name, CreatedBy, CreatedAt
/// Relation : One-to-Many avec Message
/// Persistée dans linkup_business_db via BusinessDbContext
/// </summary>
public class Channel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // T030: Relation One-to-Many
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
