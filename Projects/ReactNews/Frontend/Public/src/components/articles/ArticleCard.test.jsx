import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { ArticleCard } from './ArticleCard.jsx'

const article = {
  id: 'article-1',
  title: 'ReactNews test article',
  description: 'Article summary used by the component test.',
  sourceName: 'ReactNews Test Source',
  url: 'https://example.com/article'
}

/*
 * What: Tests the article card component used by the public news feed.
 * How: Renders it inside MemoryRouter because the Details link uses React
 * Router, then clicks the save/remove button.
 * Why: Saving articles is a core reader action and should not regress silently
 * when the card layout changes.
 */
describe('ArticleCard', () => {
  it('renders article information and calls save for unsaved articles', async () => {
    const user = userEvent.setup()
    const onSave = vi.fn()

    render(
      <MemoryRouter>
        <ArticleCard
          article={article}
          compact={false}
          saved={false}
          saving={false}
          onSave={onSave}
          onRemoveSaved={vi.fn()}
        />
      </MemoryRouter>
    )

    expect(screen.getByRole('heading', { name: 'ReactNews test article' })).toBeInTheDocument()
    expect(screen.getByText('Article summary used by the component test.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Details' })).toHaveAttribute('href', '/article/article-1')
    expect(screen.getByRole('link', { name: 'Original' })).toHaveAttribute('href', 'https://example.com/article')

    await user.click(screen.getByRole('button', { name: 'Save' }))

    expect(onSave).toHaveBeenCalledWith('article-1')
  })

  it('calls remove when an article is already saved', async () => {
    const user = userEvent.setup()
    const onRemoveSaved = vi.fn()

    render(
      <MemoryRouter>
        <ArticleCard
          article={article}
          compact
          saved
          saving={false}
          onSave={vi.fn()}
          onRemoveSaved={onRemoveSaved}
        />
      </MemoryRouter>
    )

    await user.click(screen.getByRole('button', { name: 'Saved' }))

    expect(onRemoveSaved).toHaveBeenCalledWith('article-1')
  })
})
