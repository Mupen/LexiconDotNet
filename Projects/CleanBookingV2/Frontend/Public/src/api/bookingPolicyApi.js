import { apiRequest } from './apiClient.js'

export const bookingPolicyApi = {
  get(options = {}) {
    return apiRequest('/api/booking-policy', options)
  }
}
