export function ActivityPanel({ activities }) {
  return (
    <section className="panel activity-panel">
      <div className="panel-header">
        <h2>Recent activity</h2>
        <span>{activities.length}</span>
      </div>

      <div className="activity-items">
        {activities.length === 0 && <p className="empty">No activity yet.</p>}

        {activities.map((activity) => (
          <article key={activity.id} className={`activity-item ${activity.type}`}>
            <strong>{activity.text}</strong>
            <span>{activity.happenedAt}</span>
          </article>
        ))}
      </div>
    </section>
  )
}
