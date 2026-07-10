# Frontend Structure

Frontend app:

```text
Frontend/Public
```

## Routes

| Route | Purpose | Auth |
| --- | --- | --- |
| `/` | Redirect to `/news` | Public |
| `/news` | NewsAPI headlines/search | Public |
| `/article/:articleId` | NewsAPI snapshot detail | Public |
| `/editorial-feed` | Published ReactNews editorial articles | Public |
| `/editorial-feed/:articleId` | Published editorial detail | Public |
| `/login` | Login/register | Public |
| `/personal` | Reader dashboard | Reader/Admin |
| `/saved` | Saved articles | Reader/Admin |
| `/profile` | Account/preferences | Reader/Admin |
| `/editorial` | Admin editorial workspace | Admin |

## Source Layout

```text
src/
  api/
  app/
  components/
  hooks/
  pages/
  storage/
  styles/
```

The frontend is organized mostly by file type, then by feature. This keeps the structure compact while the app remains a single frontend.

## Important Hooks

| Hook | Purpose |
| --- | --- |
| `useAuth` | current user, login/register/logout, profile/password/delete |
| `useNewsSearch` | public news search state and queries |
| `useSavedArticles` | saved article list/save/remove |
| `useReaderPreferences` | theme/font/compact/categories |
| `useEditorialArticles` | admin editorial list/create/publish/archive |
| `usePublishedEditorialArticles` | public editorial feed |
| `usePublishedEditorialArticle` | public editorial detail |

## State Ownership

URL params own news search filters.

TanStack Query owns backend request state.

React component state owns temporary forms and drafts.

Backend owns users, permissions, saved articles, preferences, editorial articles, snapshots, and NewsAPI calls.

Local storage is only a harmless fallback for preferences.

## CSS

Main entry:

```text
src/styles.css
```

It imports smaller files for base styles, layout, components, pages, and responsive breakpoints.
