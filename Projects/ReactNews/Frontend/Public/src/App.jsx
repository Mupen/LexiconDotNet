import { Navigate, NavLink, Route, Routes } from 'react-router-dom'
import { useAuth } from './hooks/articles/useAuth.js'
import { useEditorialArticles } from './hooks/articles/useEditorialArticles.js'
import { useNewsSearch } from './hooks/articles/useNewsSearch.js'
import { useReaderPreferences } from './hooks/articles/useReaderPreferences.js'
import { useSavedArticles } from './hooks/articles/useSavedArticles.js'
import { ArticleDetailPage } from './pages/articles/ArticleDetailPage.jsx'
import { EditorialDetailPage } from './pages/articles/EditorialDetailPage.jsx'
import { EditorialFeedPage } from './pages/articles/EditorialFeedPage.jsx'
import { HomePage } from './pages/articles/HomePage.jsx'
import { SavedArticlesPage } from './pages/articles/SavedArticlesPage.jsx'
import { EditorialPage } from './pages/editorial/EditorialPage.jsx'
import { LoginPage } from './pages/account/LoginPage.jsx'
import { PersonalPage } from './pages/account/PersonalPage.jsx'
import { ProfilePage } from './pages/account/ProfilePage.jsx'

/*
 * What: App is the top-level React component for the reader/editorial experience.
 * How: it creates the shared news-search state once with useNewsSearch, renders
 * the persistent page header, and lets React Router choose between public news,
 * personal reader, profile, login, saved, detail, and editorial pages.
 * Why: keeping routing and top-level state here prevents each page from creating
 * its own disconnected copy of the article-list state. The app stays small while
 * keeping the route structure visible from one entry point.
 */
export default function App() {
  /*
   * What: news contains article data, loading state, filters, paging actions,
   * and feed actions for the home page.
   * How: useNewsSearch reads URL parameters, calls the backend through TanStack
   * Query, and returns a plain object of values/actions.
   * Why: this keeps data-fetching and URL-state logic out of the visual page
   * components, so HomePage can focus on layout and rendering.
  */
  const news = useNewsSearch()
  const auth = useAuth()
  const isReader = auth.user?.role === 'Reader' || auth.user?.role === 'Admin'
  const isAdmin = auth.user?.role === 'Admin'
  const editorialArticles = useEditorialArticles(isAdmin)
  const savedArticles = useSavedArticles(isReader)
  const readerPreferences = useReaderPreferences(isReader)

  return (
    <div
      className={`app-shell theme-${readerPreferences.preferences.theme}`}
      style={{ fontSize: `${readerPreferences.preferences.fontScale}rem` }}
    >
      <header className="site-header">
        <div>
          <p className="eyebrow">Backend proxied NewsAPI project</p>
          <h1>ReactNews</h1>
        </div>
        <nav className="site-nav">
          <NavLink to="/news">News</NavLink>
          <NavLink to="/editorial-feed">Editorial</NavLink>
          {isReader && <NavLink to="/personal">Personal</NavLink>}
          {isReader && <NavLink to="/saved">Saved</NavLink>}
          {isReader && <NavLink to="/profile">Profile</NavLink>}
          {isAdmin && <NavLink to="/editorial">Admin</NavLink>}
          {auth.user ? (
            <button type="button" className="secondary" onClick={auth.logout} disabled={auth.signingOut}>Logout</button>
          ) : (
            <NavLink to="/login">Login</NavLink>
          )}
        </nav>
      </header>

      <Routes>
        {/* What: "/" redirects to the main news route.
            How: Navigate replaces the root route with /news.
            Why: /news is clearer once the app also has personal, profile, and
            editorial sections. */}
        <Route path="/" element={<Navigate to="/news" replace />} />

        {/* What: "/news" is the main reader-facing news feed.
            How: HomePage receives the shared news state object as props.
            Why: passing one object keeps the page API stable while the hook can
            grow internally with more feed behavior later. */}
        <Route path="/news" element={<HomePage news={news} savedArticles={savedArticles} readerPreferences={readerPreferences} />} />

        {/* What: "/editorial-feed" shows published ReactNews-owned articles.
            How: EditorialFeedPage calls the public editorial API through its own hook.
            Why: admin-created articles need a reader-facing route separate from
            external NewsAPI headlines. */}
        <Route path="/editorial-feed" element={<EditorialFeedPage />} />

        {/* What: "/personal" shows a preference-driven reader dashboard.
            How: PersonalPage receives saved articles, news actions, and reader
            preferences from the existing hooks.
            Why: personal pages are where saved reading, preferred categories,
            and future subscriptions should come together. */}
        <Route path="/personal" element={isReader ? <PersonalPage news={news} savedArticles={savedArticles} readerPreferences={readerPreferences} /> : <Navigate to="/login" replace />} />

        {/* What: "/saved" shows the reader's saved-for-later article list.
            How: SavedArticlesPage receives the savedArticles hook state/actions.
            Why: saved articles are now persistent backend data and deserve a
            first-class route instead of being hidden inside the home page. */}
        <Route path="/saved" element={isReader ? <SavedArticlesPage savedArticles={savedArticles} /> : <Navigate to="/login" replace />} />

        {/* What: "/profile" shows account/profile settings.
            How: ProfilePage receives persisted reader preference state.
            Why: account pages should collect identity, role, and settings in
            one place. */}
        <Route path="/profile" element={isReader ? <ProfilePage auth={auth} readerPreferences={readerPreferences} savedArticles={savedArticles} /> : <Navigate to="/login" replace />} />

        {/* What: "/login" shows the authentication entry point.
            How: LoginPage receives the auth hook and calls login/register
            actions for the backend cookie-auth flow.
            Why: account access should stay in one predictable route while the
            backend owns credentials, cookies, and roles. */}
        <Route path="/login" element={<LoginPage auth={auth} />} />

        {/* What: "/editorial" shows the admin editorial workspace.
            How: EditorialPage receives the editorial hook state/actions for
            backend-backed create, update, publish, and archive commands.
            Why: first-party editorial content is separate from external
            NewsAPI content and needs an admin-only route. */}
        <Route path="/editorial" element={isAdmin ? <EditorialPage editorialArticles={editorialArticles} /> : <Navigate to="/login" replace />} />

        {/* What: "/article/:articleId" is the dynamic article detail route.
            How: React Router extracts articleId inside ArticleDetailPage.
            Why: the assignment requires a dynamic route segment, and real news
            sites need shareable/detail URLs rather than only modal state. */}
        <Route path="/article/:articleId" element={<ArticleDetailPage savedArticles={savedArticles} />} />

        {/* What: "/editorial-feed/:articleId" shows one published first-party article.
            How: EditorialDetailPage loads the public detail endpoint by route id.
            Why: public editorial articles should have shareable detail URLs. */}
        <Route path="/editorial-feed/:articleId" element={<EditorialDetailPage />} />
      </Routes>
    </div>
  )
}
