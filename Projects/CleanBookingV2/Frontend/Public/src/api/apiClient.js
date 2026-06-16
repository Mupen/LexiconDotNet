const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5217'

export async function apiRequest(path, options = {}) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers ?? {})
    },
    ...options
  })

  if (response.status === 204) {
    return null
  }

  const text = await response.text()
  const data = text ? JSON.parse(text) : null

  if (!response.ok) {
    throw new Error(data?.detail ?? data?.title ?? response.statusText)
  }

  return data
}
