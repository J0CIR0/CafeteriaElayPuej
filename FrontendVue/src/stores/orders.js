import { defineStore } from 'pinia'
import api from '../services/api'

export const useOrdersStore = defineStore('orders', {
  state: () => ({
    orders: [],
    myOrders: [],
    pendingOrders: [],
    paidOrders: [],
    currentOrder: null,
    loading: false,
    error: null
  }),

  getters: {
    getOrderById: (state) => (id) => {
      return state.myOrders.find(o => Number(o.id) === Number(id)) || state.orders.find(o => Number(o.id) === Number(id))
    },
    getOrdersByStatus: (state) => (status) => {
      if (status === 'all') return state.myOrders
      if (status === 'pending') return state.myOrders.filter(o => o.paymentStatus === 'pending')
      if (status === 'preparing') return state.myOrders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'preparing')
      if (status === 'ready') return state.myOrders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'ready')
      if (status === 'delivered') return state.myOrders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'delivered')
      return state.myOrders
    }
  },

  actions: {
    async fetchOrders() {
      this.loading = true
      try {
        const response = await api.get('/Orders')
        this.orders = response.data || []
        this.error = null
      } catch (error) {
        this.error = 'Error al cargar pedidos'
        console.error(error)
      } finally {
        this.loading = false
      }
    },

    async fetchMyOrders() {
      this.loading = true
      try {
        const response = await api.get('/Orders/my-orders')
        this.myOrders = response.data || []
        this.error = null
      } catch (error) {
        this.error = 'Error al cargar mis pedidos'
        console.error('Error al cargar mis pedidos', error)
      } finally {
        this.loading = false
      }
    },

    async fetchOrder(id) {
      try {
        const response = await api.get(`/Orders/${id}`)
        this.currentOrder = response.data
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al obtener detalle del pedido' }
      }
    },

    async fetchPendingOrders() {
      try {
        const response = await api.get('/Orders/pending-payment')
        this.pendingOrders = response.data || []
      } catch (error) {
        console.error('Error al cargar pedidos pendientes', error)
      }
    },

    async fetchPaidOrders() {
      try {
        const response = await api.get('/Orders/paid')
        this.paidOrders = response.data || []
      } catch (error) {
        console.error('Error al cargar pedidos pagados', error)
      }
    },

    async createOrder(orderData) {
      try {
        const response = await api.post('/Orders', orderData)
        this.currentOrder = response.data
        await this.fetchMyOrders()
        return { success: true, data: response.data }
      } catch (error) {
        const msg = error.response?.data?.message || error.response?.data || error.message || 'Error al crear el pedido'
        return { success: false, message: typeof msg === 'string' ? msg : 'Error al crear el pedido' }
      }
    },

    async cancelOrder(id) {
      try {
        await api.delete(`/Orders/${id}`)
        await this.fetchMyOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al cancelar el pedido' }
      }
    },

    async updatePaymentStatus(id, status) {
      try {
        await api.patch(`/Orders/${id}/payment-status`, { status })
        await this.fetchMyOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar estado de pago' }
      }
    },

    async updateOrderStatus(id, status) {
      try {
        await api.patch(`/Orders/${id}/order-status`, { status })
        await this.fetchMyOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar estado del pedido' }
      }
    }
  }
})