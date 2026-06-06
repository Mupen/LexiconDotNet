// Creates an element with optional class and text content.
// This tiny helper removes repeated DOM boilerplate while still using real DOM APIs, which keeps
// rendering explicit and avoids unsafe HTML string concatenation.
export function element(tagName, className = null, text = null) {
  const node = document.createElement(tagName);

  if (className) {
    node.className = className;
  }

  if (text !== null && text !== undefined) {
    node.textContent = text;
  }

  return node;
}

// Creates a standard non-submit button and connects its click handler.
// Setting type to button prevents helper buttons inside forms from accidentally submitting the form,
// which keeps each action tied to its explicit handler.
export function createButton(text, className, onClick) {
  const button = element("button", className, text);
  button.type = "button";
  button.addEventListener("click", onClick);
  return button;
}
