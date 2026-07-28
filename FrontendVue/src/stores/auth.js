import { defineStore } from 'pinia'
import api from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user')) || null,
    pendingVerificationEmail: localStorage.getItem('pendingVerificationEmail') || ''
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'admin',
    isWorker: (state) => state.user?.role === 'worker' || state.user?.role === 'admin',
    isEmailVerified: (state) => state.user?.isEmailVerified || false
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
          role: response.data.role,
          isEmailVerified: response.data.isEmailVerified || false
        }
        localStorage.setItem('token', this.token)
        localStorage.setItem('user', JSON.stringify(this.user))
        this.pendingVerificationEmail = ''
        localStorage.removeItem('pendingVerificationEmail')
        return { success: true }
      } catch (error) {
        const message = error.response?.data?.message || 'Error al iniciar sesión'
        if (message.toLowerCase().includes('verificar')) {
          this.pendingVerificationEmail = email
          localStorage.setItem('pendingVerificationEmail', email)
        }
        return { success: false, message: error.response?.data?.message || 'Error al iniciar sesión' }
      }
    },
    async register(userData) {
      try {
        const response = await api.post('/Auth/register', userData)
        this.pendingVerificationEmail = userData.email
        localStorage.setItem('pendingVerificationEmail', userData.email)
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al registrar usuario' }
      }
    },
    async sendVerificationCode(email) {
      try {
        await api.post('/Verification/send', { email })
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al enviar código' }
      }
    },
    async verifyEmail(email, code) {
      try {
        await api.post('/Verification/verify', { email, code })

        if (this.user && this.user.email === email) {
          this.user.isEmailVerified = true
          localStorage.setItem('user', JSON.stringify(this.user))
        }

        this.pendingVerificationEmail = ''
        localStorage.removeItem('pendingVerificationEmail')
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Código inválido o expirado' }
      }
    },
    async forgotPassword(email) {
      try {
        await api.post('/Verification/forgot-password', { email })
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al enviar código de recuperación' }
      }
    },
    async resetPassword(email, code, newPassword) {
      try {
        await api.post('/Verification/reset-password', { email, code, newPassword })
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Código inválido o expirado' }
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