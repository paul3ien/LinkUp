global using Microsoft.EntityFrameworkCore;

namespace BusinessService.Data;

/// <summary>
/// T030: BusinessDbContext pour persistence métier
/// Driver : Npgsql.EntityFrameworkCore.PostgreSQL
/// Base de données : linkup_business_db
/// Entités : Channel, Message (One-to-Many)
/// Approche : Code-First, migrations EF Core
/// </summary>
public class BusinessDbContext : DbContext
{
    public BusinessDbContext(DbContextOptions<BusinessDbContext> options) : base(options)
    {
    }

    // T030: DbSet pour Channel entity
    public DbSet<BusinessService.Models.Channel> Channels { get; set; } = null!;

    // T032: DbSet pour Message entity
    public DbSet<BusinessService.Models.Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // T030: Configuration Channel entity
        var channelEntity = modelBuilder.Entity<BusinessService.Models.Channel>();
        channelEntity.HasKey(c => c.Id);
        channelEntity.Property(c => c.Name).IsRequired().HasMaxLength(255);
        channelEntity.Property(c => c.CreatedBy).IsRequired();
        channelEntity.HasMany(c => c.Messages)
            .WithOne(m => m.Channel)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        // T032: Configuration Message entity
        var messageEntity = modelBuilder.Entity<BusinessService.Models.Message>();
        messageEntity.HasKey(m => m.Id);
        messageEntity.Property(m => m.UserId).IsRequired();
        messageEntity.Property(m => m.Content).IsRequired();
        messageEntity.HasIndex(m => m.ChannelId);
    }
}
