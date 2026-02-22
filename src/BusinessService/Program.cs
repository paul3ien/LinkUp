global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;

// T001: BusinessService WebAPI - Minimal APIs (ASP.NET Core 8.0)
// Responsabilités : Logique métier (Channels, Messages, gRPC Client)

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
