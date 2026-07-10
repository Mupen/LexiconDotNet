# API Reference

Base URL when running locally:

```text
http://localhost:5217
```

ReactNews uses cookie authentication. After login/register, the backend sends an HttpOnly `ReactNews.Auth` cookie. Frontend requests include cookies with `credentials: include`.

## Public Endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/health` | Check that the API is running |
| `GET` | `/api/articles` | List NewsAPI headlines/search results |
| `GET` | `/api/articles/{articleId}` | Load stored article snapshot detail |
| `GET` | `/api/sources` | List NewsAPI sources |
| `GET` | `/api/public/editorial/articles` | List published ReactNews editorial articles |
| `GET` | `/api/public/editorial/articles/{id}` | Load one published editorial article |

Article query examples:

```text
/api/articles?mode=headlines&country=us&category=technology&page=1&pageSize=20
/api/articles?mode=search&q=react&language=en&sortBy=publishedAt&page=1&pageSize=20
```

## Auth Endpoints

| Method | Endpoint | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | Create Reader account and sign in |
| `POST` | `/api/auth/login` | Public | Sign in |
| `POST` | `/api/auth/logout` | Reader/Admin | Sign out |
| `GET` | `/api/auth/me` | Reader/Admin | Get current user |
| `PUT` | `/api/auth/profile` | Reader/Admin | Update display name |
| `PUT` | `/api/auth/password` | Reader/Admin | Change password |
| `DELETE` | `/api/auth/account` | Reader/Admin | Delete current account |

Public registration always creates `Reader`. Admin users are created through backend `AdminSeed` configuration.

## Reader Endpoints

| Method | Endpoint | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/saved-articles` | Reader/Admin | List saved articles |
| `POST` | `/api/saved-articles/{articleId}` | Reader/Admin | Save article snapshot |
| `DELETE` | `/api/saved-articles/{articleId}` | Reader/Admin | Remove saved article |
| `GET` | `/api/reader-preferences` | Reader/Admin | Load preferences |
| `PUT` | `/api/reader-preferences` | Reader/Admin | Save preferences |

Saved articles are saved from the snapshot store, so the article must first have appeared in `/api/articles` results.

## Admin Endpoints

| Method | Endpoint | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/editorial/articles` | Admin | List all editorial articles |
| `GET` | `/api/editorial/articles/{id}` | Admin | Load one editorial article |
| `POST` | `/api/editorial/articles` | Admin | Create editorial article |
| `PUT` | `/api/editorial/articles/{id}` | Admin | Update editorial article |
| `POST` | `/api/editorial/articles/{id}/publish` | Admin | Publish article |
| `POST` | `/api/editorial/articles/{id}/archive` | Admin | Archive article |

Editorial statuses:

```text
Draft
Review
Published
Archived
```

## Errors

Expected failures return simple error JSON:

```json
{
  "code": "validation_error",
  "error": "Message"
}
```

Common status codes:

| Code | Meaning |
| --- | --- |
| `400` | Validation problem |
| `401` | Not signed in |
| `403` | Signed in but wrong role |
| `404` | Missing resource |
| `502` | NewsAPI/provider problem |
