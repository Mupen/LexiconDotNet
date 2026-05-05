# Architecture

Version: v0.1.0

## Architecture Style

The project uses layered architecture.

Dependency direction:

```text
Api -> Application -> Domain
Infrastructure -> Application -> Domain
```

The Domain layer is the center of the system. It does not depend on ASP.NET Core, Entity Framework Core, SQLite, Swagger, Docker, or xUnit.

## Layer Responsibilities

| Layer | Responsibility |
| --- | --- |
| Api | Controllers, request DTOs, response DTOs, HTTP result mapping |
| Application | Use cases, queries, request models, read models, repository interfaces |
| Domain | Entities, enums, validation, business rules, Result contracts |
| Infrastructure | EF Core DbContext, repositories, migrations, seed data |
| UnitTests | Tests for domain, application, query, and persistence behavior |

## Project Layout

```text
ShoppingCart.Api             HTTP API, Swagger, DTOs, response mapping
ShoppingCart.Application     Use cases, queries, repository contracts, read models
ShoppingCart.Domain          Entities, enums, business rules, Result contracts
ShoppingCart.Infrastructure  EF Core, SQLite, migrations, repositories, seed data
ShoppingCart.UnitTests       xUnit tests and test repositories
```

## Domain Rules

Business rules belong in the Domain layer.

Examples:

- product price cannot be negative
- tracked products need stock quantity
- cart item quantity must be valid
- products must be active and available before being added to a cart
- shopping carts must belong to a non-empty `UserId`
- product categories cannot be their own parent

Controllers should call application use cases and map the result to HTTP. Controllers should not contain business rules.

## Use Case Pattern

Application workflows are represented as explicit use cases.

Examples:

- `CreateProduct`
- `UpdateProductDetails`
- `ChangeProductPrice`
- `CreateProductBrand`
- `CreateProductCategory`
- `CreateShoppingCart`
- `SetShoppingCartItem`
- `DeleteShoppingCart`

This keeps workflow logic centralized and easier to test.

## Query Pattern

Read operations use explicit query classes.

Examples:

- `GetAllProducts`
- `GetAvailableProducts`
- `GetProductById`
- `GetAllShoppingCarts`
- `GetShoppingCartsByUserId`

Product queries return a read model with brand and category names so the API response is easier to use from Swagger or a future frontend.

## Repository Interfaces

Repository interfaces live in the Application layer. EF Core implementations live in Infrastructure.

This keeps use cases independent of the database implementation while still allowing the API to use SQLite through dependency injection.

## Result Pattern

Expected business failures return `Result` or `Result<T>`.

Examples:

- duplicate SKU
- missing product
- invalid quantity
- insufficient stock

This avoids using exceptions for normal validation and business errors.

## Read Models

Read models are used when API responses need display-friendly data.

Example: a product stores `BrandId` and `CategoryId`, but the API response can also include `BrandName` and `CategoryName`.

This keeps the domain model clean while making the API easier to consume.

## API Boundary

The API layer exposes controllers for:

- products
- product brands
- product categories
- shopping carts

The user foundation does not have API controllers in this version.

## User + UserRole Decision

The project uses one `User` entity with a `UserRole` enum instead of subclasses such as `Admin`, `Customer`, and `Clerk`.

Reason:

- shared account data stays in one place
- future authorization can check roles
- it matches common ASP.NET Core authentication patterns
- it avoids duplicated fields across separate user subclasses

In this version, user/account code is placeholder/foundation only. It is not a completed authentication system.
