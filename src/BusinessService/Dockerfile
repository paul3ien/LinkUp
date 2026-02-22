# Dockerfile for BusinessService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/BusinessService/ src/BusinessService/
COPY src/Shared/ src/Shared/
RUN dotnet restore src/BusinessService/BusinessService.csproj

RUN dotnet publish src/BusinessService/BusinessService.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BusinessService.dll"]
