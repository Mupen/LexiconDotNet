import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { LoginPage } from './LoginPage.jsx'

/*
 * What: Renders LoginPage with a controllable fake auth object.
 * How: builds default fake auth methods, merges overrides, and wraps the page in
 * MemoryRouter for Link support.
 * Why: each test can focus on page behavior without booting the full App router
 * or making real authentication requests.
 */
function renderLoginPage(authOverrides = {}) {
  const auth = {
    user: null,
    error: null,
    signingIn: false,
    login: vi.fn(),
    register: vi.fn(),
    ...authOverrides
  }

  render(
    <MemoryRouter>
      <LoginPage auth={auth} />
    </MemoryRouter>
  )

  return auth
}

/*
 * What: Tests the login/register page behavior.
 * How: Renders the page with fake auth actions and submits the visible buttons
 * through user-event.
 * Why: Account entry is a core workflow, so the frontend should prove it sends
 * the expected login/register payloads to the auth hook.
 */
describe('LoginPage', () => {
  it('calls login with the email and password fields', async () => {
    const user = userEvent.setup()
    const auth = renderLoginPage()

    await user.clear(screen.getByLabelText('Email'))
    await user.type(screen.getByLabelText('Email'), 'admin@example.com')
    await user.clear(screen.getByLabelText('Password'))
    await user.type(screen.getByLabelText('Password'), 'AdminPassword123!')
    await user.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(auth.login).toHaveBeenCalledWith({
      email: 'admin@example.com',
      password: 'AdminPassword123!'
    })
  })

  it('calls register with the reader account form', async () => {
    const user = userEvent.setup()
    const auth = renderLoginPage()

    await user.clear(screen.getByLabelText('Email'))
    await user.type(screen.getByLabelText('Email'), 'editor@example.com')
    await user.clear(screen.getByLabelText('Display name'))
    await user.type(screen.getByLabelText('Display name'), 'Editor User')
    await user.click(screen.getByRole('button', { name: 'Create account' }))

    expect(auth.register).toHaveBeenCalledWith({
      email: 'editor@example.com',
      displayName: 'Editor User',
      password: 'Password123!'
    })
  })

  it('shows the signed-in user status when auth has a user', () => {
    renderLoginPage({
      user: {
        displayName: 'Reader User',
        role: 'Reader'
      }
    })

    expect(screen.getByRole('status')).toHaveTextContent('Signed in as Reader User (Reader).')
  })
})
