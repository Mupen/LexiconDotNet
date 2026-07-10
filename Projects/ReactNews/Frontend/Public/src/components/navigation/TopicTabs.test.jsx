import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { TopicTabs } from './TopicTabs.jsx'

/*
 * What: Tests the public headline category tabs.
 * How: Renders the tabs, checks the active category class, and clicks another
 * category button.
 * Why: Category switching drives the main news feed query, so the UI command
 * should keep calling the parent action with the selected category.
 */
describe('TopicTabs', () => {
  it('marks the active category and sends selected category changes', async () => {
    const user = userEvent.setup()
    const onSelectCategory = vi.fn()

    render(<TopicTabs activeCategory="technology" loading={false} onSelectCategory={onSelectCategory} />)

    expect(screen.getByRole('button', { name: 'technology' })).toHaveClass('active')

    await user.click(screen.getByRole('button', { name: 'sports' }))

    expect(onSelectCategory).toHaveBeenCalledWith('sports')
  })

  it('disables category buttons while the feed is loading', () => {
    render(<TopicTabs activeCategory="general" loading onSelectCategory={vi.fn()} />)

    expect(screen.getByRole('button', { name: 'general' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'business' })).toBeDisabled()
  })
})
