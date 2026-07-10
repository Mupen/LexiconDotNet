import { useQuery } from '@tanstack/react-query'
import { publicEditorialArticleKeys } from '../../api/articles/articleKeys.js'
import { newsApi } from '../../api/articles/newsApi.js'

/*
 * What: usePublishedEditorialArticles loads public first-party editorial content.
 * How: TanStack Query calls the public editorial backend endpoint and caches the result.
 * Why: the public editorial feed should reuse the same server-state pattern as NewsAPI feeds.
 */
export function usePublishedEditorialArticles() {
  const listQuery = useQuery({
    queryKey: publicEditorialArticleKeys.list(),
    queryFn: ({ signal }) => newsApi.getPublishedEditorialArticles({ signal })
  })

  return {
    articles: listQuery.data?.items ?? [],
    loading: listQuery.isFetching,
    error: listQuery.error ?? null
  }
}
