FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend/Api/ReactNews.Api/ReactNews.Api.csproj Backend/Api/ReactNews.Api/
COPY Backend/Application/ReactNews.Application/ReactNews.Application.csproj Backend/Application/ReactNews.Application/
COPY Backend/Domain/ReactNews.Domain/ReactNews.Domain.csproj Backend/Domain/ReactNews.Domain/
COPY Backend/Infrastructure/ReactNews.Infrastructure/ReactNews.Infrastructure.csproj Backend/Infrastructure/ReactNews.Infrastructure/
RUN dotnet restore Backend/Api/ReactNews.Api/ReactNews.Api.csproj

COPY Backend/ Backend/
RUN dotnet publish Backend/Api/ReactNews.Api/ReactNews.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ReactNews.Api.dll"]
