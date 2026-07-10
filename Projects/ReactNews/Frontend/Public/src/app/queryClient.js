import { QueryClient } from '@tanstack/react-query'

/*
 * What: queryClient is the shared TanStack Query cache/configuration object.
 * How: components use it indirectly through QueryClientProvider and useQuery.
 * Why: server data should be cached in one central client instead of copied into
 * local component state. That makes loading/error/cache behavior predictable.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      /*
       * What: staleTime controls how long query data is treated as fresh.
       * How: TanStack Query avoids refetching fresh data for this many
       * milliseconds unless a caller forces a refetch.
       * Why: NewsAPI data changes over time, but refetching on every render or
       * route movement would waste API quota and make the UI feel unstable.
       */
      staleTime: 60_000,

      /*
       * What: gcTime controls how long unused query results stay in memory.
       * How: once no component is using a query, TanStack Query keeps it for this
       * many milliseconds before garbage collecting it.
       * Why: users often move between list/detail views. Keeping recent data for
       * ten minutes improves navigation without treating the browser as authority.
       */
      gcTime: 10 * 60_000,

      /*
       * What: retry controls automatic retries after request failure.
       * How: TanStack Query will retry failed requests once.
       * Why: a single retry handles small network hiccups without hiding real
       * configuration errors such as a missing API key behind many repeated calls.
       */
      retry: 1,

      /*
       * What: refetchOnWindowFocus controls refetching when the browser tab gets
       * focus again.
       * How: false tells TanStack Query not to automatically refetch just because
       * the user alt-tabs back to the app.
       * Why: NewsAPI has quota limits, and this project already has explicit
       * loading/search actions. Avoiding focus refetches makes behavior easier to
       * understand during development.
       */
      refetchOnWindowFocus: false
    }
  }
})
