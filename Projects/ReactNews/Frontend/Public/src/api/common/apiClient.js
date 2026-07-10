const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5217'

/*
 * What: apiRequest is the shared frontend HTTP helper for calling the backend.
 * How: it prefixes the backend base URL, sends JSON-friendly headers, parses the
 * response body, and throws a JavaScript Error when the backend returns a
 * non-success status.
 * Why: keeping this behavior in one function prevents every API module from
 * duplicating fetch/error parsing. The frontend calls the ReactNews backend,
 * not NewsAPI directly, because the backend owns the NewsAPI key.
 */
export async function apiRequest(path, options = {}) {
  /*
   * What: fetch sends the actual browser HTTP request.
   * How: options are spread after the default headers so callers can pass
   * AbortSignal, method, body, or extra headers when needed.
   * Why: a thin wrapper gives flexibility without hiding that this is normal
   * browser fetch under the hood.
   */
  const response = await fetch(`${apiBaseUrl}${path}`, {
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers ?? {})
    },
    ...options
  })

  /*
   * What: the response body is read as text before JSON parsing.
   * How: empty responses become null; non-empty responses are parsed as JSON.
   * Why: some successful HTTP responses can legally have no body. Parsing an
   * empty string with JSON.parse would throw and make a valid response look like
   * a frontend bug.
   */
  const text = await response.text()
  const data = text ? JSON.parse(text) : null

  /*
   * What: failed HTTP responses become thrown Error objects.
   * How: the helper prefers backend error fields in a stable order, then falls
   * back to the HTTP status text.
   * Why: TanStack Query expects rejected promises for request failures, and this
   * gives React one consistent error shape to display.
   */
  if (!response.ok) {
    throw new Error(data?.error ?? data?.detail ?? data?.title ?? response.statusText)
  }

  return data
}
