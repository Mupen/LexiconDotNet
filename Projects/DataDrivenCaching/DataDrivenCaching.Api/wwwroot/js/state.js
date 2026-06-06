// ============================================================================
// Data-Driven App State
// ----------------------------------------------------------------------------
// This module owns shared frontend state and constants. Other modules import
// this data instead of creating their own copies, which keeps the app data-driven
// while making each feature file responsible for only one area of behavior.
// ============================================================================

export const defaultSettings = {
  themeName: "daylight",
  density: "comfortable",
  accentColor: "green",
  showLearningDashboard: true
};

export const themes = {
  daylight: {
    label: "Daylight",
    dataTheme: "daylight"
  },
  midnight: {
    label: "Midnight",
    dataTheme: "midnight"
  }
};

export const learningState = {
  session: {
    isLoggedIn: false,
    userName: null,
    displayName: null,
    explanation: "Session has not been checked yet."
  },
  settings: {
    ...defaultSettings,
    source: "predefined defaults",
    savedToLocalStorage: false,
    importedFromJson: false
  },
  events: []
};

export const settingsStorageKey = "DataDrivenCaching.Settings";
