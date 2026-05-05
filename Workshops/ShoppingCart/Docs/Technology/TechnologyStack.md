# Technology Stack

Version: v0.1.0

## Runtime And Language

| Technology | Use |
| --- | --- |
| C# | Main programming language |
| .NET 10 | Runtime and SDK |
| ASP.NET Core Web API | HTTP API framework |
| Nullable Reference Types | Safer null handling |
| Implicit Usings | Less boilerplate in project files |

## Project Type

| Project | Type |
| --- | --- |
| `ShoppingCart.Api` | ASP.NET Core Web API |
| `ShoppingCart.Application` | Class library |
| `ShoppingCart.Domain` | Class library |
| `ShoppingCart.Infrastructure` | Class library |
| `ShoppingCart.UnitTests` | xUnit test project |

## API Tools

| Technology | Use |
| --- | --- |
| Controllers | Endpoint implementation |
| Swagger / OpenAPI | Manual API testing and endpoint inspection |
| Swashbuckle.AspNetCore | Swagger UI generation |
| Microsoft.AspNetCore.OpenApi | OpenAPI support |
| ProblemDetails | Consistent error response shape |

## Persistence

| Technology | Use |
| --- | --- |
| Entity Framework Core | Database access and mapping |
| SQLite | Local file-based database |
| EF Core Migrations | Database schema versioning |

SQLite was chosen to keep the workshop simple to run without installing a separate database server.

The local development database is stored under:

```text
ShoppingCart.Api/Data/shopping-cart.db
```

## Containerization

| Technology | Use |
| --- | --- |
| Docker | Container runtime |
| Dockerfile | API image build |
| Docker Compose | Local container run setup |
| Visual Studio Docker Profile | Run container from Visual Studio |

## Testing

| Technology | Use |
| --- | --- |
| xUnit | Unit testing framework |
| Microsoft.NET.Test.Sdk | Test execution infrastructure |
| Microsoft.Data.Sqlite | In-memory SQLite persistence tests |
| coverlet.collector | Test coverage collection support |
| Swagger | Manual API verification |

## Main NuGet Packages

- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.VisualStudio.Azure.Containers.Tools.Targets`
- `Swashbuckle.AspNetCore`
- `xunit`
- `xunit.runner.visualstudio`

Supporting EF Core and test packages are used in the Infrastructure and UnitTests projects.
