import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'
import { articleKeys } from '../../api/articles/articleKeys.js'
import { newsApi } from '../../api/articles/newsApi.js'

const defaultParams = {
  mode: 'headlines',
  q: '',
  country: 'us',
  category: 'technology',
  source: '',
  language: 'en',
  sortBy: 'publishedAt',
  page: 1,
  pageSize: 20,
  view: 'cards'
}

/*
 * What: useNewsSearch is the main frontend state hook for article browsing.
 * How: it reads request state from URL search params, calls the backend with
 * TanStack Query, and exposes actions for
 * searching, paging, changing view, and loading headlines.
 * Why: keeping this logic in one hook prevents HomePage, SearchPanel, and table
 * controls from duplicating URL/query/cache behavior.
 */
export function useNewsSearch() {
  /*
   * What: searchParams/setSearchParams make the URL the source of truth for
   * article request state.
   * How: readRequest converts URLSearchParams into a normalized request object,
   * while updateRequest writes changes back to the URL.
   * Why: URL state makes searches bookmarkable, refresh-safe, and easier to
   * debug than hidden component-only state.
   */
  const [searchParams, setSearchParams] = useSearchParams()

  /*
   * What: request is the normalized article request derived from the URL.
   * How: useMemo recalculates it only when searchParams changes.
   * Why: many parts of the hook need the same request object, and keeping it
   * stable avoids unnecessary query key churn.
   */
  const request = useMemo(() => readRequest(searchParams), [searchParams])

  /*
   * What: articlesQuery loads the current article list from the backend.
   * How: TanStack Query caches by articleKeys.list(request), passes AbortSignal
   * to fetch, and keeps previous page data while a new page loads.
   * Why: server state should be owned by TanStack Query instead of copied into
   * many component-level states.
   */
  const articlesQuery = useQuery({
    queryKey: articleKeys.list(request),
    queryFn: ({ signal }) => newsApi.getArticles({ ...request, signal }),
    placeholderData: keepPreviousData
  })

  const articles = articlesQuery.data?.items ?? []
  const featuredArticle = articles[0] ?? null
  const supportingArticles = articles.slice(1)

  /*
   * What: updateRequest applies a partial change to the URL-backed request.
   * How: it reads the current URL request, merges the patch, and writes only
   * non-default values back to URLSearchParams.
   * Why: this keeps URLs clean while still allowing every control to update only
   * the fields it owns.
   */
  const updateRequest = useCallback((patch) => {
    setSearchParams((current) => {
      const nextRequest = {
        ...readRequest(current),
        ...patch
      }

      return writeRequest(nextRequest)
    })
  }, [setSearchParams])

  /*
   * What: loadTopHeadlines switches the app to headline mode.
   * How: it clears search text, keeps or overrides category/country, and resets
   * the page to 1.
   * Why: headline browsing and full-text search are different NewsAPI modes, so
   * switching modes should produce a clean request.
   */
  const loadTopHeadlines = useCallback(({ category, country } = {}) => {
    updateRequest({
      mode: 'headlines',
      q: '',
      category: category ?? request.category,
      country: country ?? request.country,
      page: 1
    })
  }, [request.category, request.country, updateRequest])

  /*
   * What: search switches the app to search mode.
   * How: it trims the query, updates search-specific fields, and resets the page
   * to 1. Very short queries still update URL state but avoid pretending the
   * request is a strong search.
   * Why: search controls should not keep the user on an old page number, and the
   * URL should reflect what the user typed.
   */
  const search = useCallback(({ query, language, sortBy, source } = {}) => {
    const q = (query ?? request.q).trim()

    if (q.length < 2) {
      updateRequest({
        mode: 'search',
        q,
        page: 1
      })
      return
    }

    updateRequest({
      mode: 'search',
      q,
      language: language ?? request.language,
      sortBy: sortBy ?? request.sortBy,
      source: source ?? request.source,
      page: 1
    })
  }, [request.language, request.q, request.sortBy, request.source, updateRequest])

  /*
   * What: goToPage changes the active result page.
   * How: it updates only the page field in the URL-backed request.
   * Why: pagination is part of the server request, so changing page should cause
   * TanStack Query to load a distinct cached result.
   */
  const goToPage = useCallback((page) => {
    updateRequest({ page })
  }, [updateRequest])

  /*
   * What: setView switches between card and table display.
   * How: it updates the view URL parameter without changing the backend filters.
   * Why: display mode is user-facing state worth preserving through refresh, but
   * it should not change which articles are fetched.
   */
  const setView = useCallback((view) => {
    updateRequest({ view })
  }, [updateRequest])

  /*
   * What: setPageSize changes how many articles the backend should return.
   * How: it writes pageSize and resets page to 1.
   * Why: changing page size can make the previous page number invalid or
   * confusing, so the safest behavior is to return to the first page.
   */
  const setPageSize = useCallback((pageSize) => {
    updateRequest({ pageSize, page: 1 })
  }, [updateRequest])

  /*
   * What: status converts TanStack Query state into display text.
   * How: errors, fetching, success, cache hits, and initial idle state each map
   * to a small status object used by StatusMessage.
   * Why: pages should not know TanStack Query's detailed state model. They only
   * need a simple message to show the user.
   */
  const status = useMemo(() => {
    if (articlesQuery.isError) {
      return { type: 'error', text: articlesQuery.error.message }
    }

    if (articlesQuery.isFetching) {
      return { type: 'idle', text: articlesQuery.isPlaceholderData ? 'Keeping previous page while loading.' : 'Loading articles.' }
    }

    if (articlesQuery.isSuccess) {
      return {
        type: 'success',
        text: articlesQuery.data.fromCache
          ? `Loaded ${articles.length} articles from backend cache.`
          : `Loaded ${articles.length} articles from NewsAPI through the backend.`
      }
    }

    return { type: 'idle', text: 'Ready' }
  }, [articles.length, articlesQuery])

  return {
    articles,
    featuredArticle,
    supportingArticles,
    totalResults: articlesQuery.data?.totalResults ?? 0,
    totalPages: articlesQuery.data?.totalPages ?? 0,
    page: request.page,
    pageSize: request.pageSize,
    mode: request.mode,
    queryKind: request.mode,
    cachedUntil: articlesQuery.data?.cachedUntilUtc ?? null,
    fetchedAt: articlesQuery.data?.fetchedAtUtc ?? null,
    fromCache: articlesQuery.data?.fromCache ?? false,
    loading: articlesQuery.isFetching,
    status,
    request,
    loadTopHeadlines,
    search,
    goToPage,
    setView,
    setPageSize
  }
}

/*
 * What: readRequest converts URLSearchParams into the request model used by the
 * frontend and backend.
 * How: it reads each known parameter, applies defaults, and normalizes enum-like
 * and numeric values.
 * Why: URL parameters are untrusted strings. Normalizing them here prevents bad
 * URLs from causing broken UI state.
 */
function readRequest(searchParams) {
  const mode = normalizeMode(searchParams.get('mode'))
  const page = normalizeNumber(searchParams.get('page'), defaultParams.page, 1, 100)
  const pageSize = normalizeNumber(searchParams.get('pageSize'), defaultParams.pageSize, 1, 100)

  return {
    mode,
    q: searchParams.get('q') ?? defaultParams.q,
    country: searchParams.get('country') ?? defaultParams.country,
    category: searchParams.get('category') ?? defaultParams.category,
    source: searchParams.get('source') ?? defaultParams.source,
    language: searchParams.get('language') ?? defaultParams.language,
    sortBy: searchParams.get('sortBy') ?? defaultParams.sortBy,
    page,
    pageSize,
    view: normalizeView(searchParams.get('view'))
  }
}

/*
 * What: writeRequest converts a request object back into URLSearchParams.
 * How: it writes only values that differ from defaults.
 * Why: shorter URLs are easier to read/share, and omitted defaults still produce
 * the same backend request when readRequest runs again.
 */
function writeRequest(request) {
  const next = new URLSearchParams()

  setIfNotDefault(next, 'mode', request.mode, defaultParams.mode)
  setIfNotDefault(next, 'q', request.q, defaultParams.q)
  setIfNotDefault(next, 'country', request.country, defaultParams.country)
  setIfNotDefault(next, 'category', request.category, defaultParams.category)
  setIfNotDefault(next, 'source', request.source, defaultParams.source)
  setIfNotDefault(next, 'language', request.language, defaultParams.language)
  setIfNotDefault(next, 'sortBy', request.sortBy, defaultParams.sortBy)
  setIfNotDefault(next, 'page', String(request.page), String(defaultParams.page))
  setIfNotDefault(next, 'pageSize', String(request.pageSize), String(defaultParams.pageSize))
  setIfNotDefault(next, 'view', request.view, defaultParams.view)

  return next
}

/*
 * What: setIfNotDefault conditionally writes one parameter.
 * How: undefined, null, empty string, and default values are skipped.
 * Why: this keeps the URL from filling with noise like page=1 or empty filters.
 */
function setIfNotDefault(searchParams, key, value, defaultValue) {
  if (value !== undefined && value !== null && value !== '' && value !== defaultValue) {
    searchParams.set(key, value)
  }
}

/*
 * What: normalizeMode restricts the request mode to known values.
 * How: only "search" survives; every other value becomes "headlines".
 * Why: invalid URLs should fall back to a usable default instead of breaking the
 * page or sending unsupported modes to the backend.
 */
function normalizeMode(value) {
  return value === 'search' ? 'search' : 'headlines'
}

/*
 * What: normalizeView restricts the visual display mode to known values.
 * How: only "table" survives; every other value becomes "cards".
 * Why: card view is the default reader experience, and bad URL values should not
 * create an invisible or unsupported layout.
 */
function normalizeView(value) {
  return value === 'table' ? 'table' : 'cards'
}

/*
 * What: normalizeNumber parses and clamps numeric URL values.
 * How: non-integers use the fallback; valid integers are clamped between min and
 * max.
 * Why: page and pageSize come from the URL, so users can enter bad values. The
 * frontend should correct them before sending requests.
 */
function normalizeNumber(value, fallback, min, max) {
  const parsed = Number(value)

  if (!Number.isInteger(parsed)) {
    return fallback
  }

  return Math.min(max, Math.max(min, parsed))
}
