FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so Docker can cache NuGet restore between source edits.
COPY WebCV.Domain/WebCV.Domain.csproj WebCV.Domain/
COPY WebCV.Application/WebCV.Application.csproj WebCV.Application/
COPY WebCV.Infrastructure/WebCV.Infrastructure.csproj WebCV.Infrastructure/
COPY WebCV.Api/WebCV.Api.csproj WebCV.Api/

RUN dotnet restore WebCV.Api/WebCV.Api.csproj

COPY . .
RUN dotnet publish WebCV.Api/WebCV.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Development keeps Swagger available inside the workshop container.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ConnectionStrings__WebCV="Data Source=/app/Data/webcv.db"
ENV SeedData__DefaultCvPath="SeedData/default-cv.json"

COPY --from=build /app/publish .

# SQLite writes to /app/Data, which docker-compose mounts as a named volume.
RUN mkdir -p /app/Data

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebCV.Api.dll"]
