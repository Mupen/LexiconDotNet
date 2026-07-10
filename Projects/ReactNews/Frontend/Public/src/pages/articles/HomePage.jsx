import { ArticleCard } from '../../components/articles/ArticleCard.jsx'
import { ArticleTable } from '../../components/articles/ArticleTable.jsx'
import { FeaturedArticle } from '../../components/articles/FeaturedArticle.jsx'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'
import { SearchPanel } from '../../components/forms/SearchPanel.jsx'
import { TopicTabs } from '../../components/navigation/TopicTabs.jsx'

/*
 * What: HomePage renders the main reader dashboard for headlines and search.
 * How: it receives the complete news state/actions object from App, then splits
 * that data into status, topic tabs, featured article, list/table view, paging,
 * and search controls.
 * Why: the page should coordinate layout, not own data fetching. Keeping the
 * query logic in useNewsSearch makes this component easier to read as UI.
 */
export function HomePage({ news, savedArticles, readerPreferences }) {
  /*
   * What: selectCategory switches the feed to top headlines for one category.
   * How: it delegates to the hook action that updates URL search params and
   * resets paging.
   * Why: category tabs should not know the backend request shape. They only
   * express user intent: "show this category".
   */
  function selectCategory(category) {
    news.loadTopHeadlines({ category })
  }

  /*
   * What: canGoBack/canGoForward decide whether paging buttons are enabled.
   * How: the current page and totalPages values come from the backend response
   * through useNewsSearch.
   * Why: disabling impossible page actions avoids invalid requests and gives the
   * user a clearer signal than letting them click into a no-op.
   */
  const canGoBack = news.page > 1
  const canGoForward = news.totalPages > news.page

  return (
    <>
      <StatusMessage status={news.status} />

      <section className="dashboard-layout">
        <div className="main-column">
          <TopicTabs
            activeCategory={news.request.category}
            loading={news.loading}
            onSelectCategory={selectCategory}
          />

          {news.request.view === 'cards' && (
            <FeaturedArticle
              article={news.featuredArticle}
              saved={news.featuredArticle ? savedArticles.isSaved(news.featuredArticle.id) : false}
              saving={savedArticles.saving || savedArticles.removing}
              onSave={savedArticles.saveArticle}
              onRemoveSaved={savedArticles.removeSavedArticle}
            />
          )}

          <div className="section-header">
            <div>
              <h2>{news.mode === 'search' ? 'Search Results' : 'Latest Headlines'}</h2>
              <p>
                {news.totalResults} total results. Page {news.page} of {Math.max(1, news.totalPages)}.
                Cache expires {formatCacheTime(news.cachedUntil)}.
              </p>
            </div>
            <div className="view-toggle">
              <button
                type="button"
                className={news.request.view === 'cards' ? 'active' : 'secondary'}
                onClick={() => news.setView('cards')}
              >
                Cards
              </button>
              <button
                type="button"
                className={news.request.view === 'table' ? 'active' : 'secondary'}
                onClick={() => news.setView('table')}
              >
                Table
              </button>
            </div>
            <div className="pager">
              <button
                type="button"
                className="secondary"
                disabled={news.loading || !canGoBack}
                onClick={() => news.goToPage(news.page - 1)}
              >
                Previous
              </button>
              <span>Page {news.page}</span>
              <button
                type="button"
                className="secondary"
                disabled={news.loading || !canGoForward}
                onClick={() => news.goToPage(news.page + 1)}
              >
                Next
              </button>
            </div>
          </div>

          {news.request.view === 'cards' && (
            <div className="article-grid">
              {news.supportingArticles.map((article) => (
                <ArticleCard
                  key={article.id}
                  article={article}
                  compact={readerPreferences.preferences.compactCards}
                  saved={savedArticles.isSaved(article.id)}
                  saving={savedArticles.saving || savedArticles.removing}
                  onSave={savedArticles.saveArticle}
                  onRemoveSaved={savedArticles.removeSavedArticle}
                />
              ))}
            </div>
          )}

          {news.request.view === 'table' && (
            <ArticleTable
              articles={news.articles}
              totalResults={news.totalResults}
              page={news.page}
              pageSize={news.pageSize}
              loading={news.loading}
              savedIds={savedArticles.savedIds}
              saving={savedArticles.saving || savedArticles.removing}
              onSave={savedArticles.saveArticle}
              onRemoveSaved={savedArticles.removeSavedArticle}
              onPageChange={news.goToPage}
            />
          )}
        </div>

        <aside>
          <SearchPanel
            request={news.request}
            preferences={readerPreferences.preferences}
            loading={news.loading}
            onPreferencesChange={readerPreferences.updatePreferences}
            onSearch={news.search}
            onLoadHeadlines={news.loadTopHeadlines}
            onSetPageSize={news.setPageSize}
          />
        </aside>
      </section>
    </>
  )
}

/*
 * What: formatCacheTime turns the backend cache expiry timestamp into display
 * text for the article list header.
 * How: missing values become "unknown"; valid timestamps are formatted with the
 * user's browser locale.
 * Why: cache metadata is useful for learning/debugging, but the UI should not
 * crash if the backend omits it or a request has not completed yet.
 */
function formatCacheTime(value) {
  if (!value) {
    return 'unknown'
  }

  return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}
