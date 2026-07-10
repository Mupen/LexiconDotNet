import { Link } from 'react-router-dom'
import { ArticleCard } from '../../components/articles/ArticleCard.jsx'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'

/*
 * What: SavedArticlesPage renders the reader's saved-for-later list.
 * How: it receives savedArticles state/actions from App and maps saved items to
 * ArticleCard components with remove support.
 * Why: saved articles are persistent reader data, so they need their own route
 * and view instead of only being represented by buttons in the feed.
 */
export function SavedArticlesPage({ savedArticles }) {
  /*
   * What: status describes the saved-list loading/error/success state.
   * How: it reads loading, error, and item count from the savedArticles hook.
   * Why: the page should give feedback when saved data is loading or unavailable.
   */
  const status = savedArticles.error
    ? { type: 'error', text: savedArticles.error.message }
    : savedArticles.loading
      ? { type: 'idle', text: 'Loading saved articles.' }
      : { type: 'success', text: `Loaded ${savedArticles.items.length} saved articles.` }

  return (
    <>
      <StatusMessage status={status} />
      <section className="saved-page">
        <div className="section-header">
          <div>
            <h2>Saved Articles</h2>
            <p>Articles saved from headlines and search results for later reading.</p>
          </div>
          <Link to="/" className="primary-link">Back to news</Link>
        </div>

        {savedArticles.items.length === 0 && !savedArticles.loading && (
          <div className="panel">
            <h3>No saved articles</h3>
            <p>Save articles from the news feed or detail page to build a reading list.</p>
          </div>
        )}

        <div className="article-grid">
          {savedArticles.items.map((article) => (
            <ArticleCard
              key={article.id}
              article={article}
              compact={false}
              saved
              saving={savedArticles.removing}
              onSave={savedArticles.saveArticle}
              onRemoveSaved={savedArticles.removeSavedArticle}
            />
          ))}
        </div>
      </section>
    </>
  )
}
