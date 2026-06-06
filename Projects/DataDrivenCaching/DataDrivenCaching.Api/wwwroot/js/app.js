import { recordLearningEvent } from "./events.js";
import { refreshSession } from "./session.js";
import { applySettings, loadSettingsFromLocalStorage } from "./settings.js";
import { render } from "./render.js";

initialize();

// Starts the app in the correct data order: browser convenience settings first,
// trusted backend session second, and rendering last. Keeping this file as the
// entry point makes it clear that app.js only coordinates startup.
async function initialize() {
  loadSettingsFromLocalStorage();
  applySettings();
  await refreshSession();
  recordLearningEvent({
    area: "Application",
    action: "Loaded page",
    storage: "JavaScript memory",
    authority: "Temporary frontend state",
    explanation: "The dashboard state exists in browser RAM and will be rebuilt after refresh."
  });
  render();
}
