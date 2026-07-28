import { defineStore } from 'pinia'
import api from '../services/api'

export const useOrdersStore = defineStore('orders', {
  state: () => ({
    orders: [],
    myOrders: [],
    pendingOrders: [],
    paidOrders: [],
    loading: false,
    error: null
  }),
  actions: {
    async fetchOrders() {
      this.loading = true
      try {
        const response = await api.get('/Orders')
        this.orders = response.data
        this.error = null
      } catch (error) {
        this.error = 'Error al cargar pedidos'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    async fetchMyOrders() {
      try {
        const response = await api.get('/Orders/my-orders')
        this.myOrders = response.data
      } catch (error) {
        console.error('Error al cargar mis pedidos', error)
      }
    },
    async fetchPendingOrders() {
      try {
        const response = await api.get('/Orders/pending-payment')
        this.pendingOrders = response.data
      } catch (error) {
        console.error('Error al cargar pedidos pendientes', error)
      }
    },
    async fetchPaidOrders() {
      try {
        const response = await api.get('/Orders/paid')
        this.paidOrders = response.data
      } catch (error) {
        console.error('Error al cargar pedidos pagados', error)
      }
    },
    async createOrder(orderData) {
      try {
        const response = await api.post('/Orders', orderData)
        await this.fetchMyOrders()
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al crear pedido' }
      }
    },
    async updatePaymentStatus(id, status) {
      try {
        await api.patch(`/Orders/${id}/payment-status`, { status })
        await this.fetchOrders()
        await this.fetchPendingOrders()
        await this.fetchPaidOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar estado de pago' }
      }
    },
    async updateOrderStatus(id, status) {
      try {
        await api.patch(`/Orders/${id}/order-status`, { status })
        await this.fetchOrders()
        await this.fetchPaidOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar estado del pedido' }
      }
    }
  }
})