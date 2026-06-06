# DataDrivenCaching

DataDrivenCaching is a learning project about data-driven design, storage, and caching in modern web applications.

The project is not meant to be production polished. It is meant to make data decisions visible:

- what kind of data exists
- who owns the data
- where the data should live
- how long the data should live
- whether the data is authoritative or temporary
- whether the data is trusted or user-editable
- whether the data is stored for correctness, convenience, identity, or speed

The main idea repeated throughout the project is:

> Frontend storage is convenience. Backend storage is authority. Cache is speed. Cookies are identity transport.

## Project Goal

This project should behave like a data and storage laboratory.

Each demo should teach both:

1. How a storage or caching mechanism works.
2. Why that mechanism is or is not a good fit for a specific kind of data.

The focus is data-driven design. The storage technology is only one part of the lesson.

Good examples should answer questions like:

- Is this data source-of-truth data or copied data?
- Can the user safely edit this data?
- Can JavaScript access this data?
- Can the backend access this data?
- Does the browser send this data automatically?
- Does this data survive refresh?
- Does this data survive browser restart?
- Does this data survive server restart?
- What breaks if this data is stale?
- What breaks if this data is lost?

## Tech Stack

- Backend: ASP.NET Core Minimal API
- Language: C#
- Database: SQLite
- Data access: Entity Framework Core
- Frontend: plain HTML, CSS, and vanilla JavaScript
- No React
- No frontend framework
- Redis is optional and should start as a mock/architecture demo
- Docker can run the app in a clean container with a persistent SQLite volume

## Current Structure

```text
Projects/DataDrivenCaching/
  README.md
  Dockerfile
  docker-compose.yml
  .dockerignore
  Docs/
    101.txt
  DataDrivenCaching.Api/
    DataDrivenCaching.Api.csproj
    Program.cs
    Data/
      datadrivencaching.db
    wwwroot/
      index.html
      styles.css
      app.js
      service-worker.js
  DataDrivenCaching.Application/
    DataDrivenCaching.Application.csproj
  DataDrivenCaching.Domain/
    DataDrivenCaching.Domain.csproj
  DataDrivenCaching.Infrastructure/
    DataDrivenCaching.Infrastructure.csproj
```

## Project Layers

This project uses the same broad shape as `Workshops/WebCV`, but only because
the database requirement makes the separation useful.

- `DataDrivenCaching.Api`
  - owns HTTP endpoints and static frontend hosting
  - explains request/response behavior, cookies, sessions, and cache headers
- `DataDrivenCaching.Application`
  - owns application use cases and request/response models
  - describes what the app needs without deciding where data physically lives
- `DataDrivenCaching.Domain`
  - owns core data concepts such as users and authoritative lab records
  - should not depend on EF Core, ASP.NET Core, SQLite, or browser behavior
- `DataDrivenCaching.Infrastructure`
  - owns SQLite, EF Core, repositories, and persistence details
  - explains how durable backend data is stored on disk

The goal is not to overengineer the project. The goal is to make each data
decision visible.

This project should avoid inheritance and interfaces until they solve a real
problem. Prefer concrete data records, concrete stores, and clear functions.
Add an interface only when there are multiple real implementations, a useful
testing boundary, or a plugin-style extension point.

## Planned Demo Areas

The app should be organized around data categories first, then storage mechanisms.

Planned sections:

- Data classification
- JavaScript runtime memory
- `sessionStorage`
- `localStorage`
- cookies
- ASP.NET Session
- IndexedDB
- Cache API
- Service Worker
- HTTP cache headers
- backend memory cache with `IMemoryCache`
- optional Redis/distributed cache architecture

## Data-Driven Design Principle

Do not start by asking:

> Where can I store this?

Start by asking:

> What kind of data is this?

Then decide storage based on the data's properties:

- ownership
- lifetime
- trust level
- access rules
- size
- sync behavior
- performance needs
- security impact

For example, user theme preference can live in `localStorage` because the user owns it, can edit it, and it is not security sensitive.

Player currency, admin roles, payment status, and inventory ownership must not live authoritatively in frontend storage because the user can modify frontend data.

Passwords are a special case:

- never store raw/plain-text passwords
- only store password hashes
- treat password hashes as sensitive backend data
- raw passwords should only exist briefly in request memory while hashing or verification happens
- frontend storage must never be used as the authority for identity or permissions

## Commenting Standard

This project should be heavily commented on purpose.

Comments should explain what the code does and why the design exists.

Important code blocks should explain:

- what the code does
- why this storage mechanism is being used
- where the data physically lives
- who owns the data
- who can modify the data
- when the data disappears
- whether the data is authoritative
- security implications
- performance implications

Example frontend comment style:

```js
// WHAT:
// This variable stores the currently selected map tile in JavaScript memory.
//
// WHY:
// The selected tile is temporary UI state. It matters while the page is open,
// but it is not important enough to persist after a refresh.
//
// DATA DESIGN:
// This value belongs to the browser UI, not the backend. It is convenience
// state, not authoritative application state.
//
// LIFETIME:
// The value disappears when the page refreshes because JavaScript memory is
// cleared when the document is reloaded.
let selectedMapTile = null;
```

Example backend comment style:

```csharp
// WHAT:
// IMemoryCache stores this fake expensive query result in server RAM.
//
// WHY:
// This represents derived data. The backend can recompute it, so the cache is
// not authoritative. It exists only to make repeated reads faster.
//
// DATA DESIGN:
// The server owns this cached copy, but the source of truth is still the
// original data or computation behind it.
//
// LIFETIME:
// The value disappears when it expires or when the server process restarts.
builder.Services.AddMemoryCache();
```

## Frontend Style

The frontend should be simple, readable, and data-first.

Use JavaScript data objects to describe demos, then render the UI from those objects. This makes the project itself demonstrate data-driven design.

Avoid hiding behavior behind clever abstractions. Prefer explicit, readable code with educational comments.

## Backend Style

Use one ASP.NET Core Minimal API project.

Keep backend endpoints small and focused. Each endpoint should demonstrate one concept clearly:

- setting a cookie
- reading session data
- returning cacheable data
- returning non-cacheable data
- showing memory cache hits and misses

Avoid adding layers until the learning goal needs them.

## Design Inspiration

`Workshops/WebCV` can be used as inspiration for:

- serving a static vanilla frontend from ASP.NET Core
- rendering UI from JavaScript data
- keeping layout and style readable
- using clear CSS sections and comments

This project does not need to copy WebCV's architecture if that would make the storage and data-design lessons harder to understand.

## Build

From the repository root:

```powershell
dotnet build Projects\DataDrivenCaching\DataDrivenCaching.Api\DataDrivenCaching.Api.csproj
```

## Run

From the repository root:

```powershell
dotnet run --project Projects\DataDrivenCaching\DataDrivenCaching.Api\DataDrivenCaching.Api.csproj
```

Then open the local URL shown by ASP.NET Core.

## Docker

From the project folder:

```powershell
cd Projects\DataDrivenCaching
docker compose up --build
```

Open:

```text
http://localhost:5128
```

The Docker Compose setup stores SQLite data in a named volume:

```text
datadrivencaching-data
```

That matters for the data-driven lesson: SQLite is backend authority, so it
should survive container rebuilds. The browser can lose localStorage or cookies,
but the backend database should remain durable until the Docker volume is
removed.

To reset the Docker database:

```powershell
docker compose down -v
docker compose up --build
```
