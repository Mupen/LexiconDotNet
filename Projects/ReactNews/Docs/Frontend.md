# Frontend

The frontend is one Vite app:

```text
Frontend/Public
```

It contains public routes, reader routes, and admin routes. There is no separate `Private` or `Shared` frontend app right now.

Useful files:

| File | Purpose |
| --- | --- |
| `src/App.jsx` | top-level routes and navigation |
| `src/api/common/apiClient.js` | shared fetch helper |
| `src/api/articles/newsApi.js` | backend API calls |
| `src/hooks/articles` | server-state hooks |
| `src/pages` | route pages |
| `src/components` | reusable UI components |
| `src/styles.css` | main CSS entry point |
| `playwright.config.js` | E2E test setup |

Run frontend unit tests:

```powershell
cd Projects\ReactNews\Frontend\Public
npm run test
```

Run Playwright E2E:

```powershell
cd Projects\ReactNews\Frontend\Public
npm run test:e2e
```

For detailed routes and folder layout, see [FrontendStructure.md](FrontendStructure.md).
