import { Link } from 'react-router-dom'

/*
 * What: FeaturedArticle highlights the first article from the current result
 * set.
 * How: HomePage passes the first loaded article as article. If no article exists,
 * the component renders a useful empty state instead of blank space.
 * Why: a featured article gives the page hierarchy and avoids making every
 * headline look equally important.
 */
export function FeaturedArticle({ article, saved, saving, onSave, onRemoveSaved }) {
  if (!article) {
    return (
      <section className="featured-article empty-feature">
        <h2>No article selected</h2>
        <p>Run a search or load headlines to fill the page.</p>
      </section>
    )
  }

  return (
    <section className="featured-article">
      {article.imageUrl && (
        <img
          src={article.imageUrl}
          alt=""
          onError={(event) => {
            /*
             * What: broken hero images are removed.
             * How: the failed img element removes itself when the browser cannot
             * load the external image URL.
             * Why: publisher image links can expire or block hotlinking. The
             * article text should remain usable even when media fails.
             */
            event.currentTarget.remove()
          }}
        />
      )}
      <div className="featured-copy">
        <span className="source">{article.sourceName ?? 'Unknown source'}</span>
        <h2>{article.title}</h2>
        {article.description && <p>{article.description}</p>}
        <div className="article-actions">
          <Link to={`/article/${article.id}`}>Read metadata</Link>
          <a href={article.url} target="_blank" rel="noreferrer">Open original</a>
          <button
            type="button"
            className={saved ? 'secondary' : ''}
            disabled={saving}
            onClick={() => saved ? onRemoveSaved(article.id) : onSave(article.id)}
          >
            {saved ? 'Saved' : 'Save'}
          </button>
        </div>
      </div>
    </section>
  )
}
