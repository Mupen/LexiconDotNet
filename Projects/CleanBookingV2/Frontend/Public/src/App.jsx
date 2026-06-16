import { useCallback, useEffect, useMemo, useState } from 'react'
import { bookingApi } from './api/bookingsApi.js'
import { parkingSpacesApi } from './api/parkingSpacesApi.js'
import { roomsApi } from './api/roomsApi.js'
import { ActivityPanel } from './components/ActivityPanel.jsx'
import { BookingForm } from './components/BookingForm.jsx'
import { BookingList } from './components/BookingList.jsx'
import { RoomPanel } from './components/RoomPanel.jsx'
import { StatusMessage } from './components/StatusMessage.jsx'
import { useDashboardData } from './hooks/useDashboardData.js'
import { loadPreferences, savePreferences } from './storage/preferencesStorage.js'

const emptyForm = {
  guestName: '',
  checkInDate: '',
  checkInTime: '14:00',
  checkOutDate: '',
  checkOutTime: '12:00',
  guests: 1,
  roomId: '',
  parkingSpaceId: '',
  estimatedArrivalTime: ''
}

const fallbackBookingPolicy = {
  earliestCheckIn: '14:00',
  latestCheckIn: '22:30',
  latestCheckOut: '12:00',
  lateArrivalThreshold: '20:00',
  timeSlotMinutes: 30,
  maximumGuests: 3
}

let nextActivityId = 1

const pageRoutes = {
  home: '/Home',
  stays: '/Stays',
  booking: '/Booking'
}

function getPageFromPath() {
  if (typeof window === 'undefined') {
    return 'home'
  }

  const path = window.location.pathname.toLowerCase()

  if (path === '/stays') {
    return 'stays'
  }

  if (path === '/booking') {
    return 'booking'
  }

  return 'home'
}

function timeToMinutes(time) {
  const [hours, minutes] = time.split(':').map(Number)
  return hours * 60 + minutes
}

function minutesToTime(totalMinutes) {
  const hours = Math.floor(totalMinutes / 60).toString().padStart(2, '0')
  const minutes = (totalMinutes % 60).toString().padStart(2, '0')
  return `${hours}:${minutes}`
}

function buildTimeOptions(startTime, endTime, stepMinutes) {
  const options = []
  const start = timeToMinutes(startTime)
  const end = timeToMinutes(endTime)

  for (let minutes = start; minutes <= end; minutes += stepMinutes) {
    options.push(minutesToTime(minutes))
  }

  return options
}

function toRequest(form, policy) {
  const checkInTimes = new Set(buildTimeOptions(policy.earliestCheckIn, policy.latestCheckIn, policy.timeSlotMinutes))
  const checkOutTimes = new Set(buildTimeOptions('00:00', policy.latestCheckOut, policy.timeSlotMinutes))
  const checkInTime = checkInTimes.has(form.checkInTime) ? form.checkInTime : policy.earliestCheckIn
  const checkOutTime = checkOutTimes.has(form.checkOutTime) ? form.checkOutTime : emptyForm.checkOutTime

  return {
    guestName: form.guestName,
    checkIn: `${form.checkInDate}T${checkInTime}:00`,
    checkOut: `${form.checkOutDate}T${checkOutTime}:00`,
    numberOfGuests: Number(form.guests),
    roomId: Number(form.roomId),
    parkingSpaceId: form.parkingSpaceId ? Number(form.parkingSpaceId) : null,
    estimatedArrivalTime: form.estimatedArrivalTime || null
  }
}

function formFromBooking(booking) {
  const checkIn = new Date(booking.checkIn)
  const checkOut = new Date(booking.checkOut)

  return {
    guestName: booking.guestName,
    checkInDate: checkIn.toISOString().slice(0, 10),
    checkInTime: checkIn.toTimeString().slice(0, 5),
    checkOutDate: checkOut.toISOString().slice(0, 10),
    checkOutTime: checkOut.toTimeString().slice(0, 5),
    guests: booking.numberOfGuests,
    roomId: String(booking.roomId),
    parkingSpaceId: booking.parkingSpaceId === null ? '' : String(booking.parkingSpaceId),
    estimatedArrivalTime: booking.estimatedArrivalTime ?? ''
  }
}

export default function App() {
  const [activePage, setActivePage] = useState(getPageFromPath)
  const [availableRooms, setAvailableRooms] = useState([])
  const [availableParkingSpaces, setAvailableParkingSpaces] = useState([])
  const [hasCheckedAvailability, setHasCheckedAvailability] = useState(false)
  const [selectedRoomId, setSelectedRoomId] = useState(null)
  const [form, setForm] = useState(emptyForm)
  const [selectedBookingId, setSelectedBookingId] = useState(null)
  const [status, setStatus] = useState({ type: 'idle', text: 'Ready' })
  const [preferences, setPreferences] = useState(loadPreferences)
  const [activities, setActivities] = useState([])

  const recordActivity = useCallback((text, type = 'idle') => {
    setActivities((current) => [
      {
        id: nextActivityId++,
        text,
        type,
        happenedAt: new Date().toLocaleTimeString()
      },
      ...current
    ].slice(0, 8))
  }, [])

  useEffect(() => {
    if (window.location.pathname === '/') {
      window.history.replaceState({}, '', pageRoutes.home)
      setActivePage('home')
    }

    function handlePopState() {
      setActivePage(getPageFromPath())
    }

    window.addEventListener('popstate', handlePopState)
    return () => window.removeEventListener('popstate', handlePopState)
  }, [])

  const handleLoadSuccess = useCallback(() => {
    setStatus({ type: 'success', text: 'Data loaded from backend.' })
    recordActivity('Loaded bookings, rooms, and parking spaces.', 'success')
  }, [recordActivity])

  const handleLoadError = useCallback((error) => {
    setStatus({ type: 'error', text: error.message })
    recordActivity(`Load failed: ${error.message}`, 'error')
  }, [recordActivity])

  const {
    bookings,
    rooms,
    parkingSpaces,
    bookingPolicy,
    loading,
    loadData
  } = useDashboardData({
    onLoadSuccess: handleLoadSuccess,
    onLoadError: handleLoadError
  })

  const activeBookingPolicy = bookingPolicy ?? fallbackBookingPolicy

  const selectedBooking = useMemo(
    () => bookings.find((booking) => booking.id === selectedBookingId) ?? null,
    [bookings, selectedBookingId]
  )

  const visibleBookings = useMemo(
    () => preferences.hideCancelledBookings
      ? bookings.filter((booking) => booking.status !== 'Cancelled')
      : bookings,
    [bookings, preferences.hideCancelledBookings]
  )

  const visibleRooms = hasCheckedAvailability ? availableRooms : rooms
  const totalGuests = Number(form.guests)

  useEffect(() => {
    if (visibleRooms.length === 0) {
      setSelectedRoomId(null)
      return
    }

    if (!visibleRooms.some((room) => room.id === selectedRoomId)) {
      setSelectedRoomId(visibleRooms[0].id)
    }
  }, [selectedRoomId, visibleRooms])

  function updateForm(nextForm) {
    const availabilityFields = [
      'checkInDate',
      'checkInTime',
      'checkOutDate',
      'checkOutTime',
      'guests'
    ]

    const changedAvailability = availabilityFields.some((field) => form[field] !== nextForm[field])

    if (changedAvailability) {
      setAvailableRooms([])
      setAvailableParkingSpaces([])
      setHasCheckedAvailability(false)
      setForm({ ...nextForm, roomId: '', parkingSpaceId: '' })
      return
    }

    setForm(nextForm)
  }

  function updatePreference(name, value) {
    const next = savePreferences({ ...preferences, [name]: value })
    setPreferences(next)
    recordActivity('Updated display preferences.')
  }

  function navigateTo(page) {
    setActivePage(page)
    const nextPath = pageRoutes[page]

    if (window.location.pathname !== nextPath) {
      window.history.pushState({}, '', nextPath)
    }
  }

  async function searchAvailability() {
    if (!form.checkInDate || !form.checkOutDate) {
      setStatus({ type: 'error', text: 'Choose check-in and check-out dates first.' })
      recordActivity('Availability check needs check-in and check-out dates.', 'error')
      return
    }

    try {
      const checkInTimes = new Set(buildTimeOptions(activeBookingPolicy.earliestCheckIn, activeBookingPolicy.latestCheckIn, activeBookingPolicy.timeSlotMinutes))
      const checkOutTimes = new Set(buildTimeOptions('00:00', activeBookingPolicy.latestCheckOut, activeBookingPolicy.timeSlotMinutes))
      const checkInTime = checkInTimes.has(form.checkInTime) ? form.checkInTime : activeBookingPolicy.earliestCheckIn
      const checkOutTime = checkOutTimes.has(form.checkOutTime) ? form.checkOutTime : emptyForm.checkOutTime
      const checkIn = `${form.checkInDate}T${checkInTime}:00`
      const checkOut = `${form.checkOutDate}T${checkOutTime}:00`
      const roomResult = await roomsApi.getAvailable({
        checkIn,
        checkOut,
        guests: totalGuests
      })
      const parkingResult = await parkingSpacesApi.getAvailable({
        checkIn,
        checkOut
      })
      const selectedRoomStillAvailable = roomResult.some((room) => String(room.id) === form.roomId)
      const selectedParkingStillAvailable = parkingResult.some((space) => String(space.id) === form.parkingSpaceId)
      setAvailableRooms(roomResult)
      setAvailableParkingSpaces(parkingResult)
      setHasCheckedAvailability(true)
      setSelectedRoomId(roomResult[0]?.id ?? null)
      setForm((current) => ({
        ...current,
        roomId: current.roomId && selectedRoomStillAvailable ? current.roomId : '',
        parkingSpaceId: current.parkingSpaceId && selectedParkingStillAvailable ? current.parkingSpaceId : ''
      }))
      setStatus({ type: 'success', text: `${roomResult.length} room(s), ${parkingResult.length} parking space(s) available.` })
      recordActivity(`Checked availability: ${roomResult.length} room(s), ${parkingResult.length} parking space(s) found.`, 'success')
    } catch (error) {
      setStatus({ type: 'error', text: error.message })
      recordActivity(`Availability check failed: ${error.message}`, 'error')
    }
  }

  async function saveBooking(event) {
    event.preventDefault()

    if (!form.roomId) {
      setStatus({ type: 'error', text: 'Choose an available room before creating the booking.' })
      recordActivity('Booking needs an available room.', 'error')
      return
    }

    try {
      if (selectedBookingId) {
        await bookingApi.update(selectedBookingId, toRequest(form, activeBookingPolicy))
        setStatus({ type: 'success', text: 'Booking updated.' })
        recordActivity('Updated booking.', 'success')
      } else {
        await bookingApi.create(toRequest(form, activeBookingPolicy))
        setStatus({ type: 'success', text: 'Booking created.' })
        recordActivity('Created booking.', 'success')
      }

      setForm(emptyForm)
      setSelectedBookingId(null)
      setAvailableRooms([])
      setAvailableParkingSpaces([])
      setHasCheckedAvailability(false)
      await loadData()
    } catch (error) {
      setStatus({ type: 'error', text: error.message })
      recordActivity(`Save failed: ${error.message}`, 'error')
    }
  }

  async function cancelBooking(id) {
    try {
      await bookingApi.cancel(id)
      setStatus({ type: 'success', text: 'Booking cancelled.' })
      recordActivity('Cancelled booking.', 'success')
      if (selectedBookingId === id) {
        setSelectedBookingId(null)
        setForm(emptyForm)
      }
      await loadData()
    } catch (error) {
      setStatus({ type: 'error', text: error.message })
      recordActivity(`Cancel failed: ${error.message}`, 'error')
    }
  }

  function selectBooking(booking) {
    setSelectedBookingId(booking.id)
    setSelectedRoomId(booking.roomId)
    setForm(formFromBooking(booking))
    navigateTo('booking')
    setStatus({ type: 'idle', text: `Editing booking for ${booking.guestName}.` })
    recordActivity(`Selected booking for ${booking.guestName}.`)
  }

  function selectRoomForBooking(roomId) {
    setSelectedRoomId(roomId)
    setForm((current) => ({ ...current, roomId: String(roomId) }))
    navigateTo('booking')
    setStatus({ type: 'idle', text: 'Complete your booking details.' })
    recordActivity('Moved selected room to booking.')
  }

  function resetForm() {
    setSelectedBookingId(null)
    setForm(emptyForm)
    setAvailableRooms([])
    setAvailableParkingSpaces([])
    setHasCheckedAvailability(false)
    setStatus({ type: 'idle', text: 'Ready' })
    recordActivity('Cleared booking form.')
  }

  return (
    <main className="app-shell" data-density={preferences.density}>
      <section className="site-header">
        <div>
          <h1>CleanBookingV2</h1>
          <p>Bed and breakfast room booking</p>
        </div>
        <nav className="site-nav" aria-label="Main navigation">
          {['home', 'stays', 'booking'].map((page) => (
            <button
              key={page}
              type="button"
              className={activePage === page ? 'active' : 'secondary'}
              onClick={() => navigateTo(page)}
            >
              {page === 'home' ? 'Home' : page === 'stays' ? 'Stays' : 'Booking'}
            </button>
          ))}
        </nav>
        <div className="toolbar-actions">
          <label className="toolbar-control">
            Density
            <select
              value={preferences.density}
              onChange={(event) => updatePreference('density', event.target.value)}
            >
              <option value="comfortable">Comfortable</option>
              <option value="compact">Compact</option>
            </select>
          </label>
          <label className="checkbox-row toolbar-checkbox">
            <input
              type="checkbox"
              checked={preferences.hideCancelledBookings}
              onChange={(event) => updatePreference('hideCancelledBookings', event.target.checked)}
            />
            Hide cancelled
          </label>
          <button type="button" onClick={loadData} disabled={loading}>
            {loading ? 'Loading...' : 'Refresh'}
          </button>
        </div>
      </section>

      <StatusMessage status={status} />

      {activePage === 'home' && (
        <section className="home-view">
          <div className="home-copy">
            <h2>Book a quiet stay with live availability.</h2>
            <p>
              CleanBookingV2 shows available bed and breakfast rooms for your dates,
              helps you inspect room details, and completes the booking through the
              backend so overlapping bookings are prevented.
            </p>
            <button type="button" onClick={() => navigateTo('stays')}>
              Find a stay
            </button>
          </div>
          <img src="/rooms/room-4.png" alt="Family room interior" className="home-image" />
        </section>
      )}

      {activePage === 'stays' && (
        <section className="stays-view">
          <form className="panel stay-search" onSubmit={(event) => { event.preventDefault(); searchAvailability() }}>
            <div className="panel-header">
              <h2>Search stays</h2>
            </div>
            <div className="search-grid">
              <label>
                <span title={`Choose the arrival date. Check-in time defaults to ${activeBookingPolicy.earliestCheckIn} and must be between ${activeBookingPolicy.earliestCheckIn} and ${activeBookingPolicy.latestCheckIn} in booking.`}>
                  Check-in
                </span>
                <input type="date" value={form.checkInDate} onChange={(event) => updateForm({ ...form, checkInDate: event.target.value })} required />
              </label>
              <label>
                <span title={`Choose the departure date. Check-out must be after check-in and no later than ${activeBookingPolicy.latestCheckOut} in booking.`}>
                  Check-out
                </span>
                <input type="date" value={form.checkOutDate} onChange={(event) => updateForm({ ...form, checkOutDate: event.target.value })} required />
              </label>
              <label>
                <span title="Number of guests must fit the room capacity.">
                  Guests
                </span>
                <input type="number" min="1" max={activeBookingPolicy.maximumGuests} value={form.guests} onChange={(event) => updateForm({ ...form, guests: event.target.value })} required />
              </label>
              <button type="submit">Search</button>
            </div>
          </form>

          <RoomPanel
            rooms={visibleRooms}
            bookings={bookings}
            selectedRoomId={selectedRoomId}
            isFiltered={hasCheckedAvailability}
            onSelectRoom={setSelectedRoomId}
            onBookRoom={hasCheckedAvailability ? selectRoomForBooking : null}
          />
        </section>
      )}

      {activePage === 'booking' && (
        <section className="booking-view">
          <BookingForm
            form={form}
            rooms={rooms}
            parkingSpaces={parkingSpaces}
            availableParkingSpaces={availableParkingSpaces}
            availableRooms={availableRooms}
            hasCheckedAvailability={hasCheckedAvailability}
            bookingPolicy={activeBookingPolicy}
            selectedBooking={selectedBooking}
            onChange={updateForm}
            onSubmit={saveBooking}
            onSearchAvailability={searchAvailability}
            onReset={resetForm}
          />

          <BookingList
            bookings={visibleBookings}
            selectedBookingId={selectedBookingId}
            onSelect={selectBooking}
            onCancel={cancelBooking}
          />

          <ActivityPanel activities={activities} />
        </section>
      )}
    </main>
  )
}
