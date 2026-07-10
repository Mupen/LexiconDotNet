import { Link } from 'react-router-dom'
import { ArticleCard } from '../../components/articles/ArticleCard.jsx'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'

/*
 * What: PersonalPage shows the reader-facing personalized dashboard.
 * How: it combines persisted preferred categories, saved articles, and shortcuts back into the news feed.
 * Why: a professional news site needs a page that feels personal without mixing that purpose into the general public news page.
 */
export function PersonalPage({ news, savedArticles, readerPreferences }) {
  const preferences = readerPreferences.preferences
  const recentSaved = savedArticles.items.slice(0, 3)
  const status = readerPreferences.error
    ? { type: 'error', text: readerPreferences.error.message }
    : { type: 'success', text: `Personal feed uses ${preferences.preferredCategories.length} preferred categories.` }

  return (
    <>
      <StatusMessage status={status} />
      <section className="personal-page">
        <div className="section-header">
          <div>
            <p className="eyebrow">Reader dashboard</p>
            <h2>Personal</h2>
            <p>Preferred categories, saved reading, and display settings in one place.</p>
          </div>
          <Link className="primary-link" to="/profile">Edit profile</Link>
        </div>

        <div className="dashboard-cards">
          <article className="panel">
            <h3>Preferred categories</h3>
            <div className="topic-tabs personal-topics">
              {preferences.preferredCategories.map((category) => (
                <button
                  key={category}
                  type="button"
                  className="secondary"
                  onClick={() => news.loadTopHeadlines({ category })}
                >
                  {category}
                </button>
              ))}
            </div>
            <p>Open a category to refresh the news page with that topic.</p>
            <Link className="primary-link" to="/news">Go to news</Link>
          </article>

          <article className="panel">
            <h3>Reading setup</h3>
            <dl className="metadata-list">
              <div>
                <dt>Theme</dt>
                <dd>{preferences.theme}</dd>
              </div>
              <div>
                <dt>Font size</dt>
                <dd>{preferences.fontScale}</dd>
              </div>
              <div>
                <dt>Compact cards</dt>
                <dd>{preferences.compactCards ? 'Enabled' : 'Disabled'}</dd>
              </div>
            </dl>
          </article>

          <article className="panel">
            <h3>Saved count</h3>
            <p className="metric">{savedArticles.items.length}</p>
            <Link className="primary-link" to="/saved">Open saved</Link>
          </article>
        </div>

        <div className="section-header">
          <div>
            <h3>Recent saved articles</h3>
            <p>The newest saved items are shown here as a preview of the full reading list.</p>
          </div>
        </div>

        {recentSaved.length === 0 && (
          <div className="panel">
            <p>No saved articles yet. Save articles from the news feed to fill this area.</p>
          </div>
        )}

        <div className="article-grid">
          {recentSaved.map((article) => (
            <ArticleCard
              key={article.id}
              article={article}
              compact={preferences.compactCards}
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
