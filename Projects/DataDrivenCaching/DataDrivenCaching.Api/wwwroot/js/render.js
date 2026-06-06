import { createButton, element } from "./dom.js";
import { recordLearningEvent } from "./events.js";
import { login, logout, refreshSession } from "./session.js";
import {
  applySettings,
  downloadSettingsJson,
  getPersistableSettings,
  importSettings,
  resetSettings,
  saveSettingsToLocalStorage,
  updateSetting
} from "./settings.js";
import { learningState } from "./state.js";

const appRoot = document.querySelector("#app");

// Rebuilds the visible application from the current state object.
// Rendering from state keeps the page predictable: each action changes data first, then the DOM
// is recreated to match that data instead of being patched in many separate places.
export function render() {
  applySettings();

  appRoot.replaceChildren(
    createHero(),
    createWorkspace()
  );
}

// Creates the top page section that names the lab and frames the purpose of the screen.
// Keeping it as a function makes the render tree explicit and keeps static page structure
// separate from panels that depend heavily on changing state.
function createHero() {
  const hero = element("section", "hero");
  hero.append(
    element("h1", null, "DataDrivenCaching"),
    element("p", null, "A data-driven storage and caching lab. Use the site and watch the dashboard explain which data is authoritative, temporary, cached, trusted, or user-editable.")
  );
  return hero;
}

// Builds the main workspace containing the forms and, when enabled, the learning dashboard.
// The dashboard is included conditionally from settings so the UI demonstrates how one state flag
// can control an entire section of the rendered page.
function createWorkspace() {
  const workspace = element("div", "workspace");
  const panels = element("div", "panel-grid");

  panels.append(
    createLoginPanel(),
    createThemePanel()
  );

  workspace.append(panels);

  if (learningState.settings.showLearningDashboard) {
    workspace.append(createDashboard());
  }

  return workspace;
}

// Builds the login form and wires its actions to the session API helpers.
// The panel keeps credential entry, session refresh, and logout together because all three actions
// demonstrate backend-owned session state rather than browser-owned settings.
function createLoginPanel() {
  const panel = element("section", "panel");
  const form = element("form", "form-grid");

  form.append(
    createLabeledInput("Username", "userName", "alice"),
    createLabeledInput("Password", "password", "Password123!", "password")
  );

  const actions = element("div", "actions full");
  actions.append(
    element("button", null, "Login"),
    createButton("Refresh session", "secondary", refreshSessionAndRender),
    createButton("Logout", "danger", async () => {
      await logout();
      render();
    })
  );

  form.append(actions);
  form.addEventListener("submit", async event => {
    event.preventDefault();
    await login(new FormData(form));
    render();
  });

  panel.append(
    createPanelHeader("Login And Session", "Accounts live in SQLite. Successful login creates backend session state connected to the browser by an HttpOnly cookie."),
    form
  );

  return panel;
}

// Builds the theme settings panel, including live controls, persistence actions, and a JSON preview.
// The controls update state through shared helpers so localStorage, import, reset, and export all
// operate on the same data shape.
function createThemePanel() {
  const panel = element("section", "panel");
  const form = element("div", "form-grid");

  form.append(
    createSelect("Theme", "themeName", learningState.settings.themeName, [
      ["daylight", "Daylight"],
      ["midnight", "Midnight"]
    ]),
    createSelect("Density", "density", learningState.settings.density, [
      ["comfortable", "Comfortable"],
      ["compact", "Compact"]
    ])
  );

  const actions = element("div", "actions full");
  actions.append(
    createButton("Save to localStorage", null, () => {
      saveSettingsToLocalStorage();
      render();
    }),
    createButton("Download JSON", "secondary", () => {
      downloadSettingsJson();
      render();
    }),
    createUploadButton(),
    createButton("Reset", "danger", () => {
      resetSettings();
      render();
    })
  );

  form.append(actions);

  const json = element("pre", "json-box", JSON.stringify(getPersistableSettings(), null, 2));

  panel.append(
    createPanelHeader("Theme Settings", "Theme data is user-owned convenience data. It can safely live in localStorage or an imported JSON file because it is not a security decision."),
    form,
    json
  );

  return panel;
}

// Builds the learning dashboard from current session, settings, and event state.
// It is rendered from the same state as the rest of the app so the explanation panel reflects
// actual data movement instead of separate hard-coded documentation.
function createDashboard() {
  const dashboard = element("aside", "dashboard");

  const statusList = element("ul", "status-list");
  statusList.append(
    createStatusItem("Current user", learningState.session.isLoggedIn ? `${learningState.session.displayName} (${learningState.session.userName})` : "Not logged in", learningState.session.explanation),
    createStatusItem("Theme source", learningState.settings.source, "Settings are client convenience data unless saved to the backend later."),
    createStatusItem("localStorage", learningState.settings.savedToLocalStorage ? "Saved" : "Not saved", "Users can edit localStorage manually, so it is never security authority.")
  );

  const events = element("ul", "event-list");
  for (const event of learningState.events) {
    events.append(createEventItem(event));
  }

  dashboard.append(
    createPanelHeader("Learning Dashboard", "This panel is rendered from explicit state so each action can explain where data moved and why."),
    statusList,
    element("h3", null, "Recent Events"),
    events
  );

  return dashboard;
}

// Creates a reusable panel header with a title and explanatory description.
// Centralizing this markup keeps panels visually consistent while allowing each panel to provide
// its own data-storage explanation.
function createPanelHeader(title, description) {
  const header = element("header");
  header.append(
    element("h2", null, title),
    element("p", null, description)
  );
  return header;
}

// Creates one status row for the dashboard.
// The row separates the label, current value, and detail text so the dashboard can scan well
// while still explaining why each value should or should not be trusted.
function createStatusItem(label, value, detail) {
  const item = element("li", "status-item");
  item.append(
    element("strong", null, label),
    document.createTextNode(value),
    element("span", null, detail)
  );
  return item;
}

// Creates one recent-event row for the dashboard history.
// Events are formatted in one place so every action uses the same structure for time, area,
// storage layer, authority, and explanation.
function createEventItem(event) {
  const item = element("li", "event-item");
  item.append(
    element("strong", null, `${event.happenedAt} - ${event.action}`),
    document.createTextNode(`${event.area}: ${event.storage}`),
    element("span", null, `${event.authority}. ${event.explanation}`)
  );
  return item;
}

// Creates a labeled input with a default value and optional input type.
// Wrapping the input in its label keeps the markup compact and accessible while giving form
// builders a simple helper for repeated fields.
function createLabeledInput(label, name, value, type = "text") {
  const wrapper = element("label");
  wrapper.append(element("span", "meta", label));

  const input = element("input");
  input.name = name;
  input.type = type;
  input.value = value;
  wrapper.append(input);

  return wrapper;
}

// Creates a labeled select control from an array of allowed options.
// The caller supplies the valid choices, and changes flow through updateSetting so UI controls
// cannot bypass the same state and dashboard update path.
function createSelect(label, name, value, options) {
  const wrapper = element("label");
  wrapper.append(element("span", "meta", label));

  const select = element("select");
  select.name = name;

  for (const [optionValue, text] of options) {
    const option = element("option", null, text);
    option.value = optionValue;
    option.selected = optionValue === value;
    select.append(option);
  }

  select.addEventListener("change", () => {
    updateSetting(name, select.value);
    render();
  });
  wrapper.append(select);

  return wrapper;
}

// Creates the hidden file input and visible button used to import JSON settings.
// The real file input stays hidden so the panel can use the same button styling as other actions,
// while the browser still handles local file selection securely.
function createUploadButton() {
  const label = element("label", "button-label");
  const input = element("input");
  input.type = "file";
  input.accept = "application/json";
  input.hidden = true;

  label.append(input, createButton("Load JSON", "secondary", () => input.click()));
  input.addEventListener("change", async () => {
    const file = input.files[0];
    if (!file) {
      return;
    }

    importSettings(await file.text());
    input.value = "";
    render();
  });

  return label;
}

// Refreshes the backend session, records that the check happened, and redraws the UI.
// This wraps refreshSession for button usage so the lower-level API read stays reusable while
// the user action still appears in the learning dashboard.
async function refreshSessionAndRender() {
  await refreshSession();
  recordLearningEvent({
    area: "Login",
    action: "Checked session",
    storage: "ASP.NET Session",
    authority: "Backend authority",
    explanation: "The frontend asked the backend to describe the trusted session state."
  });
  render();
}
