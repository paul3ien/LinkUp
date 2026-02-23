global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using AuthService.Data;
global using AuthService.Services;

// T001: AuthService WebAPI - Minimal APIs (ASP.NET Core 8.0)
// Responsabilités : Authentification, Registration, JWT Token Generation

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use port 7000 for HTTP
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(7000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// T010: Configuration EF Core + Npgsql (PostgreSQL)
// Lecture ConnectionString depuis appsettings.json
// DbContext : AuthDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// T021, T022: Enregistrer services d'authentification
builder.Services.AddScoped<IAuthService, AuthenticationService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// T070: CORS Configuration - Allow frontend (localhost:4200) to call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

app.UseHttpsRedirection();
// T070: CORS Middleware must come before Authentication/Authorization
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
