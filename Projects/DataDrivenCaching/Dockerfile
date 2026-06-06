FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# WHAT:
# Copy project files before source files.
#
# WHY:
# Docker can cache the NuGet restore layer when only C# source, HTML, CSS, or
# JavaScript changes. That makes rebuilds faster while keeping the build steps
# easy to understand.
COPY DataDrivenCaching.Domain/DataDrivenCaching.Domain.csproj DataDrivenCaching.Domain/
COPY DataDrivenCaching.Application/DataDrivenCaching.Application.csproj DataDrivenCaching.Application/
COPY DataDrivenCaching.Infrastructure/DataDrivenCaching.Infrastructure.csproj DataDrivenCaching.Infrastructure/
COPY DataDrivenCaching.Api/DataDrivenCaching.Api.csproj DataDrivenCaching.Api/

RUN dotnet restore DataDrivenCaching.Api/DataDrivenCaching.Api.csproj

COPY . .
RUN dotnet publish DataDrivenCaching.Api/DataDrivenCaching.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# WHAT:
# ASP.NET Core listens on port 8080 inside the container.
#
# WHY:
# Docker Compose maps a host port to this internal port. Keeping the internal
# port fixed makes container behavior predictable even if we change the host
# port later.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# WHAT:
# Override the normal appsettings.json connection string for Docker.
#
# WHY:
# In a container, /app/Data is the physical location where SQLite should write
# the database file. docker-compose mounts that folder as a named volume so the
# authoritative backend data survives image rebuilds and container recreation.
ENV ConnectionStrings__DataDrivenCaching="Data Source=/app/Data/datadrivencaching.db"

COPY --from=build /app/publish .

# WHAT:
# Create the SQLite data directory inside the image.
#
# WHY:
# The app also creates this directory at startup, but making it explicit here
# documents that /app/Data is the container's durable data mount point.
RUN mkdir -p /app/Data

EXPOSE 8080

ENTRYPOINT ["dotnet", "DataDrivenCaching.Api.dll"]
