import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { newsApi } from '../../api/articles/newsApi.js'
import { createQueryWrapper } from '../../test/renderHookWithClient.jsx'
import { useEditorialArticles } from './useEditorialArticles.js'

vi.mock('../../api/articles/newsApi.js', () => ({
  newsApi: {
    getEditorialArticles: vi.fn(),
    createEditorialArticle: vi.fn(),
    updateEditorialArticle: vi.fn(),
    publishEditorialArticle: vi.fn(),
    archiveEditorialArticle: vi.fn()
  }
}))

/*
 * What: Tests the admin editorial hook.
 * How: Mocks editorial API endpoints and verifies each command delegates to the
 * expected backend function.
 * Why: The editorial page is admin-only, but the workflow itself is server
 * state and should remain correct when the page markup changes.
 */
describe('useEditorialArticles', () => {
  afterEach(() => {
    vi.clearAllMocks()
  })

  it('loads editorial articles when enabled', async () => {
    newsApi.getEditorialArticles.mockResolvedValue({
      items: [{ id: 'editorial-1', title: 'Editorial article' }]
    })

    const { result } = renderHook(() => useEditorialArticles(true), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.articles).toHaveLength(1))
    expect(result.current.articles[0].title).toBe('Editorial article')
  })

  it('does not load or mutate when disabled', () => {
    const { result } = renderHook(() => useEditorialArticles(false), { wrapper: createQueryWrapper() })

    act(() => {
      result.current.createArticle({ title: 'Draft' })
      result.current.updateArticle('editorial-1', { title: 'Updated' })
      result.current.publishArticle('editorial-1')
      result.current.archiveArticle('editorial-1')
    })

    expect(newsApi.getEditorialArticles).not.toHaveBeenCalled()
    expect(newsApi.createEditorialArticle).not.toHaveBeenCalled()
    expect(newsApi.updateEditorialArticle).not.toHaveBeenCalled()
    expect(newsApi.publishEditorialArticle).not.toHaveBeenCalled()
    expect(newsApi.archiveEditorialArticle).not.toHaveBeenCalled()
  })

  it('sends editorial commands when enabled', async () => {
    const draft = { title: 'Draft article' }
    const update = { title: 'Updated article' }
    newsApi.getEditorialArticles.mockResolvedValue({ items: [] })
    newsApi.createEditorialArticle.mockResolvedValue({ id: 'editorial-1' })
    newsApi.updateEditorialArticle.mockResolvedValue({ id: 'editorial-1' })
    newsApi.publishEditorialArticle.mockResolvedValue({ id: 'editorial-1', status: 'Published' })
    newsApi.archiveEditorialArticle.mockResolvedValue({ id: 'editorial-1', status: 'Archived' })

    const { result } = renderHook(() => useEditorialArticles(true), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => {
      result.current.createArticle(draft)
      result.current.updateArticle('editorial-1', update)
      result.current.publishArticle('editorial-1')
      result.current.archiveArticle('editorial-1')
    })

    await waitFor(() => expect(newsApi.createEditorialArticle).toHaveBeenCalledWith(draft))
    await waitFor(() => expect(newsApi.updateEditorialArticle).toHaveBeenCalledWith('editorial-1', update))
    await waitFor(() => expect(newsApi.publishEditorialArticle).toHaveBeenCalledWith('editorial-1'))
    await waitFor(() => expect(newsApi.archiveEditorialArticle).toHaveBeenCalledWith('editorial-1'))
  })
})
