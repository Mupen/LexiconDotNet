function formatDate(value) {
  return new Intl.DateTimeFormat('en-DK', {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(value))
}

export function BookingList({ bookings, selectedBookingId, onSelect, onCancel }) {
  return (
    <section className="panel booking-list">
      <div className="panel-header">
        <h2>Bookings</h2>
        <span>{bookings.length}</span>
      </div>

      <div className="booking-items">
        {bookings.length === 0 && <p className="empty">No bookings yet.</p>}

        {bookings.map((booking) => (
          <article
            key={booking.id}
            className={`booking-item ${selectedBookingId === booking.id ? 'selected' : ''} ${booking.status === 'Cancelled' ? 'cancelled' : ''}`}
          >
            <button type="button" className="booking-main" onClick={() => onSelect(booking)}>
              <strong>{booking.guestName}</strong>
              <span>{booking.roomName}</span>
              <span>{booking.parkingSpaceName ?? 'No parking'}</span>
              <span>{formatDate(booking.checkIn)} - {formatDate(booking.checkOut)}</span>
              <span>{booking.totalPrice} DKK</span>
            </button>
            <button
              type="button"
              className="danger"
              disabled={booking.status === 'Cancelled'}
              onClick={() => onCancel(booking.id)}
            >
              Cancel
            </button>
          </article>
        ))}
      </div>
    </section>
  )
}
