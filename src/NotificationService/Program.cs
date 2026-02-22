global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;

// T001: NotificationService gRPC - Real-time notifications
// Responsabilités : Broadcasting messages, Connection management, gRPC-Web support

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.UseRouting();
app.UseGrpcWeb();

app.UseEndpoints(endpoints =>
{
});

app.Run();
