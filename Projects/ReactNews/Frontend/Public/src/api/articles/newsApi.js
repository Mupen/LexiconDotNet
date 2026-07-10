import { apiRequest } from '../common/apiClient.js'

/*
 * What: toQueryString converts a plain object into a URL query string.
 * How: it skips undefined, null, and empty-string values, then lets
 * URLSearchParams handle encoding.
 * Why: the backend has defaults for missing optional values. Sending empty
 * query values would make requests noisier and could accidentally override
 * backend defaults with meaningless blank strings.
 */
function toQueryString(parameters) {
  const search = new URLSearchParams()

  Object.entries(parameters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      search.set(key, value)
    }
  })

  return search.toString()
}

/*
 * What: newsApi groups all article/source backend calls used by the frontend.
 * How: each method builds a query string, then delegates the HTTP work and error
 * handling to apiRequest.
 * Why: React components should not know route strings or query-string details.
 * Keeping endpoint calls here makes backend contract changes easier to find.
 */
export const newsApi = {
  /*
   * What: getArticles calls the normalized article-list endpoint.
   * How: it sends mode, search, filter, sort, and paging values to /api/articles.
   * Why: this is the preferred endpoint because the backend decides whether the
   * request maps to NewsAPI top-headlines or everything.
   */
  getArticles({ mode, q, country, category, source, language, sortBy, page, pageSize, signal }) {
    const query = toQueryString({ mode, q, country, category, source, language, sortBy, page, pageSize })
    return apiRequest(`/api/articles?${query}`, { signal })
  },

  /*
   * What: getSources loads source metadata for source filters.
   * How: it forwards optional category/language/country filters to /api/sources.
   * Why: the frontend should not call NewsAPI directly for sources because that
   * would expose the API key and duplicate backend mapping rules.
   */
  getSources({ category, language, country, signal } = {}) {
    const query = toQueryString({ category, language, country })
    return apiRequest(`/api/sources?${query}`, { signal })
  },

  /*
   * What: getArticle loads one article snapshot by generated article id.
   * How: it calls the backend detail route, which reads the persisted snapshot
   * store rather than asking NewsAPI for an article id.
   * Why: NewsAPI does not provide a stable get-by-id endpoint for this app, so
   * ReactNews owns the detail snapshot lookup.
   */
  getArticle(articleId, { signal } = {}) {
    return apiRequest(`/api/articles/${articleId}`, { signal })
  },

  /*
   * What: getSavedArticles loads articles the reader saved for later.
   * How: it calls the backend saved-article list endpoint.
   * Why: saved articles are persistent reader state and should come from the
   * backend instead of local-only browser storage.
   */
  getSavedArticles({ signal } = {}) {
    return apiRequest('/api/saved-articles', { signal })
  },

  /*
   * What: saveArticle saves one article snapshot by id.
   * How: it posts to /api/saved-articles/{articleId}; the backend looks up the
   * article snapshot and stores it in the saved list.
   * Why: the frontend already knows article ids from list/detail routes, so it
   * should not send the full article body back to the backend.
   */
  saveArticle(articleId, { signal } = {}) {
    return apiRequest(`/api/saved-articles/${articleId}`, {
      method: 'POST',
      signal
    })
  },

  /*
   * What: removeSavedArticle removes one article from the saved list.
   * How: it sends DELETE to /api/saved-articles/{articleId}.
   * Why: the backend owns the persisted saved-list state, so remove should be a
   * backend command rather than deleting only local UI data.
   */
  removeSavedArticle(articleId, { signal } = {}) {
    return apiRequest(`/api/saved-articles/${articleId}`, {
      method: 'DELETE',
      signal
    })
  },

  /*
   * What: getReaderPreferences loads persisted reader display/feed settings.
   * How: it calls the backend local-reader preference endpoint.
   * Why: preferences should survive backend/frontend restarts and later map to a real account.
   */
  getReaderPreferences({ signal } = {}) {
    return apiRequest('/api/reader-preferences', { signal })
  },

  /*
   * What: updateReaderPreferences persists a full preference object.
   * How: it sends JSON with PUT so the backend can validate and replace the current settings.
   * Why: the backend is the source of truth for reader preferences once persistence exists.
   */
  updateReaderPreferences(preferences, { signal } = {}) {
    return apiRequest('/api/reader-preferences', {
      method: 'PUT',
      body: JSON.stringify(preferences),
      signal
    })
  },

  getEditorialArticles({ signal } = {}) {
    return apiRequest('/api/editorial/articles', { signal })
  },

  /*
   * What: getPublishedEditorialArticles loads the public ReactNews-owned article feed.
   * How: it calls the anonymous backend route that only returns Published editorial articles.
   * Why: first-party content should be readable by guests without exposing admin drafts or archived work.
   */
  getPublishedEditorialArticles({ signal } = {}) {
    return apiRequest('/api/public/editorial/articles', { signal })
  },

  /*
   * What: getPublishedEditorialArticle loads one published editorial article.
   * How: it sends the id to the public detail route.
   * Why: reader-facing editorial detail pages need shareable URLs and must not call admin-only endpoints.
   */
  getPublishedEditorialArticle(articleId, { signal } = {}) {
    return apiRequest(`/api/public/editorial/articles/${articleId}`, { signal })
  },

  createEditorialArticle(article, { signal } = {}) {
    return apiRequest('/api/editorial/articles', {
      method: 'POST',
      body: JSON.stringify(article),
      signal
    })
  },

  updateEditorialArticle(articleId, article, { signal } = {}) {
    return apiRequest(`/api/editorial/articles/${articleId}`, {
      method: 'PUT',
      body: JSON.stringify(article),
      signal
    })
  },

  publishEditorialArticle(articleId, { signal } = {}) {
    return apiRequest(`/api/editorial/articles/${articleId}/publish`, {
      method: 'POST',
      signal
    })
  },

  archiveEditorialArticle(articleId, { signal } = {}) {
    return apiRequest(`/api/editorial/articles/${articleId}/archive`, {
      method: 'POST',
      signal
    })
  },

  register(request, { signal } = {}) {
    return apiRequest('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(request),
      signal
    })
  },

  login(request, { signal } = {}) {
    return apiRequest('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(request),
      signal
    })
  },

  logout({ signal } = {}) {
    return apiRequest('/api/auth/logout', {
      method: 'POST',
      signal
    })
  },

  me({ signal } = {}) {
    return apiRequest('/api/auth/me', { signal })
  },

  /*
   * What: updateProfile changes safe profile fields for the current user.
   * How: it sends the display-name request to the authenticated profile endpoint.
   * Why: the frontend should not mutate auth cache directly; the backend must validate and persist account changes.
   */
  updateProfile(request, { signal } = {}) {
    return apiRequest('/api/auth/profile', {
      method: 'PUT',
      body: JSON.stringify(request),
      signal
    })
  },

  /*
   * What: changePassword updates the current user's password.
   * How: it sends currentPassword and newPassword to the backend.
   * Why: credential changes must be validated server-side against the stored password hash.
   */
  changePassword(request, { signal } = {}) {
    return apiRequest('/api/auth/password', {
      method: 'PUT',
      body: JSON.stringify(request),
      signal
    })
  },

  /*
   * What: deleteAccount removes the current signed-in account.
   * How: it sends password confirmation with DELETE /api/auth/account.
   * Why: deleting an account is destructive and should require backend confirmation before clearing frontend state.
   */
  deleteAccount(request, { signal } = {}) {
    return apiRequest('/api/auth/account', {
      method: 'DELETE',
      body: JSON.stringify(request),
      signal
    })
  }
}
