import '@testing-library/jest-dom/vitest'

/*
 * What: This file configures the browser-like test environment used by Vitest.
 * How: importing jest-dom adds readable assertions such as toBeInTheDocument,
 * while Vitest provides the jsdom DOM configured in vite.config.js.
 * Why: React component tests should read like user-facing behavior checks, not
 * low-level DOM property checks.
 */
