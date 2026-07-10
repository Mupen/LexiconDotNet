/*
 * What: StatusMessage renders the current loading/error/success text.
 * How: the caller passes a status object with type and text; type becomes a CSS
 * class and text becomes visible content.
 * Why: centralizing status markup keeps pages/components from each inventing
 * their own status styles and accessibility role.
 */
export function StatusMessage({ status }) {
  return (
    <div className={`status ${status.type}`} role="status">
      {status.text}
    </div>
  )
}
