import { Link, useParams } from 'react-router-dom'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'
import { usePublishedEditorialArticle } from '../../hooks/articles/usePublishedEditorialArticle.js'

/*
 * What: EditorialDetailPage renders one published ReactNews-owned article.
 * How: it reads the article id from the URL, loads the public detail endpoint,
 * and displays article metadata plus the full body.
 * Why: first-party editorial articles need shareable reader URLs just like
 * external NewsAPI snapshots.
 */
export function EditorialDetailPage() {
  const { articleId } = useParams()
  const editorial = usePublishedEditorialArticle(articleId)
  const article = editorial.article
  const status = editorial.error
    ? { type: 'error', text: editorial.error.message }
    : editorial.loading
      ? { type: 'idle', text: 'Loading editorial article.' }
      : { type: 'success', text: 'Editorial article loaded.' }

  return (
    <>
      <StatusMessage status={status} />
      <article className="detail-page editorial-detail">
        <Link to="/editorial-feed" className="back-link">Back to editorial feed</Link>

        {!article && !editorial.loading && (
          <div className="panel">
            <h2>Editorial article not available</h2>
            <p>The article may not exist, or it may no longer be published.</p>
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
              <span className="source">{article.category} · {article.author}</span>
              <h1>{article.title}</h1>
              <p className="lead">{article.summary}</p>

              <dl className="metadata-list">
                <div>
                  <dt>Author</dt>
                  <dd>{article.author}</dd>
                </div>
                <div>
                  <dt>Category</dt>
                  <dd>{article.category}</dd>
                </div>
                <div>
                  <dt>Published</dt>
                  <dd>{article.publishedAtUtc ? new Date(article.publishedAtUtc).toLocaleString() : 'Unknown'}</dd>
                </div>
              </dl>

              <div className="editorial-body">
                {article.body.split('\n').filter(Boolean).map((paragraph) => (
                  <p key={paragraph}>{paragraph}</p>
                ))}
              </div>
            </div>
          </>
        )}
      </article>
    </>
  )
}
