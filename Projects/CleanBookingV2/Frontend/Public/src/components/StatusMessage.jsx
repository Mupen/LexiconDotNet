export function StatusMessage({ status }) {
  return (
    <div className={`status ${status.type}`} role="status">
      {status.text}
    </div>
  )
}
