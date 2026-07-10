/*
 * What: topics lists the NewsAPI headline categories exposed in the UI.
 * How: TopicTabs maps this array into category buttons.
 * Why: keeping the list in one place makes it obvious which categories are
 * supported and avoids repeating button markup manually.
 */
const topics = [
  'general',
  'business',
  'technology',
  'science',
  'health',
  'sports',
  'entertainment'
]

/*
 * What: TopicTabs lets users switch headline categories.
 * How: each topic button calls onSelectCategory with its category name and uses
 * activeCategory to decide which button should look selected.
 * Why: category selection is navigation within the news feed, but the component
 * should not know how URL params or backend calls are updated.
 */
export function TopicTabs({ activeCategory, loading, onSelectCategory }) {
  return (
    <div className="topic-tabs" aria-label="Headline categories">
      {topics.map((topic) => (
        <button
          key={topic}
          type="button"
          className={activeCategory === topic ? 'active' : ''}
          disabled={loading}
          onClick={() => onSelectCategory(topic)}
        >
          {topic}
        </button>
      ))}
    </div>
  )
}
