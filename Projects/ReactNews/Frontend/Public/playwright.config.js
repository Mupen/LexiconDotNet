import { defineConfig, devices } from '@playwright/test'

/*
 * What: Playwright configuration for ReactNews browser tests.
 * How: webServer starts the real ASP.NET Core API and Vite frontend before the
 * tests run, then Playwright drives Chromium against the frontend URL.
 * Why: unit tests prove isolated code, while E2E tests prove the real browser,
 * auth cookies, API routes, EF migrations, and React UI work together.
 */
export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  expect: {
    timeout: 8_000
  },
  fullyParallel: false,
  reporter: [['list']],
  use: {
    baseURL: 'http://127.0.0.1:5174',
    trace: 'retain-on-failure'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ],
  webServer: [
    {
      command: 'powershell -ExecutionPolicy Bypass -File e2e/Start-ReactNewsApiForPlaywright.ps1',
      url: 'http://127.0.0.1:5227/api/health',
      timeout: 60_000,
      reuseExistingServer: false
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --port 5174',
      url: 'http://127.0.0.1:5174',
      timeout: 60_000,
      reuseExistingServer: false,
      env: {
        VITE_API_BASE_URL: 'http://127.0.0.1:5227'
      }
    }
  ]
})
