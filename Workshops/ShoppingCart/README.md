# ShoppingCart

## Introduction
ShoppingCart is a backend shopping cart API built with C# and .NET 10. The project represents a small online store backend with products, product brands, product categories, and shopping carts.

The code is structured using layered architecture so domain logic, application workflows, persistence, and HTTP endpoints are separated. It includes SQLite persistence through Entity Framework Core, Swagger for manual API testing, Docker support, and unit tests with xUnit.

The current version is backend only. There is no frontend yet.

---

## Assignment Scope

The workshop requirement was to build a small online store backend with products and shopping carts. Each cart belongs to a user id, contains product items, tracks quantities, and can be created, read, edited, and deleted through the API.

See [Assignment.md](Docs/Assignment/Assignment.md) for the full requirement mapping.

---

## Extras

- `.gitignore` for the project
- `README.md` for the project
- Layered architecture (Domain, Application, Infrastructure, API)
- SQLite database with EF Core migrations
- Swagger/OpenAPI for manual API testing
- Docker and Docker Compose support
- Development seed data
- Use of records for request and response DTOs
- Structured query and use case pattern
- Result pattern for expected business errors
- Unit tests for domain, application, query, and persistence behavior

---

## What I Built

I built a backend shopping cart API with a focus on clean architecture, business rules, persistence, and API testing through Swagger.

The project is structured into multiple layers:

```text
ShoppingCart.Api             Controllers, DTOs, Swagger, HTTP result mapping
ShoppingCart.Application     Use cases, queries, requests, read models, interfaces
ShoppingCart.Domain          Entities, enums, business rules, Result contracts
ShoppingCart.Infrastructure  EF Core, SQLite, migrations, repositories, seed data
ShoppingCart.UnitTests       Unit and persistence tests
```

The system currently supports:

- Product catalog with:
  - SKU, name, description, price, status, brand, and category
  - inventory tracking
  - stock quantity
  - availability calculation

- Product brands:
  - creating brands
  - renaming brands
  - listing and fetching brands

- Product categories:
  - root categories
  - child categories
  - renaming categories
  - moving categories

- Shopping carts:
  - creating a cart for a user id
  - listing all carts
  - fetching one cart by id
  - listing carts by user id
  - adding products to a cart
  - updating product quantity
  - removing a product by setting quantity to zero
  - deleting carts
  - calculating total quantity and total price

- SQLite persistence:
  - EF Core migrations
  - automatic migration on startup
  - development seed data

- Docker support:
  - Dockerfile
  - docker-compose.yml
  - Visual Studio Docker profile

The project also includes a user foundation:

- `User`
- `UserRole`
- user repository
- user use cases
- seeded demo users

This user code is only a placeholder for later authentication and authorization work. There are no user API endpoints, no login, no frontend account flow, no password hashing service, and no role-based controller authorization in this version.

The project includes unit tests verifying:

- product rules
- brand and category rules
- shopping cart behavior
- result contracts
- application use cases
- query behavior
- EF Core persistence behavior

---

## Why I Did It This Way

### Clean separation of concerns
I structured the system into layers to clearly separate responsibilities:

- Domain handles business rules
- Application coordinates use cases and queries
- Infrastructure handles database access
- API handles HTTP requests and responses

This makes the system easier to test, extend, and maintain.

---

### Backend-first API design
The assignment focuses on the shopping cart backend, so I built the backend API first and used Swagger as the manual testing interface.

This makes it possible to verify the API before building a frontend.

---

### Practical cart editing
Instead of creating separate endpoints for increasing and decreasing quantities, the API has one endpoint for setting the item quantity:

```text
PUT /api/shopping-carts/{id}/items
```

If the quantity is greater than zero, the item is added or updated. If the quantity is zero, the item is removed. This matches the assignment idea that a future frontend can decide how the user changes quantities.

---

### Read models and response DTOs
Instead of exposing domain entities directly, the API returns response DTOs.

For products, the response includes display-friendly values such as:

- brand name
- category name
- availability

This keeps the domain model clean while making the API easier to use from Swagger or a future frontend.

---

### Use case-driven logic
Operations are handled through explicit use cases, such as:

- creating products
- changing product price
- creating shopping carts
- setting shopping cart items
- deleting shopping carts

This keeps business workflows centralized and reusable.

---

### SQLite for simple local persistence
SQLite was used because the workshop project should be easy to run without installing a separate database server.

The API creates and migrates the database automatically when it starts.

---

### Docker support
Docker was added so the API can be run in a container and tested in a more realistic environment.

The project supports:

- command-line Docker Compose
- Visual Studio Docker launch profile

---

### Incremental development
The system was built step by step:

1. Core domain entities and rules
2. Application use cases and queries
3. EF Core persistence and migrations
4. API controllers and DTOs
5. Swagger/manual testing
6. Docker support
7. Documentation cleanup

This made it easier to validate each part before moving forward.

---

## Quick Start

From the repository root:

```powershell
dotnet run --project Workshops\ShoppingCart\ShoppingCart.Api\ShoppingCart.Api.csproj --launch-profile http
```

Open Swagger:

```text
http://localhost:5088/swagger
```

The project can also be run with Docker Compose:

```powershell
docker compose -f Workshops\ShoppingCart\docker-compose.yml up --build
```

Build and test:

```powershell
dotnet build Workshops\ShoppingCart\ShoppingCart.Api\ShoppingCart.Api.csproj
dotnet test Workshops\ShoppingCart\ShoppingCart.UnitTests\ShoppingCart.UnitTests.csproj
```

See [Runbook.md](Docs/Running/Runbook.md) for full local, Docker, build, test, and database reset commands.

---

## AI Disclosure
This project was written by Daniel Henriksen. ChatGPT (AI by OpenAI) was used as a collaborative tool throughout the process.

My workflow was to build and shape the project through my own decisions, then use ChatGPT to:

- identify architectural improvements
- debug issues and trace errors
- review API behavior
- refine structure and naming
- assist with Docker setup
- assist with documentation and explanations

All core logic, structure, and design decisions are my own work.
ChatGPT acted as a reviewer and technical assistant.

---

## Status
Work in progress, but ready as a backend-focused workshop hand-in.

Currently completed:

- product API
- brand and category API
- shopping cart API
- SQLite persistence
- Swagger testing
- Docker support
- unit tests
- documentation

Not completed:

- frontend
- login
- authorization
- checkout
- payment
- order history

The `User` class and role-related code are included only as a foundation/placeholders for future work.

---

## Documentation

The deeper documentation is split by topic:

| Topic | File |
| --- | --- |
| Assignment mapping | [Docs/Assignment/Assignment.md](Docs/Assignment/Assignment.md) |
| Architecture | [Docs/Architecture/Architecture.md](Docs/Architecture/Architecture.md) |
| Domain model | [Docs/Domain/DomainModel.md](Docs/Domain/DomainModel.md) |
| API reference | [Docs/Api/ApiReference.md](Docs/Api/ApiReference.md) |
| Technology stack | [Docs/Technology/TechnologyStack.md](Docs/Technology/TechnologyStack.md) |
| Running, Docker, and database reset | [Docs/Running/Runbook.md](Docs/Running/Runbook.md) |
| Manual Swagger checklist | [Docs/Testing/ManualTestChecklist.md](Docs/Testing/ManualTestChecklist.md) |
| Release notes | [Docs/Project/ReleaseNotes_v0.1.0.md](Docs/Project/ReleaseNotes_v0.1.0.md) |

