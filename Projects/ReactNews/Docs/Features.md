# Features

ReactNews is a news website with public news browsing, reader accounts, saved articles, preferences, and admin editorial publishing.

## Public Features

- Browse NewsAPI headlines.
- Search NewsAPI articles.
- Filter by category, country, language, source, sort, page, and page size.
- Switch between card and table views.
- Open article detail pages at `/article/:articleId`.
- Open original publisher links.
- Browse published ReactNews editorial articles at `/editorial-feed`.
- Open editorial detail pages at `/editorial-feed/:articleId`.

## Reader Features

Readers can:

- register, log in, and log out
- save articles for later
- remove saved articles
- view saved articles at `/saved`
- use the personal page at `/personal`
- update preferences: theme, font size, compact cards, preferred categories
- edit display name
- change password
- delete account

Saved articles and preferences are stored per user in SQLite.

## Admin Features

Admins can do everything Readers can do, plus:

- open `/editorial`
- create editorial articles
- publish articles
- archive articles
- manage first-party ReactNews content separately from external NewsAPI articles

Public registration never creates Admin users. Admin users are created by backend `AdminSeed` configuration.

## Persistence

SQLite stores:

- users
- saved articles
- reader preferences
- editorial articles
- article snapshots
- EF Core migration history

EF Core migrations own the schema.

## Caching

The backend uses memory cache for NewsAPI responses. Cache improves speed and reduces repeated API calls, but it is not the source of truth.

## Future Work

Good future additions:

- better editorial validation and editing UX
- comments/moderation
- admin invite flow
- production Identity flow
- richer personalization and subscriptions
