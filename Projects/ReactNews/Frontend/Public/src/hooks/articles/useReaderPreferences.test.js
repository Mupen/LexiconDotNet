import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { newsApi } from '../../api/articles/newsApi.js'
import { createQueryWrapper } from '../../test/renderHookWithClient.jsx'
import { useReaderPreferences } from './useReaderPreferences.js'

vi.mock('../../api/articles/newsApi.js', () => ({
  newsApi: {
    getReaderPreferences: vi.fn(),
    updateReaderPreferences: vi.fn()
  }
}))

/*
 * What: Tests the reader preferences hook.
 * How: Mocks backend preference endpoints and localStorage through jsdom.
 * Why: Theme/font/category settings affect the whole app shell, so optimistic
 * local updates and backend persistence should both be protected.
 */
describe('useReaderPreferences', () => {
  afterEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('loads preferences from the backend when enabled', async () => {
    newsApi.getReaderPreferences.mockResolvedValue({
      theme: 'dark',
      fontScale: 1.15,
      compactCards: true,
      preferredCategories: ['technology']
    })

    const { result } = renderHook(() => useReaderPreferences(true), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.preferences.theme).toBe('dark'))
    expect(result.current.preferences.compactCards).toBe(true)
  })

  it('updates local state and persists changes when enabled', async () => {
    newsApi.getReaderPreferences.mockResolvedValue({
      theme: 'light',
      fontScale: 1,
      compactCards: false,
      preferredCategories: ['general']
    })
    newsApi.updateReaderPreferences.mockResolvedValue({
      theme: 'dark',
      fontScale: 1,
      compactCards: false,
      preferredCategories: ['general']
    })

    const { result } = renderHook(() => useReaderPreferences(true), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.preferences.theme).toBe('light'))
    act(() => {
      result.current.updatePreferences({ theme: 'dark' })
    })

    await waitFor(() => expect(result.current.preferences.theme).toBe('dark'))
    expect(newsApi.updateReaderPreferences).toHaveBeenCalledWith({
      theme: 'dark',
      fontScale: 1,
      compactCards: false,
      preferredCategories: ['technology', 'general']
    })
  })

  it('keeps local-only updates when disabled', async () => {
    const { result } = renderHook(() => useReaderPreferences(false), { wrapper: createQueryWrapper() })

    act(() => {
      result.current.updatePreferences({ theme: 'dark' })
    })

    await waitFor(() => expect(result.current.preferences.theme).toBe('dark'))
    expect(newsApi.getReaderPreferences).not.toHaveBeenCalled()
    expect(newsApi.updateReaderPreferences).not.toHaveBeenCalled()
  })
})
