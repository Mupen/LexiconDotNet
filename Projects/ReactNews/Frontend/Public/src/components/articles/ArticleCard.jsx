import { Link } from 'react-router-dom'

/*
 * What: ArticleCard renders one article in the card/grid view.
 * How: it shows optional image, source, title, optional description, and links
 * to both the internal detail page and the original publisher URL.
 * Why: cards are the reader-friendly browsing surface, while the original link
 * keeps attribution and lets users leave ReactNews for the full article.
 */
export function ArticleCard({ article, compact, saved, saving, onSave, onRemoveSaved }) {
  return (
    <article className={`article-card ${compact ? 'compact' : ''}`}>
      {article.imageUrl && (
        <img
          src={article.imageUrl}
          alt=""
          loading="lazy"
          onError={(event) => {
            /*
             * What: broken article images are removed from the DOM.
             * How: the browser fires onError when the remote image cannot load,
             * and currentTarget is the failed img element.
             * Why: NewsAPI image URLs are third-party data. Removing broken
             * images keeps the card layout clean instead of showing a broken icon.
             */
            event.currentTarget.remove()
          }}
        />
      )}
      <div className="article-card-copy">
        <span className="source">{article.sourceName ?? 'Unknown source'}</span>
        <h3>{article.title}</h3>
        {!compact && article.description && <p>{article.description}</p>}
        <div className="article-actions">
          <Link to={`/article/${article.id}`}>Details</Link>
          <a href={article.url} target="_blank" rel="noreferrer">Original</a>
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
    </article>
  )
}
