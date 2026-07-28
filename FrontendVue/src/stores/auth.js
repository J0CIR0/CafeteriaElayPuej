import { defineStore } from 'pinia'
import api from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user')) || null
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'admin',
    isWorker: (state) => state.user?.role === 'worker' || state.user?.role === 'admin'
  },
  actions: {
    async login(email, password) {
      try {
        const response = await api.post('/Auth/login', { email, password })
        this.token = response.data.token
        this.user = {
          id: response.data.userId,
          email: response.data.email,
          fullName: response.data.fullName,
          role: response.data.role
        }
        localStorage.setItem('token', this.token)
        localStorage.setItem('user', JSON.stringify(this.user))
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al iniciar sesión' }
      }
    },
    async register(userData) {
      try {
        const response = await api.post('/Auth/register', userData)
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al registrar usuario' }
      }
    },
    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    },
    async validateSession() {
      try {
        const response = await api.get('/Auth/validate-session')
        return { success: true, data: response.data }
      } catch (error) {
        this.logout()
        return { success: false }
      }
    }
  }
})