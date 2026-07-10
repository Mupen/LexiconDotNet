import { useQuery } from '@tanstack/react-query'
import { publicEditorialArticleKeys } from '../../api/articles/articleKeys.js'
import { newsApi } from '../../api/articles/newsApi.js'

/*
 * What: usePublishedEditorialArticle loads one public editorial article.
 * How: TanStack Query calls the public detail endpoint with the route id.
 * Why: detail-page data fetching belongs in a hook so the page stays focused on rendering.
 */
export function usePublishedEditorialArticle(articleId) {
  const detailQuery = useQuery({
    queryKey: publicEditorialArticleKeys.detail(articleId),
    queryFn: ({ signal }) => newsApi.getPublishedEditorialArticle(articleId, { signal }),
    enabled: Boolean(articleId)
  })

  return {
    article: detailQuery.data ?? null,
    loading: detailQuery.isFetching,
    error: detailQuery.error ?? null
  }
}
