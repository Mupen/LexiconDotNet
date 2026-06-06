import { recordLearningEvent } from "./events.js";
import { learningState } from "./state.js";

// Reads the current session from the API and copies only the session summary into frontend state.
// The browser asks the backend because login state is security-sensitive and should not be trusted
// just because JavaScript memory or localStorage says a user is logged in.
export async function refreshSession() {
  const response = await fetch("/api/session");
  const session = await response.json();

  setSessionState(session);
}

// Sends submitted credentials to the backend and updates the UI based on whether the server accepts them.
// The password is posted to the API instead of being checked in JavaScript because SQLite and the backend
// own account verification, while the browser only receives the resulting session status.
export async function login(formData) {
  const response = await fetch("/api/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      userName: formData.get("userName"),
      password: formData.get("password")
    })
  });

  if (!response.ok) {
    recordLearningEvent({
      area: "Login",
      action: "Rejected login",
      storage: "SQLite + backend session",
      authority: "Backend authority",
      explanation: "The backend did not verify the username/password pair, so no session was created."
    });
    return;
  }

  const session = await response.json();
  setSessionState(session);

  recordLearningEvent({
    area: "Login",
    action: "Created session",
    storage: "SQLite + ASP.NET Session + HttpOnly cookie",
    authority: "Backend authority",
    explanation: "SQLite verified the password hash. The browser received only a session cookie, not account data."
  });
}

// Ends the backend session and then stores the logged-out state returned by the API.
// Logging out goes through the server because the trusted session lives in ASP.NET Session,
// not in editable browser state.
export async function logout() {
  const response = await fetch("/api/logout", { method: "POST" });
  const session = await response.json();

  setSessionState(session);

  recordLearningEvent({
    area: "Login",
    action: "Cleared session",
    storage: "ASP.NET Session",
    authority: "Backend authority",
    explanation: "The trusted login state was removed from the backend session."
  });
}

// Copies the API session shape into the frontend state shape.
// Keeping this mapping in one place prevents login, logout, and refresh from drifting apart.
function setSessionState(session) {
  learningState.session = {
    isLoggedIn: session.isLoggedIn,
    userName: session.userName,
    displayName: session.displayName,
    explanation: session.explanation
  };
}
