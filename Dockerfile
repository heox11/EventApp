# Use .NET 9 SDK to build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files correctly
COPY EventRegistration.sln ./
COPY EventManagement.API/ EventManagement.API/
COPY EventManagement.API.Tests/ EventManagement.API.Tests/

# Restore dependencies
RUN dotnet restore EventRegistration.sln

# Build and publish project
RUN dotnet publish EventManagement.API/EventManagement.API.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventManagement.API.dll"]
