import { Link } from 'react-router-dom'
import { useState } from 'react'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'

/*
 * What: LoginPage shows the account entry point.
 * How: it renders login/register fields and calls the auth hook actions passed
 * in from App.
 * Why: authentication is owned by the backend, but the frontend still needs one
 * predictable page for account access. Public registration creates Reader
 * accounts; Admin accounts come from backend seed configuration.
 */
export function LoginPage({ auth }) {
  const [form, setForm] = useState({
    email: 'reader@example.com',
    displayName: 'Demo Reader',
    password: 'Password123!'
  })

  const status = auth.error
    ? { type: 'error', text: auth.error.message }
    : auth.user
      ? { type: 'success', text: `Signed in as ${auth.user.displayName} (${auth.user.role}).` }
      : { type: 'idle', text: 'Sign in or create a demo account.' }

  /*
   * What: Updates one field in the login/register form state.
   * How: copies the current form object and replaces only the named field.
   * Why: login and registration share the same visible fields, so one small
   * helper keeps the input handlers consistent.
   */
  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
  }

  return (
    <section className="account-page">
      <StatusMessage status={status} />
      <div className="panel account-panel">
        <div>
          <p className="eyebrow">Account access</p>
          <h2>Sign in to ReactNews</h2>
          <p>Reader accounts will keep personal feeds, saved articles, display settings, and profile data together.</p>
        </div>

        <form>
          <label>
            Email
            <input type="email" value={form.email} onChange={(event) => updateField('email', event.target.value)} />
          </label>
          <label>
            Display name
            <input value={form.displayName} onChange={(event) => updateField('displayName', event.target.value)} />
          </label>
          <label>
            Password
            <input type="password" value={form.password} onChange={(event) => updateField('password', event.target.value)} />
          </label>
          <div className="button-row">
            <button type="button" disabled={auth.signingIn} onClick={() => auth.login({ email: form.email, password: form.password })}>Sign in</button>
            <button type="button" className="secondary" disabled={auth.signingIn} onClick={() => auth.register(form)}>Create account</button>
          </div>
        </form>
      </div>

      <div className="account-grid">
        <article className="panel">
          <h3>Reader route</h3>
          <p>Use the personal page to show saved articles, preferred categories, and reading preferences.</p>
          <Link className="primary-link" to="/personal">Open personal</Link>
        </article>
        <article className="panel">
          <h3>Admin route</h3>
          <p>Use the editorial workspace to create, preview, and publish original ReactNews articles.</p>
          <Link className="primary-link" to="/editorial">Open editorial</Link>
        </article>
      </div>
    </section>
  )
}
