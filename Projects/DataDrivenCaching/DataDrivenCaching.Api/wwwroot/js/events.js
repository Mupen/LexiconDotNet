import { learningState } from "./state.js";

// Adds a learning event to the front of the dashboard history and keeps only the newest items.
// Limiting the list keeps the UI readable while still showing the recent data movement that
// explains what storage layer or authority each action used.
export function recordLearningEvent(event) {
  learningState.events.unshift({
    ...event,
    happenedAt: new Date().toLocaleTimeString()
  });

  learningState.events = learningState.events.slice(0, 8);
}
