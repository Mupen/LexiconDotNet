import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

/*
 * What: Creates a fresh TanStack Query client for one frontend test.
 * How: Disables retries and console-noisy error throwing so tests can assert
 * loading/error behavior deterministically.
 * Why: Hooks such as useAuth and useSavedArticles are server-state hooks; each
 * test needs isolated cache state so one test cannot influence another.
 */
export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false
      },
      mutations: {
        retry: false
      }
    }
  })
}

/*
 * What: Wraps hook tests in QueryClientProvider.
 * How: Returns a React component that provides the supplied or newly-created
 * query client.
 * Why: TanStack Query hooks throw without a provider, and sharing this wrapper
 * keeps every hook test short and consistent.
 */
export function createQueryWrapper(queryClient = createTestQueryClient()) {
  return function QueryWrapper({ children }) {
    return (
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    )
  }
}
