global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using BusinessService.Data;
global using BusinessService.Services;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using System.Text;

// T001: BusinessService WebAPI - Minimal APIs (ASP.NET Core 8.0)
// Responsabilités : Logique métier (Channels, Messages, gRPC Client)

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use port 7001 for HTTP
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(7001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// T010: Configuration EF Core + Npgsql (PostgreSQL)
// Lecture ConnectionString depuis appsettings.json
// DbContext : BusinessDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BusinessDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// T030: Dependency Injection - Services
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IChatClient, ChatClient>();  // T033: gRPC Client

// T032: Authentication - JWT Bearer Token
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
var issuer = jwtSection["Issuer"] ?? "LinkUp";
var audience = jwtSection["Audience"] ?? "LinkUpClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // T031: Prevent circular reference serialization (Message → Channel → Messages → ...)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// T070: CORS Configuration - Allow frontend (all localhost ports) to call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            return new Uri(origin).Host == "localhost" || new Uri(origin).Host == "127.0.0.1";
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

// T010: Auto-apply migrations on startup for development
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BusinessDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Migration failed");
}

// T070: CORS Middleware must come before Authentication/Authorization
app.UseCors();
// T032: Authentication Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
