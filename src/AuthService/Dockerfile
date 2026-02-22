# Dockerfile for AuthService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and restore
COPY *.sln .
COPY src/AuthService/ src/AuthService/
COPY src/Shared/ src/Shared/
RUN dotnet restore src/AuthService/AuthService.csproj

# Build + publish
RUN dotnet publish src/AuthService/AuthService.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthService.dll"]
