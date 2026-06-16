import { apiRequest } from './apiClient.js'

export const bookingApi = {
  getAll(options = {}) {
    return apiRequest('/api/bookings', options)
  },

  create(request) {
    return apiRequest('/api/bookings', {
      method: 'POST',
      body: JSON.stringify(request)
    })
  },

  update(id, request) {
    return apiRequest(`/api/bookings/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request)
    })
  },

  cancel(id) {
    return apiRequest(`/api/bookings/${id}`, {
      method: 'DELETE'
    })
  }
}
