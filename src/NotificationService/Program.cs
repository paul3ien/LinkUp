global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using NotificationService.Services;

// T001: NotificationService gRPC - Real-time notifications
var builder = WebApplication.CreateBuilder(args);

// T040: Two ports:
//  7002 = HTTP/1.1 only → gRPC-Web (browser via @protobuf-ts)
//  7003 = HTTP/2 only  → native gRPC from BusinessService (no TLS needed with Http2)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(7002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1; // gRPC-Web (browser)
    });
    serverOptions.ListenLocalhost(7003, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2; // native gRPC (backend)
    });
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

var app = builder.Build();

// T043: gRPC endpoints only accept POST — OPTIONS preflight never reaches endpoint CORS.
// Intercept OPTIONS manually before any routing to return CORS headers immediately.
app.Use(async (context, next) =>
{
    const string origin = "http://localhost:4200";
    context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
    context.Response.Headers.Append("Access-Control-Expose-Headers",
        "grpc-status, grpc-message, grpc-encoding, grpc-accept-encoding, x-grpc-web");

    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.Headers.Append("Access-Control-Allow-Methods", "POST, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers",
            "content-type, x-grpc-web, x-user-agent, authorization, grpc-timeout");
        context.Response.Headers.Append("Access-Control-Max-Age", "7200");
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});

app.UseRouting();
app.UseGrpcWeb();

app.MapGrpcService<NotificationService.Services.ChatGrpcService>().EnableGrpcWeb();
app.MapGrpcReflectionService();

app.Run();

