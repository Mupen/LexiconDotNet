import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App.jsx'

const authState = vi.hoisted(() => ({
  user: null
}))

vi.mock('./hooks/articles/useAuth.js', () => ({
  useAuth: () => ({
    user: authState.user,
    loading: false,
    error: null,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
    deleteAccount: vi.fn(),
    signingIn: false,
    signingOut: false,
    updatingProfile: false,
    changingPassword: false,
    deletingAccount: false
  })
}))

vi.mock('./hooks/articles/useNewsSearch.js', () => ({
  useNewsSearch: () => ({
    request: {
      category: 'general',
      view: 'cards'
    },
    status: {
      type: 'success',
      text: 'Loaded test news.'
    },
    featuredArticle: null,
    articles: [],
    sources: [],
    loading: false,
    mode: 'headlines',
    totalResults: 0,
    totalPages: 1,
    page: 1,
    cachedUntil: null,
    loadTopHeadlines: vi.fn(),
    setView: vi.fn(),
    search: vi.fn(),
    setPageSize: vi.fn(),
    goToPage: vi.fn()
  })
}))

vi.mock('./hooks/articles/useSavedArticles.js', () => ({
  useSavedArticles: () => ({
    items: [],
    savedIds: new Set(),
    loading: false,
    saving: false,
    removing: false,
    error: null,
    saveArticle: vi.fn(),
    removeSavedArticle: vi.fn(),
    isSaved: () => false
  })
}))

vi.mock('./hooks/articles/useReaderPreferences.js', () => ({
  useReaderPreferences: () => ({
    preferences: {
      theme: 'light',
      fontScale: 1,
      compactCards: false,
      preferredCategories: ['general']
    },
    loading: false,
    saving: false,
    error: null,
    updatePreferences: vi.fn()
  })
}))

vi.mock('./hooks/articles/useEditorialArticles.js', () => ({
  useEditorialArticles: () => ({
    articles: [],
    loading: false,
    saving: false,
    error: null,
    createArticle: vi.fn(),
    updateArticle: vi.fn(),
    publishArticle: vi.fn(),
    archiveArticle: vi.fn()
  })
}))

vi.mock('./hooks/articles/usePublishedEditorialArticles.js', () => ({
  usePublishedEditorialArticles: () => ({
    articles: [{
      id: 'published-editorial',
      title: 'Published editorial article',
      summary: 'A published first-party article.',
      body: 'Body',
      author: 'Admin Editor',
      category: 'technology',
      imageUrl: null,
      publishedAtUtc: '2026-07-10T10:00:00Z'
    }],
    loading: false,
    error: null
  })
}))

vi.mock('./hooks/articles/usePublishedEditorialArticle.js', () => ({
  usePublishedEditorialArticle: () => ({
    article: {
      id: 'published-editorial',
      title: 'Published editorial article',
      summary: 'A published first-party article.',
      body: 'Body paragraph',
      author: 'Admin Editor',
      category: 'technology',
      imageUrl: null,
      publishedAtUtc: '2026-07-10T10:00:00Z'
    },
    loading: false,
    error: null
  })
}))

/*
 * What: Renders App at a specific route for route-protection tests.
 * How: uses MemoryRouter with the requested route as the initial history entry.
 * Why: tests can verify page access and redirects without starting a browser or
 * the Vite development server.
 */
function renderAppAt(route) {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <App />
    </MemoryRouter>
  )
}

/*
 * What: Tests top-level route protection and role-aware navigation.
 * How: Mocks the page hooks and changes the current auth user before rendering
 * App inside MemoryRouter.
 * Why: App owns which pages are visible to anonymous, Reader, and Admin users;
 * those decisions should be protected separately from backend authorization.
 */
describe('App route protection', () => {
  beforeEach(() => {
    authState.user = null
  })

  it('redirects anonymous users from personal pages to login', () => {
    renderAppAt('/personal')

    expect(screen.getByRole('heading', { name: 'Sign in to ReactNews' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Login' })).toBeInTheDocument()
  })

  it('shows reader navigation without admin access', () => {
    authState.user = {
      displayName: 'Reader User',
      email: 'reader@example.com',
      role: 'Reader'
    }

    renderAppAt('/personal')

    expect(screen.getByRole('link', { name: 'Personal' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Saved' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Profile' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Editorial' })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Admin' })).not.toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Personal' })).toBeInTheDocument()
  })

  it('shows admin editorial access', () => {
    authState.user = {
      displayName: 'Admin User',
      email: 'admin@example.com',
      role: 'Admin'
    }

    renderAppAt('/editorial')

    expect(screen.getByRole('link', { name: 'Admin' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Editorial' })).toBeInTheDocument()
  })

  it('shows the public editorial feed to anonymous users', () => {
    renderAppAt('/editorial-feed')

    expect(screen.getByRole('link', { name: 'Editorial' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Editorial Feed' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Published editorial article' })).toBeInTheDocument()
  })

  it('shows public editorial detail pages', () => {
    renderAppAt('/editorial-feed/published-editorial')

    expect(screen.getByRole('heading', { name: 'Published editorial article' })).toBeInTheDocument()
    expect(screen.getByText('Body paragraph')).toBeInTheDocument()
  })
})
