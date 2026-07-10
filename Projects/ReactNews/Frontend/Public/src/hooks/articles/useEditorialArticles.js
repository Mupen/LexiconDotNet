import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { editorialArticleKeys } from '../../api/articles/articleKeys.js'
import { newsApi } from '../../api/articles/newsApi.js'

/*
 * What: useEditorialArticles owns frontend server state for admin-created articles.
 * How: TanStack Query loads the editorial list and mutations create/update/publish/archive articles.
 * Why: the editorial page should not duplicate HTTP/cache behavior inside UI event handlers.
 */
export function useEditorialArticles(enabled = true) {
  const queryClient = useQueryClient()
  const listQuery = useQuery({
    queryKey: editorialArticleKeys.list(),
    queryFn: ({ signal }) => newsApi.getEditorialArticles({ signal }),
    enabled
  })

  const invalidateEditorial = () => queryClient.invalidateQueries({ queryKey: editorialArticleKeys.all })

  const createMutation = useMutation({
    mutationFn: (article) => newsApi.createEditorialArticle(article),
    onSuccess: invalidateEditorial
  })

  const updateMutation = useMutation({
    mutationFn: ({ articleId, article }) => newsApi.updateEditorialArticle(articleId, article),
    onSuccess: invalidateEditorial
  })

  const publishMutation = useMutation({
    mutationFn: (articleId) => newsApi.publishEditorialArticle(articleId),
    onSuccess: invalidateEditorial
  })

  const archiveMutation = useMutation({
    mutationFn: (articleId) => newsApi.archiveEditorialArticle(articleId),
    onSuccess: invalidateEditorial
  })

  return {
    articles: listQuery.data?.items ?? [],
    loading: listQuery.isFetching,
    saving: createMutation.isPending || updateMutation.isPending || publishMutation.isPending || archiveMutation.isPending,
    error: listQuery.error ?? createMutation.error ?? updateMutation.error ?? publishMutation.error ?? archiveMutation.error ?? null,
    createArticle: (article) => enabled && createMutation.mutate(article),
    updateArticle: (articleId, article) => enabled && updateMutation.mutate({ articleId, article }),
    publishArticle: (articleId) => enabled && publishMutation.mutate(articleId),
    archiveArticle: (articleId) => enabled && archiveMutation.mutate(articleId)
  }
}
