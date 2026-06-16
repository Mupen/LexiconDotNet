FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Api/CleanBookingV2.Api/CleanBookingV2.Api.csproj Api/CleanBookingV2.Api/
COPY Application/CleanBookingV2.Application/CleanBookingV2.Application.csproj Application/CleanBookingV2.Application/
COPY Domain/CleanBookingV2.Domain/CleanBookingV2.Domain.csproj Domain/CleanBookingV2.Domain/
COPY Infrastructure/CleanBookingV2.Infrastructure/CleanBookingV2.Infrastructure.csproj Infrastructure/CleanBookingV2.Infrastructure/

RUN dotnet restore Api/CleanBookingV2.Api/CleanBookingV2.Api.csproj

COPY Api/ Api/
COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/

RUN dotnet publish Api/CleanBookingV2.Api/CleanBookingV2.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/Data
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CleanBookingV2.Api.dll"]
