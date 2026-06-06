namespace DataDrivenCaching.Domain.Entities;

// WHAT:
// LabUser represents a user account that is stored in the backend database.
//
// WHY:
// This project needs at least one durable, backend-owned data type so the demos
// can compare browser storage and cache layers against real authoritative data.
// A user account is useful for that because it naturally teaches ownership,
// identity, trust boundaries, and password handling.
//
// DATA DESIGN:
// This entity is authoritative backend data. The browser may display parts of
// it, but the browser must not be trusted to decide whether the user exists,
// what their role is, or what password hash belongs to the account.
public sealed class LabUser
{
    // WHAT:
    // Id is the database identity for this user row.
    //
    // WHY:
    // Durable backend records need stable identifiers. Client-side values such
    // as localStorage keys are not enough because users can change them.
    public int Id { get; set; }

    // WHAT:
    // UserName is the public login/display name for the account.
    //
    // WHY:
    // This is safe to show in the UI, but the backend still owns the canonical
    // value because account identity is authoritative server data.
    public required string UserName { get; set; }

    // WHAT:
    // DisplayName is friendly UI text for the account.
    //
    // WHY:
    // This lets the frontend show a readable name without exposing sensitive
    // identity data such as the password hash. It is still backend-owned data,
    // because the browser should not decide who the logged-in user is.
    public required string DisplayName { get; set; }

    // WHAT:
    // PasswordHash stores the result of hashing a password, not the password.
    //
    // WHY:
    // Passwords must never be stored as plain text. A later implementation
    // should hash the password using a proper password hasher before assigning
    // this value. The database should only receive the hash.
    //
    // SECURITY:
    // If the SQLite database is copied or leaked, a hash is still sensitive,
    // but it is much safer than a raw password. The raw password should exist
    // only briefly in request memory while it is being verified or hashed.
    public required string PasswordHash { get; set; }

    // WHAT:
    // CreatedAtUtc records when the backend created the account.
    //
    // WHY:
    // Timestamps are backend-owned facts. The browser can suggest times, but
    // the server should create important audit values itself.
    public DateTimeOffset CreatedAtUtc { get; set; }
}
