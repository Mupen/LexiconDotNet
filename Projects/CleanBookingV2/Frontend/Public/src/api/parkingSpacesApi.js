import { apiRequest } from './apiClient.js'

export const parkingSpacesApi = {
  getAll(options = {}) {
    return apiRequest('/api/parking-spaces', options)
  },

  getAvailable({ checkIn, checkOut }) {
    const query = new URLSearchParams({
      checkIn,
      checkOut
    })

    return apiRequest(`/api/parking-spaces/available?${query}`)
  }
}
