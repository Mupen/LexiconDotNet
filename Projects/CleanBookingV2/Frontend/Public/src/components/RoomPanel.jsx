import { BookingCalendar } from './BookingCalendar.jsx'

export function RoomPanel({ rooms, bookings, selectedRoomId, isFiltered, onSelectRoom, onBookRoom }) {
  const selectedRoom = rooms.find((room) => room.id === selectedRoomId) ?? null
  const selectedRoomBookings = selectedRoom
    ? bookings.filter((booking) => booking.roomId === selectedRoom.id)
    : []

  return (
    <section className="panel room-panel">
      <div className="panel-header">
        <h2>{isFiltered ? 'Available Rooms' : 'Rooms'}</h2>
      </div>

      <div className="room-grid">
        {rooms.map((room) => (
          <button
            key={room.id}
            type="button"
            className={`room-card ${selectedRoomId === room.id ? 'selected' : ''}`}
            onClick={() => onSelectRoom(room.id)}
          >
            <strong>{room.name}</strong>
            <span>{room.roomType}</span>
            <span>{room.sizeInSquareMeters} m2</span>
            <span>{room.capacity} guest(s)</span>
            <span>{room.pricePerNight} DKK / day</span>
          </button>
        ))}
      </div>

      {rooms.length === 0 && (
        <p className="empty">No rooms available for the selected dates.</p>
      )}

      {selectedRoom && (
        <div className="room-detail">
          <img
            src={`/rooms/room-${selectedRoom.id}.png`}
            alt={`${selectedRoom.name} interior`}
            className="room-photo"
          />
          <div className="room-detail-copy">
            <h3>{selectedRoom.name}</h3>
            <p>{selectedRoom.roomType} room for {selectedRoom.capacity} guest(s)</p>
          </div>
          <BookingCalendar bookings={selectedRoomBookings} />
          {onBookRoom && (
            <button type="button" onClick={() => onBookRoom(selectedRoom.id)}>
              Book this room
            </button>
          )}
        </div>
      )}
    </section>
  )
}
