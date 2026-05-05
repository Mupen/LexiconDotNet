# Runbook

Version: v0.1.0

## Run Locally

From the repository root:

```powershell
dotnet restore Workshops\ShoppingCart\ShoppingCart.Api\ShoppingCart.Api.csproj
dotnet run --project Workshops\ShoppingCart\ShoppingCart.Api\ShoppingCart.Api.csproj --launch-profile http
```

Open Swagger:

```text
http://localhost:5088/swagger
```

## Run With Docker

From the repository root:

```powershell
docker compose -f Workshops\ShoppingCart\docker-compose.yml up --build
```

Open Swagger:

```text
http://localhost:5088/swagger
```

Stop Docker:

```powershell
docker compose -f Workshops\ShoppingCart\docker-compose.yml down
```

Reset the Docker database volume:

```powershell
docker compose -f Workshops\ShoppingCart\docker-compose.yml down -v
```

## Build

```powershell
dotnet restore Workshops\ShoppingCart\ShoppingCart.Api\ShoppingCart.Api.csproj
dotnet restore Workshops\ShoppingCart\ShoppingCart.UnitTests\ShoppingCart.UnitTests.csproj
dotnet build Workshops\ShoppingCart\ShoppingCart.Api\ShoppingCart.Api.csproj --no-restore
dotnet build Workshops\ShoppingCart\ShoppingCart.UnitTests\ShoppingCart.UnitTests.csproj --no-restore
```

## Test

```powershell
dotnet build Workshops\ShoppingCart\ShoppingCart.UnitTests\ShoppingCart.UnitTests.csproj --no-restore
dotnet test Workshops\ShoppingCart\ShoppingCart.UnitTests\ShoppingCart.UnitTests.csproj --no-build --no-restore
```

## Reset Local SQLite Database

Stop the API first.

Delete files from:

```text
Workshops\ShoppingCart\ShoppingCart.Api\Data\
```

The next API start recreates the database, applies migrations, and seeds development data.

## Notes

- The local HTTP launch profile uses `http://localhost:5088`.
- The Docker Compose setup maps host port `5088` to container port `8080`.
- Database migrations run automatically when the API starts.
- Development seed data is added only when `ASPNETCORE_ENVIRONMENT` is `Development`.
