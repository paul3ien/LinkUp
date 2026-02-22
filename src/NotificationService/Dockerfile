# Dockerfile for NotificationService (gRPC / HTTP2)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/NotificationService/ src/NotificationService/
COPY src/Shared/ src/Shared/
RUN dotnet restore src/NotificationService/NotificationService.csproj

RUN dotnet publish src/NotificationService/NotificationService.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
# HTTP/2 gRPC on port 5000 (configured in Program.cs via Kestrel)
EXPOSE 5000
ENTRYPOINT ["dotnet", "NotificationService.dll"]
