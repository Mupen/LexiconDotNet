# WebCV

## Introduction
WebCV is a full-stack CV workshop project built with ASP.NET Core, SQLite, JavaScript, HTML, and CSS.

The backend stores CV data in a local SQLite database and exposes it through a small API. The frontend starts from a minimal `index.html`, fetches JSON from `/api/cv`, and builds the visible CV page dynamically in JavaScript.

The project is intentionally small, but it follows the same layered structure used in the larger workshop projects.

---

## Assignment Scope

The workshop requirement was to translate a CV into a web version using HTML, CSS, lists, links, social links.

This version goes further by adding:

- ASP.NET Core API
- SQLite persistence
- JSON-based development seed data
- responsive frontend rendering
- Docker and Docker Compose support
- Swagger for API inspection
- a black/gold visual theme inspired by the original PDF CV
---

## What I Built

The rendered CV includes:

- profile header with image, name, role, contact details, and about text
- skills section
- education section
- experience section
- project-style work examples
- footer section for hobbies and personal interests
- responsive layout for desktop and mobile

The frontend is static JavaScript and CSS. It does not use a frontend framework.

The backend supports:

```text
GET /api/cv
PUT /api/cv
```

`GET /api/cv` returns the current CV profile.

`PUT /api/cv` replaces the stored profile. This is useful for practicing JSON payloads and could later support an admin/editor UI.

---

## Project Structure

```text
WebCV.Api             ASP.NET Core API host, static frontend, app settings, seed data
WebCV.Application     Queries, use cases, repository contracts, request/response models
WebCV.Domain          CV profile, sections, section items, and social links
WebCV.Infrastructure  EF Core SQLite context, repository implementation, JSON seed loader
```

Important frontend files:

```text
WebCV.Api/wwwroot/index.html
WebCV.Api/wwwroot/app.js
WebCV.Api/wwwroot/styles.css
WebCV.Api/wwwroot/images/Profile_Image_5.png
```

Important backend/data files:

```text
WebCV.Api/Program.cs
WebCV.Api/appsettings.json
WebCV.Api/SeedData/default-cv.json
WebCV.Api/Logs/
WebCV.Infrastructure/Persistence/WebCvSeedData.cs
WebCV.Infrastructure/Persistence/WebCvDbContext.cs
WebCV.Infrastructure/Repositories/EfCvProfileRepository.cs
```

---

## Requirements

- .NET SDK 10
- a modern browser
- Docker Desktop, only if running the Docker Compose version

---

## Seed Data

The default CV content is stored outside the C# code:

```text
WebCV.Api/SeedData/default-cv.json
```

On first startup, the API:

1. creates the SQLite database if it does not exist
2. checks whether a CV profile already exists
3. loads `SeedData/default-cv.json`
4. maps the JSON data into domain entities
5. saves the profile to SQLite

After the database has data, the database is the source of truth. Editing the JSON file will affect a fresh database, but it will not overwrite an existing profile.

To reseed locally, stop the app and remove:

```text
WebCV.Api/Data/webcv.db
WebCV.Api/Data/webcv.db-shm
WebCV.Api/Data/webcv.db-wal
```

Then start the API again.

---

## API

The API exposes the current CV profile as JSON:

```text
GET /api/cv
```

The stored CV profile can be replaced with:

```text
PUT /api/cv
```

The `PUT` body uses the same shape as `WebCV.Api/SeedData/default-cv.json`:

```json
{
  "fullName": "Example Name",
  "title": "Example Title",
  "summary": "Short profile summary.",
  "location": "City, Country",
  "email": "name@example.com",
  "phone": "+46 70 000 00 00",
  "socialLinks": [
    { "label": "GitHub", "url": "https://github.com/", "sortOrder": 1 }
  ],
  "sections": [
    {
      "heading": "Skills",
      "layout": "tags",
      "sortOrder": 1,
      "items": [
        {
          "title": "C#",
          "subtitle": ".NET",
          "period": "Current",
          "description": "Backend development.",
          "tags": [ "ASP.NET Core", "SQLite" ],
          "sortOrder": 1
        }
      ]
    }
  ]
}
```

Supported frontend section layouts:

```text
tags
timeline
experience
cards
interests
```

Swagger UI is enabled in Development at:

```text
http://localhost:5096/swagger
```

---

## Why I Did It This Way

### Layered architecture

The project uses separate Domain, Application, Infrastructure, and API projects. This keeps the domain model away from HTTP and database concerns while still keeping the workshop small enough to follow.

### JSON seed data

The CV content changes more often than the code. Keeping it in JSON makes the text easier to edit and keeps the seeding logic focused on loading and validating data.

### SQLite

SQLite keeps the project easy to run locally and in Docker without installing a separate database server.

### Vanilla frontend

The assignment focuses on HTML and CSS fundamentals, so the frontend uses plain JavaScript DOM creation instead of a framework.

### Docker support

Docker makes it possible to run the API and static frontend in a clean container with a persistent SQLite volume.

---

## Quick Start

From the repository root:

```powershell
dotnet run --project Workshops\WebCV\WebCV.Api\WebCV.Api.csproj --launch-profile http
```

Open:

```text
http://localhost:5096
```

Swagger:

```text
http://localhost:5096/swagger
```

The launch profile uses:

```text
http://localhost:5096
```

---

## Docker

From the WebCV workshop folder:

```powershell
cd Workshops\WebCV
docker compose up --build
```

Open:

```text
http://localhost:5096
```

The Docker Compose setup stores SQLite data in a named volume:

```text
webcv-data
```

To reset the Docker database:

```powershell
docker compose down -v
docker compose up --build
```

---

## Build

From the repository root:

```powershell
dotnet restore Workshops\WebCV\WebCV.Api\WebCV.Api.csproj
dotnet build Workshops\WebCV\WebCV.Api\WebCV.Api.csproj --no-restore -m:1
```

The `-m:1` option keeps the build serial. That avoids noisy project-reference output races on this machine while still compiling all WebCV projects.

---

## Notes for Reviewers

- The frontend is intentionally framework-free and renders the CV from `/api/cv`.
- The database is created with `EnsureCreatedAsync`; EF Core migrations are not included.
- `PUT /api/cv` replaces the single stored profile instead of editing individual sections.
- The local SQLite database files under `WebCV.Api/Data` are runtime data, not source code.
- There is no WebCV test project yet, so verification is currently build plus manual API/browser checks.

---

## Current Status

Completed:

- dynamic frontend rendering from `/api/cv`
- responsive black/gold CV design
- profile image support
- SQLite persistence
- JSON seed data
- Swagger
- Dockerfile
- Docker Compose
- README
- practical comments in CSS, JavaScript, and seeding/startup code

Not currently included:

- unit tests
- EF Core migrations
- frontend editor/admin UI
- authentication
- multiple CV profiles

---

## AI Disclosure

This project was written by Daniel Henriksen. ChatGPT (AI by OpenAI) was used as a collaborative tool throughout the process.

The project direction, content choices, and final design decisions are Daniel Henriksen's.
