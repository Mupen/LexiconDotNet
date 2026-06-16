import { useMemo, useState } from 'react'

const dayFormatter = new Intl.DateTimeFormat('en-DK', { day: 'numeric' })
const monthFormatter = new Intl.DateTimeFormat('en-DK', { month: 'long', year: 'numeric' })
const weekdayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

function startOfMonth(date) {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

function addMonths(date, months) {
  return new Date(date.getFullYear(), date.getMonth() + months, 1)
}

function getCalendarDays(month) {
  const firstDay = startOfMonth(month)
  const mondayBasedDay = (firstDay.getDay() + 6) % 7
  const firstCalendarDay = new Date(firstDay)
  firstCalendarDay.setDate(firstDay.getDate() - mondayBasedDay)

  return Array.from({ length: 42 }, (_, index) => {
    const day = new Date(firstCalendarDay)
    day.setDate(firstCalendarDay.getDate() + index)
    return day
  })
}

function getDayStatus(day, bookings) {
  const dayStart = new Date(day.getFullYear(), day.getMonth(), day.getDate())
  const dayEnd = new Date(dayStart)
  dayEnd.setDate(dayStart.getDate() + 1)

  const bookingsForDay = bookings.filter((booking) => {
    const checkIn = new Date(booking.checkIn)
    const checkOut = new Date(booking.checkOut)
    return checkIn < dayEnd && dayStart < checkOut
  })

  if (bookingsForDay.some((booking) => booking.status === 'Active')) {
    return 'booked'
  }

  return 'free'
}

export function BookingCalendar({ bookings }) {
  const [visibleMonth, setVisibleMonth] = useState(() => startOfMonth(new Date()))

  const days = useMemo(() => getCalendarDays(visibleMonth), [visibleMonth])

  return (
    <section className="room-calendar" aria-label="Room booking calendar">
      <div className="calendar-header">
        <button type="button" className="secondary calendar-nav" onClick={() => setVisibleMonth(addMonths(visibleMonth, -1))}>
          Prev
        </button>
        <strong>{monthFormatter.format(visibleMonth)}</strong>
        <button type="button" className="secondary calendar-nav" onClick={() => setVisibleMonth(addMonths(visibleMonth, 1))}>
          Next
        </button>
      </div>

      <div className="calendar-legend">
        <span><i className="legend-dot free" />Free</span>
        <span><i className="legend-dot booked" />Booked</span>
      </div>

      <div className="calendar-grid">
        {weekdayLabels.map((label) => (
          <span key={label} className="calendar-weekday">{label}</span>
        ))}

        {days.map((day) => {
          const status = getDayStatus(day, bookings)
          const isOutsideMonth = day.getMonth() !== visibleMonth.getMonth()

          return (
            <span
              key={day.toISOString()}
              className={`calendar-day ${status} ${isOutsideMonth ? 'outside-month' : ''}`}
              title={`${day.toLocaleDateString('en-DK')} - ${status}`}
            >
              {dayFormatter.format(day)}
            </span>
          )
        })}
      </div>
    </section>
  )
}
