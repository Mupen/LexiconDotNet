import { useCallback, useEffect, useState } from 'react'
import { bookingPolicyApi } from '../api/bookingPolicyApi.js'
import { bookingApi } from '../api/bookingsApi.js'
import { parkingSpacesApi } from '../api/parkingSpacesApi.js'
import { roomsApi } from '../api/roomsApi.js'

export function useDashboardData({ onLoadSuccess, onLoadError } = {}) {
  const [bookings, setBookings] = useState([])
  const [rooms, setRooms] = useState([])
  const [parkingSpaces, setParkingSpaces] = useState([])
  const [bookingPolicy, setBookingPolicy] = useState(null)
  const [loading, setLoading] = useState(false)

  const loadData = useCallback(async ({ signal } = {}) => {
    setLoading(true)
    try {
      const [bookingResult, roomResult, parkingResult, policyResult] = await Promise.all([
        bookingApi.getAll({ signal }),
        roomsApi.getAll({ signal }),
        parkingSpacesApi.getAll({ signal }),
        bookingPolicyApi.get({ signal })
      ])

      if (signal?.aborted) {
        return
      }

      setBookings(bookingResult)
      setRooms(roomResult)
      setParkingSpaces(parkingResult)
      setBookingPolicy(policyResult)
      onLoadSuccess?.()
      return true
    } catch (error) {
      if (signal?.aborted || error.name === 'AbortError') {
        return false
      }

      onLoadError?.(error)
      return false
    } finally {
      if (!signal?.aborted) {
        setLoading(false)
      }
    }
  }, [onLoadError, onLoadSuccess])

  useEffect(() => {
    const controller = new AbortController()
    loadData({ signal: controller.signal }).catch(() => {})
    return () => controller.abort()
  }, [loadData])

  return {
    bookings,
    rooms,
    parkingSpaces,
    bookingPolicy,
    loading,
    loadData
  }
}
