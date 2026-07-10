import { Link } from 'react-router-dom'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'
import { usePublishedEditorialArticles } from '../../hooks/articles/usePublishedEditorialArticles.js'

/*
 * What: EditorialFeedPage shows published articles written inside ReactNews.
 * How: it loads public editorial articles through a hook and renders each item
 * as a first-party article card linking to a ReactNews detail page.
 * Why: admin-created articles only matter to readers once published content has
 * a public browsing surface separate from external NewsAPI feeds.
 */
export function EditorialFeedPage() {
  const editorial = usePublishedEditorialArticles()
  const status = editorial.error
    ? { type: 'error', text: editorial.error.message }
    : editorial.loading
      ? { type: 'idle', text: 'Loading ReactNews articles.' }
      : { type: 'success', text: `Loaded ${editorial.articles.length} ReactNews articles.` }

  return (
    <>
      <StatusMessage status={status} />
      <main className="content-stack">
        <section className="panel editorial-feed-intro">
          <div>
            <p className="eyebrow">ReactNews editorial</p>
            <h2>Editorial Feed</h2>
            <p>Published articles written and managed inside this ReactNews project.</p>
          </div>
        </section>

        {editorial.articles.length === 0 && !editorial.loading && (
          <section className="panel empty-state">
            <h3>No published editorial articles</h3>
            <p>Publish an article from the admin editorial workspace to make it appear here.</p>
          </section>
        )}

        <section className="editorial-feed-list">
          {editorial.articles.map((article) => (
            <article key={article.id} className="panel editorial-feed-card">
              {article.imageUrl && (
                <img
                  src={article.imageUrl}
                  alt=""
                  loading="lazy"
                  onError={(event) => {
                    event.currentTarget.remove()
                  }}
                />
              )}
              <div>
                <span className="source">{article.category} · {article.author}</span>
                <h3>{article.title}</h3>
                <p>{article.summary}</p>
                <div className="article-actions">
                  <Link to={`/editorial-feed/${article.id}`}>Read article</Link>
                  {article.publishedAtUtc && <span>{new Date(article.publishedAtUtc).toLocaleDateString()}</span>}
                </div>
              </div>
            </article>
          ))}
        </section>
      </main>
    </>
  )
}
