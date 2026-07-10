import { afterEach, describe, expect, it, vi } from 'vitest'
import { apiRequest } from './apiClient.js'

/*
 * What: Tests the shared backend HTTP helper.
 * How: Replaces global fetch with a Vitest mock and checks the request options
 * and returned/thrown values.
 * Why: Every frontend API call depends on this helper, so cookie handling and
 * backend error mapping should be protected by direct tests.
 */
describe('apiRequest', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('sends credentials and parses successful JSON responses', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: true,
      text: async () => JSON.stringify({ health: 'ok' })
    })

    const result = await apiRequest('/api/health')

    expect(fetchMock).toHaveBeenCalledWith('http://localhost:5217/api/health', {
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json'
      }
    })
    expect(result).toEqual({ health: 'ok' })
  })

  it('throws backend error messages for failed responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: false,
      statusText: 'Bad Request',
      text: async () => JSON.stringify({ error: 'Email is already registered.' })
    })

    await expect(apiRequest('/api/auth/register', { method: 'POST' }))
      .rejects
      .toThrow('Email is already registered.')
  })

  it('returns null for empty successful responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: true,
      text: async () => ''
    })

    const result = await apiRequest('/api/empty')

    expect(result).toBeNull()
  })
})
