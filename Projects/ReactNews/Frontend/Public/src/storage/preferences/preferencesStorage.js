/*
 * What: storageKey is the localStorage key used for ReactNews display
 * preferences.
 * How: loadPreferences and savePreferences both use the same string key.
 * Why: a named constant prevents typo bugs where saving and loading use
 * different keys.
 */
const storageKey = 'reactnews.preferences'

/*
 * What: defaultPreferences defines safe browser-only UI defaults.
 * How: loaded preferences are merged over this object.
 * Why: adding a new preference later should not break users who already have an
 * older localStorage object without that field.
 */
export const defaultPreferences = {
  theme: 'light',
  fontScale: 1,
  compactCards: false,
  preferredCategories: ['technology', 'general']
}

/*
 * What: loadPreferences reads display preferences from localStorage.
 * How: it parses the stored JSON and merges it with defaults; if anything goes
 * wrong, it returns defaults.
 * Why: localStorage can be missing, manually edited, or invalid JSON. UI
 * preferences are convenience data, so the app should recover gracefully instead
 * of failing to render.
 */
export function loadPreferences() {
  try {
    const stored = localStorage.getItem(storageKey)
    return stored ? { ...defaultPreferences, ...JSON.parse(stored) } : defaultPreferences
  } catch {
    return defaultPreferences
  }
}

/*
 * What: savePreferences writes display preferences to localStorage.
 * How: the preferences object is serialized as JSON under storageKey.
 * Why: display preferences are harmless client-owned data. They should persist
 * across refreshes without requiring login or backend storage.
 */
export function savePreferences(preferences) {
  localStorage.setItem(storageKey, JSON.stringify(preferences))
}
