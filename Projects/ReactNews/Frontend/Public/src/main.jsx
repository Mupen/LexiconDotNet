import { StrictMode } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App.jsx'
import { queryClient } from './app/queryClient.js'
import './styles.css'

/*
 * What: this is the browser entry point for the public React app.
 * How: React mounts App into the #root element from index.html and wraps it in
 * the providers that the app needs: StrictMode, TanStack Query, and React Router.
 * Why: provider setup belongs at the edge of the frontend. That keeps normal
 * components from manually creating routers or query clients and makes the app's
 * runtime dependencies easy to see in one place.
 */
createRoot(document.getElementById('root')).render(
  <StrictMode>
    {/* What: QueryClientProvider gives all child components access to TanStack
        Query's request cache.
        How: it receives the single queryClient instance from src/app/queryClient.
        Why: one shared client means article lists/details can reuse cached data
        instead of every component making isolated requests. */}
    <QueryClientProvider client={queryClient}>
      {/* What: BrowserRouter enables URL-based navigation.
          How: it listens to browser history and lets App define route mappings.
          Why: the project needs real routes so article details can be opened,
          refreshed, and shared as URLs. */}
      <BrowserRouter>
        <App />
      </BrowserRouter>
      {/* What: ReactQueryDevtools is a development helper for inspecting query
          cache state.
          How: it is mounted closed by default so it does not cover the UI.
          Why: this helps diagnose loading, cached, stale, and refetched query
          states during development. */}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  </StrictMode>
)
