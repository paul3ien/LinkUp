global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using NotificationService.Services;

// T001: NotificationService gRPC - Real-time notifications
// Responsabilités : Broadcasting messages, Connection management, gRPC-Web support

var builder = WebApplication.CreateBuilder(args);

// T040: gRPC over plaintext HTTP/2 so grpcurl/grpcui can connect (localhost)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// T040: gRPC server + Reflection for debugging (grpcui, Postman gRPC)
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// T043: CORS for gRPC-Web (browser clients)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("grpc-status", "grpc-message", "grpc-encoding", "grpc-accept-encoding");
    });
});

// T041: Register connection manager for subscriber tracking
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

var app = builder.Build();

app.UseRouting();

// T043: gRPC-Web middleware (must be before MapGrpcService)
app.UseGrpcWeb();
app.UseCors();

// T043: Enable gRPC-Web on ChatGrpcService endpoint
app.MapGrpcService<NotificationService.Services.ChatGrpcService>().EnableGrpcWeb();
app.MapGrpcReflectionService();

app.Run();

