global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using BusinessService.Data;

// T001: BusinessService WebAPI - Minimal APIs (ASP.NET Core 8.0)
// Responsabilités : Logique métier (Channels, Messages, gRPC Client)

var builder = WebApplication.CreateBuilder(args);

// T010: Configuration EF Core + Npgsql (PostgreSQL)
// Lecture ConnectionString depuis appsettings.json
// DbContext : BusinessDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BusinessDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
