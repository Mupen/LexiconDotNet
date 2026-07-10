import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { newsApi } from '../../api/articles/newsApi.js'
import { createQueryWrapper } from '../../test/renderHookWithClient.jsx'
import { useSavedArticles } from './useSavedArticles.js'

vi.mock('../../api/articles/newsApi.js', () => ({
  newsApi: {
    getSavedArticles: vi.fn(),
    saveArticle: vi.fn(),
    removeSavedArticle: vi.fn()
  }
}))

/*
 * What: Tests the saved articles hook.
 * How: Mocks saved-article API calls and observes hook state/actions through
 * React Testing Library's renderHook.
 * Why: Save/remove actions are used by cards, detail pages, and the saved page;
 * the cache behavior should be verified in one place.
 */
describe('useSavedArticles', () => {
  afterEach(() => {
    vi.clearAllMocks()
  })

  it('loads saved articles and exposes saved id lookup', async () => {
    newsApi.getSavedArticles.mockResolvedValue({
      items: [{ id: 'article-1', title: 'Saved article' }]
    })

    const { result } = renderHook(() => useSavedArticles(true), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.items).toHaveLength(1))
    expect(result.current.isSaved('article-1')).toBe(true)
    expect(result.current.isSaved('missing')).toBe(false)
  })

  it('does not load or mutate when disabled', async () => {
    const { result } = renderHook(() => useSavedArticles(false), { wrapper: createQueryWrapper() })

    act(() => {
      result.current.saveArticle('article-1')
      result.current.removeSavedArticle('article-1')
    })

    expect(newsApi.getSavedArticles).not.toHaveBeenCalled()
    expect(newsApi.saveArticle).not.toHaveBeenCalled()
    expect(newsApi.removeSavedArticle).not.toHaveBeenCalled()
  })

  it('sends save and remove commands when enabled', async () => {
    newsApi.getSavedArticles.mockResolvedValue({ items: [] })
    newsApi.saveArticle.mockResolvedValue({ id: 'article-1' })
    newsApi.removeSavedArticle.mockResolvedValue({ removed: true })

    const { result } = renderHook(() => useSavedArticles(true), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => {
      result.current.saveArticle('article-1')
      result.current.removeSavedArticle('article-1')
    })

    await waitFor(() => expect(newsApi.saveArticle).toHaveBeenCalledWith('article-1'))
    await waitFor(() => expect(newsApi.removeSavedArticle).toHaveBeenCalledWith('article-1'))
  })
})
