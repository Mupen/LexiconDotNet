import { recordLearningEvent } from "./events.js";
import { defaultSettings, learningState, settingsStorageKey, themes } from "./state.js";

// Attempts to hydrate theme settings from localStorage and validates them before use.
// localStorage is used for user convenience, but it is editable by the user, so invalid
// or stale values are ignored instead of being treated as trusted application state.
export function loadSettingsFromLocalStorage() {
  const stored = localStorage.getItem(settingsStorageKey);

  if (!stored) {
    return;
  }

  try {
    const parsed = JSON.parse(stored);
    learningState.settings = {
      ...defaultSettings,
      ...validateSettings(parsed),
      source: "localStorage",
      savedToLocalStorage: true,
      importedFromJson: false
    };
  } catch {
    localStorage.removeItem(settingsStorageKey);
  }
}

// Persists the safe subset of theme settings to localStorage and records why that is acceptable.
// Only non-security preferences are saved because browser storage is convenient for UI choices
// but should not be used as authority for identity or permissions.
export function saveSettingsToLocalStorage() {
  localStorage.setItem(settingsStorageKey, JSON.stringify(getPersistableSettings(), null, 2));
  learningState.settings.savedToLocalStorage = true;
  learningState.settings.source = "localStorage";

  recordLearningEvent({
    area: "Theme Settings",
    action: "Saved settings",
    storage: "localStorage",
    authority: "Client convenience data",
    explanation: "Theme settings belong to the user and are safe to store in editable browser storage."
  });
}

// Updates one setting in memory, applies it immediately, and records the temporary state change.
// The change is intentionally temporary until saved so the app can demonstrate the difference
// between live JavaScript state and persisted browser storage.
export function updateSetting(name, value) {
  learningState.settings[name] = value;
  learningState.settings.source = "JavaScript memory";
  applySettings();

  recordLearningEvent({
    area: "Theme Settings",
    action: `Changed ${name}`,
    storage: "JavaScript memory",
    authority: "Temporary frontend state",
    explanation: "The setting changed on screen first. It becomes persistent only when saved to localStorage."
  });
}

// Removes saved browser settings and restores the predefined defaults.
// This gives the app a known clean state and shows that defaults come from application code,
// while localStorage is only an optional user-owned copy.
export function resetSettings() {
  localStorage.removeItem(settingsStorageKey);
  learningState.settings = {
    ...defaultSettings,
    source: "predefined defaults",
    savedToLocalStorage: false,
    importedFromJson: false
  };
  applySettings();

  recordLearningEvent({
    area: "Theme Settings",
    action: "Reset settings",
    storage: "predefined defaults",
    authority: "Application-provided defaults",
    explanation: "The browser copy was removed. The UI returned to the default data object."
  });
}

// Parses settings from an uploaded JSON file, validates them, and applies the accepted values.
// Imported files are treated as untrusted user input, so the app keeps the data-driven workflow
// while still allowing only known themes, densities, colors, and boolean flags.
export function importSettings(rawJson) {
  const parsed = JSON.parse(rawJson);
  learningState.settings = {
    ...defaultSettings,
    ...validateSettings(parsed),
    source: "imported JSON",
    savedToLocalStorage: false,
    importedFromJson: true
  };
  applySettings();

  recordLearningEvent({
    area: "Theme Settings",
    action: "Imported JSON",
    storage: "uploaded JSON file",
    authority: "Untrusted user input",
    explanation: "Imported settings are user-editable data, so the app validates allowed values before applying them."
  });
}

// Returns only the settings that should be exported or saved.
// Derived metadata such as source and dashboard flags about storage are excluded so persisted data
// stays focused on user preferences instead of internal explanation state.
export function getPersistableSettings() {
  return {
    themeName: learningState.settings.themeName,
    density: learningState.settings.density,
    accentColor: learningState.settings.accentColor,
    showLearningDashboard: learningState.settings.showLearningDashboard
  };
}

// Normalizes incoming settings against the allowed option lists and default values.
// Central validation lets localStorage and JSON import share the same safety rules instead of
// duplicating trust decisions in each caller.
export function validateSettings(settings) {
  return {
    themeName: themes[settings.themeName] ? settings.themeName : defaultSettings.themeName,
    density: ["comfortable", "compact"].includes(settings.density) ? settings.density : defaultSettings.density,
    accentColor: ["green", "blue", "red"].includes(settings.accentColor) ? settings.accentColor : defaultSettings.accentColor,
    showLearningDashboard: typeof settings.showLearningDashboard === "boolean"
      ? settings.showLearningDashboard
      : defaultSettings.showLearningDashboard
  };
}

// Applies the current theme and density to document-level data attributes.
// CSS can then react to state declaratively through selectors, which keeps visual changes out
// of the rendering code and avoids manually styling many individual elements.
export function applySettings() {
  const theme = themes[learningState.settings.themeName] ?? themes.daylight;
  document.documentElement.dataset.theme = theme.dataTheme;
  document.documentElement.dataset.density = learningState.settings.density;
}

// Creates the current settings JSON as a downloadable file and records the action.
// Creating a Blob URL lets the browser download generated data without sending user preferences
// back to the server, reinforcing that these settings are client-owned convenience data.
export function downloadSettingsJson() {
  const json = JSON.stringify(getPersistableSettings(), null, 2);
  const blob = new Blob([json], { type: "application/json" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = "datadrivencaching-settings.json";
  link.click();
  URL.revokeObjectURL(url);

  recordLearningEvent({
    area: "Theme Settings",
    action: "Downloaded JSON",
    storage: "downloaded file",
    authority: "User-owned copy",
    explanation: "The exported file is portable settings data, but it is still editable by the user."
  });
}
