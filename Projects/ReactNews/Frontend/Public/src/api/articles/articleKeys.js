/*
 * What: articleKeys centralizes TanStack Query cache key shapes.
 * How: each function returns an array key. TanStack Query compares those arrays
 * to decide which cached request result belongs to which UI state.
 * Why: hard-coding query keys in components is error-prone. A shared key factory
 * keeps list, detail, and source cache entries consistent as the app grows.
 */
export const articleKeys = {
  /*
   * What: all is the root namespace for article-related queries.
   * How: more specific keys spread this base array and append their own labels.
   * Why: a root key lets future code invalidate all article queries together.
   */
  all: ['articles'],

  /*
   * What: lists identifies all article-list queries.
   * How: individual list keys append request parameters after this segment.
   * Why: this separates list data from article detail data in the query cache.
   */
  lists: () => [...articleKeys.all, 'list'],

  /*
   * What: list identifies one article-list request.
   * How: the full request parameter object becomes part of the cache key.
   * Why: page 1 technology headlines and page 2 sports headlines must not share
   * the same cached response.
   */
  list: (params) => [...articleKeys.lists(), params],

  /*
   * What: details identifies all detail-page queries.
   * How: individual article ids are appended by detail(articleId).
   * Why: detail snapshots have different caching behavior from article lists.
   */
  details: () => [...articleKeys.all, 'detail'],

  /*
   * What: detail identifies one article snapshot request.
   * How: the generated article id is appended as the final key part.
   * Why: each detail page needs its own cache entry.
   */
  detail: (articleId) => [...articleKeys.details(), articleId],

  /*
   * What: sources identifies source-list requests.
   * How: optional filters become part of the cache key.
   * Why: source lists can differ by category, country, or language, so their
   * cached results must stay separate.
   */
  sources: (params = {}) => ['sources', params]
}

/*
 * What: savedArticleKeys centralizes TanStack Query keys for saved articles.
 * How: the list key is separate from article list/detail keys because saved
 * articles are reader state, not fetched news feed state.
 * Why: save/remove mutations can invalidate only saved-article data without
 * forcing every article search query to reload.
 */
export const savedArticleKeys = {
  all: ['savedArticles'],
  list: () => [...savedArticleKeys.all, 'list']
}

/*
 * What: readerPreferenceKeys centralizes cache keys for persisted reader preferences.
 * How: one detail key represents the single local reader profile.
 * Why: future account support can add user-specific keys without rewriting components.
 */
export const readerPreferenceKeys = {
  all: ['readerPreferences'],
  detail: () => [...readerPreferenceKeys.all, 'detail']
}

/*
 * What: editorialArticleKeys centralizes query keys for admin-created articles.
 * How: list and detail keys live under one editorial root namespace.
 * Why: editorial cache invalidation should not disturb external NewsAPI article queries.
 */
export const editorialArticleKeys = {
  all: ['editorialArticles'],
  list: () => [...editorialArticleKeys.all, 'list'],
  detail: (id) => [...editorialArticleKeys.all, 'detail', id]
}

/*
 * What: publicEditorialArticleKeys centralizes cache keys for published
 * ReactNews-owned articles.
 * How: it uses a different root key than the admin editorial workspace.
 * Why: public readers should cache only published content, while admins need a
 * separate cache that includes drafts and archived articles.
 */
export const publicEditorialArticleKeys = {
  all: ['publicEditorialArticles'],
  list: () => [...publicEditorialArticleKeys.all, 'list'],
  detail: (id) => [...publicEditorialArticleKeys.all, 'detail', id]
}

/*
 * What: authKeys centralizes cache keys for account/session state.
 * How: the current user is represented by one "me" key.
 * Why: login/logout can invalidate or replace this one key to update navigation and protected pages.
 */
export const authKeys = {
  all: ['auth'],
  me: () => [...authKeys.all, 'me']
}
