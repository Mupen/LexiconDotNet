const storageKey = 'CleanBookingV2.Preferences'

export const defaultPreferences = {
  density: 'comfortable',
  hideCancelledBookings: false
}

export function loadPreferences() {
  const stored = localStorage.getItem(storageKey)

  if (!stored) {
    return defaultPreferences
  }

  try {
    return validatePreferences(JSON.parse(stored))
  } catch {
    localStorage.removeItem(storageKey)
    return defaultPreferences
  }
}

export function savePreferences(preferences) {
  const validated = validatePreferences(preferences)
  localStorage.setItem(storageKey, JSON.stringify(validated))
  return validated
}

function validatePreferences(preferences) {
  return {
    density: preferences?.density === 'compact' ? 'compact' : defaultPreferences.density,
    hideCancelledBookings: typeof preferences?.hideCancelledBookings === 'boolean'
      ? preferences.hideCancelledBookings
      : defaultPreferences.hideCancelledBookings
  }
}
