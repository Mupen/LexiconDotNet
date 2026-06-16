import { useState } from 'react'

function timeToMinutes(time) {
  const [hours, minutes] = time.split(':').map(Number)
  return hours * 60 + minutes
}

function buildTimeOptions(startTime, endTime, stepMinutes) {
  const options = []
  const start = timeToMinutes(startTime)
  const end = timeToMinutes(endTime)

  for (let minutes = start; minutes <= end; minutes += stepMinutes) {
    const hour = Math.floor(minutes / 60).toString().padStart(2, '0')
    const minute = (minutes % 60).toString().padStart(2, '0')
    options.push(`${hour}:${minute}`)
  }

  return options
}

function ChoicePicker({ value, options, placeholder, disabled, onChange }) {
  const [isOpen, setIsOpen] = useState(false)
  const selectedOption = options.find((option) => option.value === value)

  function choose(nextValue) {
    onChange(nextValue)
    setIsOpen(false)
  }

  return (
    <div className="choice-picker">
      <button
        type="button"
        className="choice-trigger"
        disabled={disabled}
        onClick={() => setIsOpen((current) => !current)}
      >
        {selectedOption?.displayName ?? placeholder}
      </button>
      {isOpen && !disabled && (
        <div className="choice-list">
          {options.map((option) => (
            <button
              key={option.value}
              type="button"
              className={`choice-option ${option.value === value ? 'selected' : ''}`}
              onClick={() => choose(option.value)}
            >
              {option.listName}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

export function BookingForm({
  form,
  rooms,
  parkingSpaces,
  availableParkingSpaces,
  availableRooms,
  hasCheckedAvailability,
  bookingPolicy,
  selectedBooking,
  onChange,
  onSubmit,
  onSearchAvailability,
  onReset
}) {
  function updateField(field, value) {
    onChange({ ...form, [field]: value })
  }

  const checkInTimeOptions = buildTimeOptions(
    bookingPolicy.earliestCheckIn,
    bookingPolicy.latestCheckIn,
    bookingPolicy.timeSlotMinutes)
  const checkOutTimeOptions = buildTimeOptions(
    '00:00',
    bookingPolicy.latestCheckOut,
    bookingPolicy.timeSlotMinutes)
  const roomOptions = selectedBooking && !hasCheckedAvailability
    ? rooms
    : hasCheckedAvailability
      ? availableRooms
      : []
  const roomPlaceholder = hasCheckedAvailability || selectedBooking
    ? 'Choose room'
    : 'Check availability'
  const parkingOptions = selectedBooking && !hasCheckedAvailability
    ? parkingSpaces
    : hasCheckedAvailability
      ? availableParkingSpaces
      : []
  const roomChoices = roomOptions.map((room) => ({
    value: String(room.id),
    displayName: room.name,
    listName: `${room.name} - ${room.capacity} guest(s) - ${room.pricePerNight} DKK`
  }))
  const parkingChoices = [
    {
      value: '',
      displayName: 'No parking',
      listName: 'No parking'
    },
    ...parkingOptions
      .filter((space) => space.isActive)
      .map((space) => ({
        value: String(space.id),
        displayName: space.name,
        listName: `${space.name} - ${space.parkingSpaceType}`
      }))
  ]

  return (
    <form className="panel booking-form" onSubmit={onSubmit}>
      <div className="panel-header">
        <h2>{selectedBooking ? 'Edit Booking' : 'New Booking'}</h2>
        <button type="button" className="secondary" onClick={onReset}>
          Clear
        </button>
      </div>

      <label>
        Guest name
        <input
          value={form.guestName}
          onChange={(event) => updateField('guestName', event.target.value)}
          required
        />
      </label>

      <div className="two-columns">
        <label>
          <span title={`Choose the arrival date. Check-in time must be between ${bookingPolicy.earliestCheckIn} and ${bookingPolicy.latestCheckIn}. Arrivals after ${bookingPolicy.lateArrivalThreshold} need an estimated arrival time.`}>
            Check-in date
          </span>
          <input
            type="date"
            value={form.checkInDate}
            onChange={(event) => updateField('checkInDate', event.target.value)}
            required
          />
        </label>
        <label>
          <span title={`Check-in is allowed from ${bookingPolicy.earliestCheckIn} to ${bookingPolicy.latestCheckIn}. If the check-in time is after ${bookingPolicy.lateArrivalThreshold}, add an estimated arrival time.`}>
            Check-in time
          </span>
          <select
            value={form.checkInTime}
            onChange={(event) => updateField('checkInTime', event.target.value)}
            required
          >
            {checkInTimeOptions.map((time) => (
              <option key={time} value={time}>{time}</option>
            ))}
          </select>
        </label>
      </div>

      <div className="two-columns">
        <label>
          <span title="Choose the departure date. It must be after check-in.">
            Check-out date
          </span>
          <input
            type="date"
            value={form.checkOutDate}
            onChange={(event) => updateField('checkOutDate', event.target.value)}
            required
          />
        </label>
        <label>
          <span title={`Check-out must be no later than ${bookingPolicy.latestCheckOut}.`}>
            Check-out time
          </span>
          <select
            value={form.checkOutTime}
            onChange={(event) => updateField('checkOutTime', event.target.value)}
            required
          >
            {checkOutTimeOptions.map((time) => (
              <option key={time} value={time}>{time}</option>
            ))}
          </select>
        </label>
      </div>

      <div className="two-columns">
        <div className="field-stack">
          <label>
            Guests
            <input
              type="number"
              min="1"
              max={bookingPolicy.maximumGuests}
              value={form.guests}
              onChange={(event) => updateField('guests', event.target.value)}
              required
            />
          </label>
          <label>
            <span title={`Only required when check-in is after ${bookingPolicy.lateArrivalThreshold}.`}>
              Estimated arrival time
            </span>
            <input
              type="time"
              value={form.estimatedArrivalTime}
              onChange={(event) => updateField('estimatedArrivalTime', event.target.value)}
            />
          </label>
        </div>
        <div className="field-stack">
          <label>
            Room
            <ChoicePicker
              value={form.roomId}
              options={roomChoices}
              placeholder={roomPlaceholder}
              disabled={!hasCheckedAvailability && !selectedBooking}
              onChange={(value) => updateField('roomId', value)}
            />
          </label>
          <label>
            Parking
            <ChoicePicker
              value={form.parkingSpaceId}
              options={parkingChoices}
              placeholder="No parking"
              disabled={!hasCheckedAvailability && !selectedBooking}
              onChange={(value) => updateField('parkingSpaceId', value)}
            />
          </label>
        </div>
      </div>

      <div className="form-actions">
        <button type="button" className="secondary" onClick={onSearchAvailability}>
          Check availability
        </button>
        <button type="submit" disabled={!selectedBooking && !hasCheckedAvailability}>
          {selectedBooking ? 'Update booking' : 'Create booking'}
        </button>
      </div>
    </form>
  )
}
