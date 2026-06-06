namespace DataDrivenCaching.Domain.Entities;

// WHAT:
// LabDataRecord is a small durable data item used by the storage demos.
//
// WHY:
// The project needs backend-owned application data that can be compared with
// temporary frontend state, browser storage, HTTP cache responses, and backend
// memory cache copies.
//
// DATA DESIGN:
// This record is intended to be source-of-truth data. Cached versions may exist
// elsewhere, but this database row is the authoritative copy.
public sealed class LabDataRecord
{
    public int Id { get; set; }

    // WHAT:
    // Name gives the record a human-readable identity in the demo UI.
    //
    // WHY:
    // A simple name makes it easy to show when frontend copies are stale after
    // the authoritative database value changes.
    public required string Name { get; set; }

    // WHAT:
    // Value is intentionally simple text.
    //
    // WHY:
    // The lesson here is not complex modeling. The lesson is how one durable
    // backend value can move through caches and browser storage.
    public required string Value { get; set; }

    // WHAT:
    // UpdatedAtUtc records the last backend-side modification.
    //
    // WHY:
    // Cache demos need a visible way to show stale versus current data.
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
