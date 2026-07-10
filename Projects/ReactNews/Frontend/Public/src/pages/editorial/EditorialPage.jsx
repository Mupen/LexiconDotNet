import { useState } from 'react'
import { StatusMessage } from '../../components/common/StatusMessage.jsx'

/*
 * What: EditorialPage shows the admin workspace for ReactNews-owned articles.
 * How: it renders a draft editor, preview panel, and table of local editorial articles using local component state.
 * Why: external NewsAPI content and admin-created content are different features, so the admin workflow needs its own page.
 */
export function EditorialPage({ editorialArticles }) {
  const [draft, setDraft] = useState({
    title: '',
    category: 'general',
    author: 'Admin Editor',
    imageUrl: '',
    summary: '',
    body: '',
    status: 'Draft'
  })

  const previewTitle = draft.title.trim() || 'Untitled editorial article'
  const previewSummary = draft.summary.trim() || 'Summary preview appears here while the editor writes.'

  const status = editorialArticles.error
    ? { type: 'error', text: editorialArticles.error.message }
    : editorialArticles.loading
      ? { type: 'idle', text: 'Loading editorial articles.' }
      : { type: 'success', text: `Loaded ${editorialArticles.articles.length} editorial articles.` }

  /*
   * What: Updates one editable field in the draft article form.
   * How: copies the current draft object and replaces only the changed property.
   * Why: the editorial form has many fields, and one helper keeps each input from
   * needing its own state setter.
   */
  function updateField(field, value) {
    setDraft((current) => ({ ...current, [field]: value }))
  }

  /*
   * What: Sends the current draft to the editorial article hook.
   * How: passes the full draft object to createArticle, which performs the API
   * mutation and refresh behavior.
   * Why: the page should keep form state locally while the hook owns server
   * communication and cache updates.
   */
  function saveDraft() {
    editorialArticles.createArticle(draft)
  }

  return (
    <section className="editorial-page">
      <StatusMessage status={status} />
      <div className="section-header">
        <div>
          <p className="eyebrow">Admin workspace</p>
          <h2>Editorial</h2>
          <p>Create, preview, and manage ReactNews-owned articles.</p>
        </div>
      </div>

      <div className="editorial-layout">
        <form className="panel editorial-form">
          <h3>Create article</h3>
          <label>
            Title
            <input value={draft.title} onChange={(event) => updateField('title', event.target.value)} />
          </label>
          <div className="form-grid">
            <label>
              Category
              <select value={draft.category} onChange={(event) => updateField('category', event.target.value)}>
                <option value="general">General</option>
                <option value="business">Business</option>
                <option value="technology">Technology</option>
                <option value="health">Health</option>
                <option value="sports">Sports</option>
              </select>
            </label>
            <label>
              Status
              <select value={draft.status} onChange={(event) => updateField('status', event.target.value)}>
                <option value="Draft">Draft</option>
                <option value="Review">Review</option>
                <option value="Published">Published</option>
              </select>
            </label>
          </div>
          <label>
            Author
            <input value={draft.author} onChange={(event) => updateField('author', event.target.value)} />
          </label>
          <label>
            Image URL
            <input value={draft.imageUrl} onChange={(event) => updateField('imageUrl', event.target.value)} />
          </label>
          <label>
            Summary
            <textarea value={draft.summary} onChange={(event) => updateField('summary', event.target.value)} />
          </label>
          <label>
            Body
            <textarea rows="8" value={draft.body} onChange={(event) => updateField('body', event.target.value)} />
          </label>
          <div className="button-row">
            <button type="button" disabled={editorialArticles.saving} onClick={saveDraft}>Save draft</button>
          </div>
        </form>

        <aside className="panel editorial-preview">
          <p className="eyebrow">{draft.category}</p>
          <h3>{previewTitle}</h3>
          <p>{previewSummary}</p>
          {draft.imageUrl && <img src={draft.imageUrl} alt="" />}
        </aside>
      </div>

      <section className="article-table-panel">
        <div className="section-header">
          <h3>Editorial articles</h3>
        </div>
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Category</th>
                <th>Author</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {editorialArticles.articles.map((article) => (
                <tr key={article.id}>
                  <td>{article.title}</td>
                  <td>{article.status}</td>
                  <td>{article.category}</td>
                  <td>{article.author}</td>
                  <td>
                    <div className="table-actions">
                      <button
                        type="button"
                        className="secondary"
                        disabled={editorialArticles.saving || article.status === 'Published'}
                        onClick={() => editorialArticles.publishArticle(article.id)}
                      >
                        Publish
                      </button>
                      <button
                        type="button"
                        className="secondary"
                        disabled={editorialArticles.saving || article.status === 'Archived'}
                        onClick={() => editorialArticles.archiveArticle(article.id)}
                      >
                        Archive
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </section>
  )
}
