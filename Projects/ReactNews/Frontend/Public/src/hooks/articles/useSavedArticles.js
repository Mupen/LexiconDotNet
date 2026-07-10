import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo } from 'react'
import { newsApi } from '../../api/articles/newsApi.js'
import { savedArticleKeys } from '../../api/articles/articleKeys.js'

/*
 * What: useSavedArticles owns frontend state for the reader's saved list.
 * How: TanStack Query loads saved articles, while mutations save/remove and
 * invalidate the saved-list query afterwards.
 * Why: saved articles are server state. Keeping this in a hook prevents pages
 * and cards from duplicating mutation/cache logic.
 */
export function useSavedArticles(enabled = true) {
  /*
   * What: queryClient lets mutations refresh cached saved-article data.
   * How: after save/remove succeeds, invalidateQueries marks the saved list stale.
   * Why: the UI should update from the backend's authoritative state instead of
   * manually guessing every possible cache update.
   */
  const queryClient = useQueryClient()

  /*
   * What: savedQuery loads the current saved article list.
   * How: the key comes from savedArticleKeys and the fetch function calls the
   * backend saved-article endpoint.
   * Why: every page/button can share one cached saved-list result.
   */
  const savedQuery = useQuery({
    queryKey: savedArticleKeys.list(),
    queryFn: ({ signal }) => newsApi.getSavedArticles({ signal }),
    enabled
  })

  /*
   * What: saveMutation sends a save command for one article id.
   * How: mutationFn calls the backend and onSuccess invalidates saved articles.
   * Why: save is a command, not a read query, and should refresh the saved list
   * when the backend accepts it.
   */
  const saveMutation = useMutation({
    mutationFn: (articleId) => newsApi.saveArticle(articleId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: savedArticleKeys.all })
  })

  /*
   * What: removeMutation sends a remove command for one article id.
   * How: mutationFn calls DELETE and onSuccess invalidates saved articles.
   * Why: the saved page and article buttons should reflect removal immediately
   * after the backend confirms it.
   */
  const removeMutation = useMutation({
    mutationFn: (articleId) => newsApi.removeSavedArticle(articleId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: savedArticleKeys.all })
  })

  const items = savedQuery.data?.items ?? []

  /*
   * What: savedIds gives components fast "is this saved?" checks.
   * How: useMemo rebuilds a Set only when saved items change.
   * Why: cards, tables, and details should not repeatedly scan the full saved
   * list for every render.
   */
  const savedIds = useMemo(() => new Set(items.map((article) => article.id)), [items])

  return {
    items,
    savedIds,
    loading: savedQuery.isFetching,
    saving: saveMutation.isPending,
    removing: removeMutation.isPending,
    error: savedQuery.error ?? saveMutation.error ?? removeMutation.error ?? null,
    saveArticle: (articleId) => enabled && saveMutation.mutate(articleId),
    removeSavedArticle: (articleId) => enabled && removeMutation.mutate(articleId),
    isSaved: (articleId) => savedIds.has(articleId)
  }
}
