import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { newsApi } from '../../api/articles/newsApi.js'
import { createQueryWrapper } from '../../test/renderHookWithClient.jsx'
import { useAuth } from './useAuth.js'

vi.mock('../../api/articles/newsApi.js', () => ({
  newsApi: {
    me: vi.fn(),
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
    deleteAccount: vi.fn()
  }
}))

const readerResponse = {
  user: {
    id: 'reader-1',
    email: 'reader@example.com',
    displayName: 'Reader User',
    role: 'Reader'
  }
}

/*
 * What: Tests the useAuth hook.
 * How: Mocks the frontend API module and renders the hook inside a fresh query
 * client provider.
 * Why: App navigation and protected pages depend on useAuth being the single
 * source of truth for current user/session state.
 */
describe('useAuth', () => {
  afterEach(() => {
    vi.clearAllMocks()
  })

  it('loads the current user from the backend', async () => {
    newsApi.me.mockResolvedValue(readerResponse)

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    expect(newsApi.me).toHaveBeenCalledTimes(1)
  })

  it('logs in and updates the current user cache', async () => {
    newsApi.me.mockResolvedValue(null)
    newsApi.login.mockResolvedValue(readerResponse)

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => {
      result.current.login({ email: 'reader@example.com', password: 'Password123!' })
    })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    expect(newsApi.login).toHaveBeenCalledWith({
      email: 'reader@example.com',
      password: 'Password123!'
    })
  })

  it('registers and updates the current user cache', async () => {
    newsApi.me.mockResolvedValue(null)
    newsApi.register.mockResolvedValue(readerResponse)

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.loading).toBe(false))
    act(() => {
      result.current.register({
        email: 'reader@example.com',
        displayName: 'Reader User',
        password: 'Password123!',
        role: 'Reader'
      })
    })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    expect(newsApi.register).toHaveBeenCalledWith({
      email: 'reader@example.com',
      displayName: 'Reader User',
      password: 'Password123!',
      role: 'Reader'
    })
  })

  it('logs out and clears the current user cache', async () => {
    newsApi.me
      .mockResolvedValueOnce(readerResponse)
      .mockResolvedValue(null)
    newsApi.logout.mockResolvedValue({ signedOut: true })

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    act(() => {
      result.current.logout()
    })

    await waitFor(() => expect(result.current.user).toBeNull())
    expect(newsApi.logout).toHaveBeenCalledTimes(1)
  })

  it('updates profile and replaces the current user cache', async () => {
    const updatedResponse = {
      user: {
        ...readerResponse.user,
        displayName: 'Updated Reader'
      }
    }
    newsApi.me.mockResolvedValue(readerResponse)
    newsApi.updateProfile.mockResolvedValue(updatedResponse)

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    act(() => {
      result.current.updateProfile({ displayName: 'Updated Reader' })
    })

    await waitFor(() => expect(result.current.user).toEqual(updatedResponse.user))
    expect(newsApi.updateProfile).toHaveBeenCalledWith({ displayName: 'Updated Reader' })
  })

  it('changes password without clearing the current user', async () => {
    newsApi.me.mockResolvedValue(readerResponse)
    newsApi.changePassword.mockResolvedValue(readerResponse)

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    act(() => {
      result.current.changePassword({
        currentPassword: 'Password123!',
        newPassword: 'NewPassword123!'
      })
    })

    await waitFor(() => expect(newsApi.changePassword).toHaveBeenCalledWith({
      currentPassword: 'Password123!',
      newPassword: 'NewPassword123!'
    }))
    expect(result.current.user).toEqual(readerResponse.user)
  })

  it('deletes account and clears the current user cache', async () => {
    newsApi.me
      .mockResolvedValueOnce(readerResponse)
      .mockResolvedValue(null)
    newsApi.deleteAccount.mockResolvedValue({ deleted: true })

    const { result } = renderHook(() => useAuth(), { wrapper: createQueryWrapper() })

    await waitFor(() => expect(result.current.user).toEqual(readerResponse.user))
    act(() => {
      result.current.deleteAccount({ currentPassword: 'Password123!' })
    })

    await waitFor(() => expect(result.current.user).toBeNull())
    expect(newsApi.deleteAccount).toHaveBeenCalledWith({ currentPassword: 'Password123!' })
  })
})
