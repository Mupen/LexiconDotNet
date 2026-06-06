# DataDrivenCaching Project Plan

This plan turns the broad guidance from `101.txt` into the current concrete direction for the project.

The project should be a data-driven design learning lab. Storage and caching features should not be demonstrated as isolated tricks. They should be demonstrated through a small website where every feature has a clear data reason.

Main project idea:

> Build a small website with login, user settings, theme switching, JSON import/export, cached data, offline behavior, and a learning dashboard that explains what is happening while the user interacts with the site.

Core rule:

> Start with the data. Then choose the storage.

## Learning Goals

The site should teach these distinctions repeatedly:

- frontend storage is convenience
- backend storage is authority
- cache is speed
- cookies are identity transport
- sessions are backend state connected to a browser by a cookie
- user-editable browser data must not be trusted for security decisions
- passwords must never be stored as plain text
- cached data may be stale
- different data types need different storage systems

## Data-Driven Design Rules

This project should prefer:

- simple data records
- explicit data shapes
- concrete classes
- clear functions
- readable control flow
- comments that explain why data belongs where it belongs

This project should avoid by default:

- inheritance
- interfaces without a real second implementation
- generic abstractions
- reflection-based registration
- hidden framework magic
- clever patterns that make data movement harder to see

Interfaces are allowed only when they solve a real problem, for example:

- there are multiple real implementations
- there is a useful testing boundary
- there is a plugin-style extension point
- the abstraction makes the data flow easier to understand

## Planned Project Structure

```text
Projects/DataDrivenCaching/
  README.md
  Dockerfile
  docker-compose.yml
  .dockerignore
  Docs/
    101.txt
    ProjectPlan.md
  DataDrivenCaching.Api/
    Program.cs
    Data/
      datadrivencaching.db
    wwwroot/
      index.html
      styles.css
      app.js
      service-worker.js
  DataDrivenCaching.Application/
  DataDrivenCaching.Domain/
  DataDrivenCaching.Infrastructure/
```

## Main User-Facing Demo

The website should feel like a small control panel, not a marketing page.

The first screen should show:

- login panel
- current user/session status
- theme/settings controls
- storage/caching demo controls
- learning dashboard

The learning dashboard is important. It should explain what is happening while the user uses the site.

## Demo 1: Accounts, Login, And Session

### Purpose

Teach backend authority, password hashing, cookies, and server-side session state.

### Data

Use three predefined accounts stored in SQLite.

Example:

```text
alice
bob
charlie
```

Each account should have:

- id
- username
- password hash
- display name
- created timestamp

### Important Security Rule

Do not store raw passwords in SQLite.

The database stores password hashes only.

The raw password should only exist briefly in request memory during login.

### Login Flow

1. User enters username and password.
2. Browser sends credentials to `/api/login`.
3. Backend loads the user from SQLite.
4. Backend verifies the password against the stored password hash.
5. Backend writes trusted login state to ASP.NET Session.
6. Browser receives a session cookie.
7. Browser does not receive the password hash.
8. UI asks `/api/session` who is logged in.

### Data Lessons

- SQLite is the authority for accounts.
- Password hash is sensitive backend data.
- Browser form input is untrusted request data.
- Session cookie transports identity, but does not contain the account data.
- ASP.NET Session data lives on the backend.
- The user can delete cookies and lose the session.
- The user must not be able to change their role or identity through localStorage.

## Demo 2: Theme And User Settings

### Purpose

Teach frontend convenience data, JSON serialization, and user-owned settings.

### Data

The site should have predefined theme/settings data.

Example settings:

```json
{
  "themeName": "midnight",
  "density": "comfortable",
  "accentColor": "green",
  "showLearningDashboard": true
}
```

### Storage

Use `localStorage` for browser-owned settings.

### Features

- switch theme
- change display density
- save settings to `localStorage`
- load settings from `localStorage`
- reset settings
- download settings as JSON
- import settings from JSON

### Data Lessons

- Theme settings are user-owned convenience data.
- `localStorage` survives refresh and browser restart.
- `localStorage` can be edited manually in DevTools.
- Imported JSON is untrusted input and must be validated.
- Theme settings are safe to store client-side because they are not security decisions.
- Do not store roles, payment status, currency, or permissions in `localStorage`.

## Demo 3: Temporary Page State

### Purpose

Teach the difference between runtime memory and tab-scoped storage.

### Runtime Memory Examples

- currently selected dashboard panel
- last hovered storage card
- unsaved UI-only state

### `sessionStorage` Examples

- selected tab for the current browser tab
- temporary form draft
- current walkthrough step

### Data Lessons

- JavaScript memory disappears on refresh.
- `sessionStorage` survives refresh but not tab close.
- temporary UI state should not be sent to the backend unless the backend needs it.
- temporary UI state is not authoritative.

## Demo 4: Authoritative Data Vs Cached Copy

### Purpose

Teach the difference between source-of-truth data and cached data.

### Data

Use `LabDataRecord` rows stored in SQLite.

Each record has:

- id
- name
- value
- updated timestamp

### Features

- save authoritative value to SQLite
- load authoritative value from backend
- save a frontend copy
- compare cached/frontend copy with backend value
- update backend value and show that the copy is stale

### Data Lessons

- SQLite is authority.
- Browser copies are convenience.
- Cached copies can be stale.
- Stale data is not always bad, but it must be understood.
- Important decisions must use authoritative backend data.

## Demo 5: Backend Memory Cache

### Purpose

Teach server-side caching with `IMemoryCache`.

### Scenario

Create a fake expensive query.

Example:

- first request waits 1-2 seconds
- result is saved in `IMemoryCache`
- second request is fast
- cache expires after a short time

### Data Lessons

- backend memory cache lives in server RAM
- cache is not authority
- cache disappears on server restart
- cache can become stale
- cache improves speed, not correctness

## Demo 6: HTTP Cache Headers

### Purpose

Teach browser-managed HTTP caching.

### Endpoints

Planned examples:

- `/api/http-cache/no-store`
- `/api/http-cache/short`
- `/api/http-cache/immutable`

### Data Lessons

- browsers can cache HTTP responses automatically
- cache headers control caching behavior
- `no-store` is useful for sensitive or always-fresh data
- `max-age` allows temporary reuse
- stale API responses can confuse users if used incorrectly

## Demo 7: IndexedDB

### Purpose

Teach structured browser database storage.

### Scenario

Store local/offline copies of lab records or user notes.

### Data Lessons

- IndexedDB is browser-side structured storage
- it is asynchronous
- it can store larger structured data than `localStorage`
- users can delete or edit it
- it is not backend authority

## Demo 8: Cache API And Service Worker

### Purpose

Teach offline app shell caching and response caching.

### Features

- register service worker
- cache app shell files
- provide offline fallback page
- optionally cache a sample JSON response

### Data Lessons

- Service Worker acts as a background network layer
- Cache API stores HTTP responses
- cached app shell is not the same as logged-in backend access
- offline UI can load while live API calls fail
- offline-first behavior must be designed deliberately

## Demo 9: Learning Dashboard

### Purpose

Make the learning visible while the user uses the website.

The dashboard should update after important actions and explain:

- what happened
- where the data went
- who owns the data
- whether the data is trusted
- whether the data is authoritative
- whether the data is temporary or persistent
- whether the browser or backend can access it
- whether the user can edit it

### Example Dashboard State

```js
const learningState = {
  session: {
    loggedInUser: null,
    sessionCookieSeen: false,
    backendSessionConfirmed: false
  },
  theme: {
    currentTheme: "default",
    source: "predefined settings",
    savedToLocalStorage: false,
    importedFromJson: false
  },
  cache: {
    lastBackendCacheResult: "not tested",
    lastHttpCacheEndpoint: null
  },
  events: []
};
```

### Example Learning Event

```js
{
  area: "Theme Settings",
  action: "Saved theme",
  storage: "localStorage",
  authority: "Client convenience data",
  explanation: "The user owns this setting, so localStorage is acceptable."
}
```

### Data Lessons

- the dashboard itself is data-driven
- UI can be rendered from explicit state objects
- data movement becomes easier to understand when it is recorded
- learning events make invisible storage behavior visible

## Suggested Implementation Order

1. Update database model for users and lab records.
2. Seed three predefined users with password hashes.
3. Add login, logout, and session status endpoints.
4. Create the first static HTML/CSS/JS shell.
5. Add the learning dashboard state and event log.
6. Add theme switching from predefined settings.
7. Save/load theme settings with `localStorage`.
8. Add JSON export/import for settings.
9. Add temporary UI state with memory and `sessionStorage`.
10. Add authoritative data editor backed by SQLite.
11. Add cached copy comparison.
12. Add backend `IMemoryCache` demo.
13. Add HTTP cache header demo endpoints.
14. Add IndexedDB demo.
15. Add Cache API and Service Worker demo.
16. Review comments and explanations for every feature.
17. Keep Docker support working as the app grows.

## Docker Plan

Docker support should stay simple:

- build the ASP.NET Core app with a multi-stage Dockerfile
- run the published API/static frontend in the ASP.NET runtime image
- expose container port `8080`
- map host port `5128` to container port `8080`
- store SQLite data in a named volume mounted at `/app/Data`

Data lesson:

- the container image is replaceable
- the container filesystem is temporary
- the named volume is durable backend storage
- SQLite remains the backend authority even when the app runs in Docker
- cookies and localStorage still live in the browser, not in the container

## Commenting Requirements

Every important block should explain:

- what the code does
- why the code exists
- what data is being handled
- where the data physically lives
- who owns the data
- who can modify the data
- whether the data is trusted
- whether the data is authoritative
- when the data disappears
- what security risk exists
- what performance tradeoff exists

## Current Direction

The project should not become a generic authentication app or a polished dashboard product.

The goal is a focused learning site where each feature exists because it teaches data-driven design, storage, caching, trust, or lifetime.
