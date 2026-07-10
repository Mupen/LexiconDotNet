import { SearchPanel } from '../../components/forms/SearchPanel.jsx'
import { useState } from 'react'

/*
 * What: ProfilePage shows account identity and persisted reader settings.
 * How: it renders a profile summary plus SearchPanel in preferences-only mode.
 * Why: profile pages should own identity/settings, while the general news page should stay focused on browsing articles.
 */
export function ProfilePage({ auth, readerPreferences, savedArticles }) {
  const preferences = readerPreferences.preferences
  const [displayName, setDisplayName] = useState(auth.user?.displayName ?? '')
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: ''
  })
  const [deletePassword, setDeletePassword] = useState('')
  const profileRequest = {
    q: 'react',
    country: 'us',
    language: 'en',
    sortBy: 'publishedAt',
    pageSize: 20
  }

  /*
   * What: saveProfile sends a display-name update to the backend.
   * How: it prevents normal form submit navigation and delegates to useAuth.
   * Why: the backend owns validation and persistence, while this page owns the form state.
   */
  function saveProfile(event) {
    event.preventDefault()
    auth.updateProfile({ displayName })
  }

  /*
   * What: savePassword sends current/new password values to the backend.
   * How: it uses the auth hook mutation and clears local password inputs after the request is started.
   * Why: password values should not stay visible in form state longer than needed.
   */
  function savePassword(event) {
    event.preventDefault()
    auth.changePassword(passwordForm)
    setPasswordForm({ currentPassword: '', newPassword: '' })
  }

  /*
   * What: removeAccount requests account deletion.
   * How: it sends password confirmation through the auth hook.
   * Why: account deletion should be an explicit form action instead of a simple accidental button click.
   */
  function removeAccount(event) {
    event.preventDefault()
    auth.deleteAccount({ currentPassword: deletePassword })
    setDeletePassword('')
  }

  return (
    <section className="profile-page">
      <div className="section-header">
        <div>
          <p className="eyebrow">Reader profile</p>
          <h2>Profile</h2>
          <p>Profile and reader settings are shown together here before real account authentication is added.</p>
        </div>
      </div>

      <div className="profile-layout">
        <article className="panel account-panel">
          <h3>Account</h3>
          <dl className="metadata-list">
            <div>
              <dt>Name</dt>
              <dd>{auth.user?.displayName ?? 'Unknown'}</dd>
            </div>
            <div>
              <dt>Email</dt>
              <dd>{auth.user?.email ?? 'Unknown'}</dd>
            </div>
            <div>
              <dt>Role</dt>
              <dd>{auth.user?.role ?? 'Unknown'}</dd>
            </div>
            <div>
              <dt>Saved articles</dt>
              <dd>{savedArticles.items.length}</dd>
            </div>
          </dl>

          <form className="account-form" onSubmit={saveProfile}>
            <label>
              Display name
              <input
                value={displayName}
                onChange={(event) => setDisplayName(event.target.value)}
                minLength={2}
                maxLength={80}
                required
              />
            </label>
            <button type="submit" disabled={auth.updatingProfile}>Save profile</button>
          </form>

          <form className="account-form" onSubmit={savePassword}>
            <h4>Change password</h4>
            <label>
              Current password
              <input
                type="password"
                value={passwordForm.currentPassword}
                onChange={(event) => setPasswordForm({
                  ...passwordForm,
                  currentPassword: event.target.value
                })}
                required
              />
            </label>
            <label>
              New password
              <input
                type="password"
                value={passwordForm.newPassword}
                onChange={(event) => setPasswordForm({
                  ...passwordForm,
                  newPassword: event.target.value
                })}
                minLength={8}
                required
              />
            </label>
            <button type="submit" disabled={auth.changingPassword}>Change password</button>
          </form>

          <form className="account-form danger-zone" onSubmit={removeAccount}>
            <h4>Delete account</h4>
            <p>Deleting the account signs you out and removes the login record.</p>
            <label>
              Current password
              <input
                type="password"
                value={deletePassword}
                onChange={(event) => setDeletePassword(event.target.value)}
                required
              />
            </label>
            <button type="submit" className="secondary" disabled={auth.deletingAccount}>Delete account</button>
          </form>
        </article>

        <SearchPanel
          request={profileRequest}
          preferences={preferences}
          loading={readerPreferences.saving}
          onPreferencesChange={readerPreferences.updatePreferences}
          showArticleControls={false}
        />
      </div>
    </section>
  )
}
