import { apiRequest } from './apiClient.js'

export const roomsApi = {
  getAll(options = {}) {
    return apiRequest('/api/rooms', options)
  },

  getAvailable({ checkIn, checkOut, guests }) {
    const query = new URLSearchParams({
      checkIn,
      checkOut,
      guests: String(guests)
    })

    return apiRequest(`/api/rooms/available?${query}`)
  }
}
