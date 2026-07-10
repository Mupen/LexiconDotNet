import { useEffect, useState } from 'react'

/*
 * What: SearchPanel renders article filter/search controls and reader preference controls.
 * How: it keeps local draft form state, then calls parent actions when the user
 * submits, changes page size, toggles preferences, or loads headlines. The
 * showArticleControls flag hides article-search controls when a page only needs
 * the preference fields.
 * Why: draft state lets users type/change controls without immediately firing a
 * backend request for every keystroke, and the same preference UI can be reused
 * without showing buttons that do nothing.
 */
export function SearchPanel({
  request,
  preferences,
  loading,
  onPreferencesChange,
  onSearch,
  onLoadHeadlines,
  onSetPageSize,
  showArticleControls = true
}) {
  /*
   * What: draft stores the editable form values.
   * How: it starts from the current URL-backed request and uses "react" as a
   * friendly default search term when no search query exists.
   * Why: form controls need controlled values, but the canonical request state
   * still belongs in the URL/search hook after the user submits.
   */
  const [draft, setDraft] = useState({
    q: request.q || 'react',
    country: request.country,
    language: request.language,
    sortBy: request.sortBy,
    pageSize: request.pageSize
  })

  /*
   * What: this effect resynchronizes the form when the URL/request changes.
   * How: whenever relevant request fields change, draft is replaced with those
   * current values.
   * Why: users can change categories, navigate browser history, or load saved
   * URLs. The form should reflect the actual active request instead of stale
   * previous input.
   */
  useEffect(() => {
    setDraft({
      q: request.q || 'react',
      country: request.country,
      language: request.language,
      sortBy: request.sortBy,
      pageSize: request.pageSize
    })
  }, [request.country, request.language, request.pageSize, request.q, request.sortBy])

  /*
   * What: updateField changes one field in the local draft object.
   * How: it copies the current draft and overwrites the named field.
   * Why: using one helper avoids repeating object-spread update logic for every
   * input/select control.
   */
  function updateField(field, value) {
    setDraft((current) => ({ ...current, [field]: value }))
  }

  /*
   * What: handleSubmit turns the draft search form into an actual search.
   * How: it prevents the browser's normal form navigation and calls onSearch
   * with the current draft values.
   * Why: React should update URL/query state itself instead of reloading the
   * whole page like a traditional HTML form submit.
   */
  function handleSubmit(event) {
    event.preventDefault()

    if (!showArticleControls) {
      return
    }

    onSearch({
      query: draft.q,
      language: draft.language,
      sortBy: draft.sortBy
    })
  }

  return (
    <section className="panel search-panel">
      <div className="panel-header">
        <h2>{showArticleControls ? 'Search' : 'Reader settings'}</h2>
      </div>

      <form onSubmit={handleSubmit}>
        {showArticleControls && (
          <>
            <label>
              Keywords
              <input
                value={draft.q}
                onChange={(event) => updateField('q', event.target.value)}
                placeholder="react, climate, finance"
              />
            </label>

            <div className="form-grid">
              <label>
                Country
                <select value={draft.country} onChange={(event) => updateField('country', event.target.value)}>
                  <option value="us">US</option>
                  <option value="se">Sweden</option>
                  <option value="gb">United Kingdom</option>
                  <option value="de">Germany</option>
                </select>
              </label>

              <label>
                Language
                <select value={draft.language} onChange={(event) => updateField('language', event.target.value)}>
                  <option value="en">English</option>
                  <option value="sv">Swedish</option>
                  <option value="de">German</option>
                  <option value="fr">French</option>
                </select>
              </label>
            </div>

            <label>
              Search sort
              <select value={draft.sortBy} onChange={(event) => updateField('sortBy', event.target.value)}>
                <option value="publishedAt">Newest</option>
                <option value="popularity">Popular publishers</option>
                <option value="relevancy">Most relevant</option>
              </select>
            </label>

          <label>
              Page size
              <select
                value={draft.pageSize}
                onChange={(event) => {
                  const pageSize = Number(event.target.value)
                  updateField('pageSize', pageSize)
                  onSetPageSize(pageSize)
                }}
              >
                <option value="10">10</option>
                <option value="20">20</option>
                <option value="30">30</option>
              </select>
            </label>
          </>
        )}

        <label className="checkbox-control">
          <input
            type="checkbox"
            checked={preferences.compactCards}
            onChange={(event) => onPreferencesChange({
              ...preferences,
              compactCards: event.target.checked
            })}
          />
          Compact article cards
        </label>

        <label>
          Theme
          <select
            value={preferences.theme}
            onChange={(event) => onPreferencesChange({
              ...preferences,
              theme: event.target.value
            })}
          >
            <option value="light">Light</option>
            <option value="dark">Dark</option>
          </select>
        </label>

        <label>
          Font size
          <select
            value={preferences.fontScale}
            onChange={(event) => onPreferencesChange({
              ...preferences,
              fontScale: Number(event.target.value)
            })}
          >
            <option value="0.9">Small</option>
            <option value="1">Normal</option>
            <option value="1.15">Large</option>
            <option value="1.3">Extra large</option>
          </select>
        </label>

        <fieldset className="category-preferences">
          <legend>Preferred categories</legend>
          {['business', 'entertainment', 'general', 'health', 'science', 'sports', 'technology'].map((category) => {
            const checked = preferences.preferredCategories.includes(category)

            return (
              <label key={category} className="checkbox-control">
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={(event) => {
                    const preferredCategories = event.target.checked
                      ? [...preferences.preferredCategories, category]
                      : preferences.preferredCategories.filter((item) => item !== category)

                    onPreferencesChange({
                      ...preferences,
                      preferredCategories: preferredCategories.length > 0 ? preferredCategories : preferences.preferredCategories
                    })
                  }}
                />
                {category}
              </label>
            )
          })}
        </fieldset>

        {showArticleControls && (
          <div className="button-row">
            <button type="submit" disabled={loading}>Search</button>
            <button type="button" className="secondary" disabled={loading} onClick={() => onLoadHeadlines({ country: draft.country })}>
              Headlines
            </button>
          </div>
        )}
      </form>
    </section>
  )
}
