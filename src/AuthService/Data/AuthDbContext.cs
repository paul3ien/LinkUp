global using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

/// <summary>
/// T020: AuthDbContext pour persistence User
/// Driver : Npgsql.EntityFrameworkCore.PostgreSQL
/// Base de données : linkup_auth_db
/// Approche : Code-First, migrations EF Core
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    // T020: DbSet pour User entity
    public DbSet<AuthService.Models.User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // T020: Configuration User entity
        var userEntity = modelBuilder.Entity<AuthService.Models.User>();
        userEntity.HasKey(u => u.Id);
        userEntity.Property(u => u.Email).IsRequired().HasMaxLength(255);
        userEntity.Property(u => u.PasswordHash).IsRequired();
        userEntity.HasIndex(u => u.Email).IsUnique();
    }
}
