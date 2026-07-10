import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { articleKeys } from '../../api/articles/articleKeys.js'
import { newsApi } from '../../api/articles/newsApi.js'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'

/*
 * What: ArticleDetailPage renders metadata for one article snapshot.
 * How: it reads articleId from the route, tries to seed initial data from
 * already-cached article lists, then asks the backend detail endpoint for the
 * persisted snapshot.
 * Why: NewsAPI does not provide a stable get-by-id endpoint for this app. The
 * backend remembers snapshots, and the frontend uses routing so details are
 * refreshable and shareable.
 */
export function ArticleDetailPage({ savedArticles }) {
  /*
   * What: articleId is the dynamic route value from /article/:articleId.
   * How: React Router reads it from the current URL.
   * Why: keeping the id in the URL is required for real navigation and avoids
   * relying only on hidden component state.
   */
  const { articleId } = useParams()

  /*
   * What: queryClient lets this page inspect existing list-query cache data.
   * How: findArticleInCachedLists searches cached article list responses before
   * the network request completes.
   * Why: detail pages can render instantly when the user opened an article from
   * the current list, while still asking the backend for the authoritative
   * persisted snapshot.
   */
  const queryClient = useQueryClient()

  const articleQuery = useQuery({
    queryKey: articleKeys.detail(articleId),
    queryFn: ({ signal }) => newsApi.getArticle(articleId, { signal }),
    initialData: () => findArticleInCachedLists(queryClient, articleId)
  })

  const article = articleQuery.data ?? null
  const status = articleQuery.isError
    ? { type: 'error', text: articleQuery.error.message }
    : articleQuery.isFetching
      ? { type: 'idle', text: 'Loading article metadata.' }
      : { type: 'success', text: 'Article metadata loaded.' }

  return (
    <>
      <StatusMessage status={status} />
      <article className="detail-page">
        <Link to="/" className="back-link">Back to news</Link>

        {!article && (
          <div className="panel">
            <h2>Article not available</h2>
            <p>Load headlines or run a search first, then open an article from the result list.</p>
          </div>
        )}

        {article && (
          <>
            {article.imageUrl && (
              <img
                src={article.imageUrl}
                alt=""
                className="detail-image"
                onError={(event) => {
                  event.currentTarget.remove()
                }}
              />
            )}

            <div className="detail-copy">
              <span className="source">{article.sourceName ?? 'Unknown source'}</span>
              <h1>{article.title}</h1>
              {article.description && <p className="lead">{article.description}</p>}

              <dl className="metadata-list">
                <div>
                  <dt>Author</dt>
                  <dd>{article.author ?? 'Unknown'}</dd>
                </div>
                <div>
                  <dt>Published</dt>
                  <dd>{article.publishedAt ? new Date(article.publishedAt).toLocaleString() : 'Unknown'}</dd>
                </div>
                <div>
                  <dt>Source</dt>
                  <dd>{article.sourceName ?? 'Unknown'}</dd>
                </div>
              </dl>

              {article.content && <p>{article.content}</p>}
              <div className="article-actions">
                <a href={article.url} target="_blank" rel="noreferrer" className="primary-link">
                  Open original article
                </a>
                <button
                  type="button"
                  className={savedArticles.isSaved(article.id) ? 'secondary' : ''}
                  disabled={savedArticles.saving || savedArticles.removing}
                  onClick={() => savedArticles.isSaved(article.id)
                    ? savedArticles.removeSavedArticle(article.id)
                    : savedArticles.saveArticle(article.id)}
                >
                  {savedArticles.isSaved(article.id) ? 'Saved' : 'Save for later'}
                </button>
              </div>
            </div>
          </>
        )}
      </article>
    </>
  )
}

/*
 * What: findArticleInCachedLists searches previously loaded article-list data.
 * How: TanStack Query returns all cached list queries; the function scans each
 * list for a matching article id.
 * Why: this improves perceived speed and keeps the detail page useful even
 * while the backend request is still in flight.
 */
function findArticleInCachedLists(queryClient, articleId) {
  const cachedLists = queryClient.getQueriesData({ queryKey: articleKeys.lists() })

  for (const [, data] of cachedLists) {
    const article = data?.items?.find((item) => item.id === articleId)

    if (article) {
      return article
    }
  }

  return undefined
}
