import { defineStore } from 'pinia'
import api from '../services/api'

export const useWorkerStore = defineStore('worker', {
  state: () => ({
    orders: [],
    products: [],
    categories: [],
    clients: [],
    inventoryMovements: [],
    loading: false,
    error: null,
    lastOrdersCount: 0,
    hasNewOrdersAlert: false
  }),

  getters: {
    pendingPaymentOrders: (state) => state.orders.filter(o => o.paymentStatus === 'pending'),
    preparingOrders: (state) => state.orders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'preparing'),
    readyOrders: (state) => state.orders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'ready'),
    deliveredOrders: (state) => state.orders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'delivered'),
    
    todayOrders: (state) => {
      const todayStr = new Date().toDateString()
      return state.orders.filter(o => new Date(o.createdAt).toDateString() === todayStr)
    },
    todayDeliveredOrders: (state) => {
      const todayStr = new Date().toDateString()
      return state.orders.filter(o => o.orderStatus === 'delivered' && new Date(o.createdAt).toDateString() === todayStr)
    },
    todayRevenue: (state) => {
      const todayStr = new Date().toDateString()
      return state.orders
        .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt).toDateString() === todayStr)
        .reduce((sum, o) => sum + (Number(o.total) || 0), 0)
    },
    todayCashRevenue: (state) => {
      const todayStr = new Date().toDateString()
      return state.orders
        .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt).toDateString() === todayStr && (o.paymentMethod || '').toLowerCase() !== 'qr')
        .reduce((sum, o) => sum + (Number(o.total) || 0), 0)
    },
    todayQrRevenue: (state) => {
      const todayStr = new Date().toDateString()
      return state.orders
        .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt).toDateString() === todayStr && (o.paymentMethod || '').toLowerCase() === 'qr')
        .reduce((sum, o) => sum + (Number(o.total) || 0), 0)
    },

    lowStockProducts: (state) => state.products.filter(p => Number(p.stock) <= Number(p.minStock || 0)),
    outOfStockProducts: (state) => state.products.filter(p => Number(p.stock) === 0),
    totalStock: (state) => state.products.reduce((sum, p) => sum + (Number(p.stock) || 0), 0)
  },

  actions: {
    async fetchDashboardData() {
      this.loading = true
      this.error = null
      try {
        await Promise.all([
          this.fetchOrders(),
          this.fetchProducts(),
          this.fetchCategories(),
          this.fetchClients(),
          this.fetchInventoryMovements()
        ])
      } catch (err) {
        this.error = 'Error al cargar los datos del panel del trabajador'
        console.error(err)
      } finally {
        this.loading = false
      }
    },

    async fetchOrders() {
      try {
        const response = await api.get('/Orders')
        const newOrders = response.data || []
        
        if (this.lastOrdersCount > 0 && newOrders.length > this.lastOrdersCount) {
          this.hasNewOrdersAlert = true
        }
        
        this.orders = newOrders
        this.lastOrdersCount = newOrders.length
        return { success: true, data: newOrders }
      } catch (err) {
        console.error('Error al obtener pedidos', err)
        return { success: false, message: 'Error al obtener pedidos' }
      }
    },

    clearNewOrdersAlert() {
      this.hasNewOrdersAlert = false
    },

    async fetchProducts() {
      try {
        const response = await api.get('/Products/admin/all')
        this.products = response.data || []
        return { success: true }
      } catch (err) {
        try {
          const fallbackResp = await api.get('/Products')
          this.products = fallbackResp.data || []
          return { success: true }
        } catch (e) {
          console.error('Error al obtener productos', e)
          return { success: false }
        }
      }
    },

    async fetchCategories() {
      try {
        const response = await api.get('/Categories/admin/all')
        this.categories = response.data || []
        return { success: true }
      } catch (err) {
        try {
          const fallbackResp = await api.get('/Categories')
          this.categories = fallbackResp.data || []
          return { success: true }
        } catch (e) {
          console.error('Error al obtener categorías', e)
          return { success: false }
        }
      }
    },

    async fetchClients() {
      try {
        const response = await api.get('/Users')
        const allUsers = response.data || []
        this.clients = allUsers.filter(u => (u.role || '').toLowerCase() === 'cliente' || (u.role || '').toLowerCase() === 'customer')
        return { success: true }
      } catch (err) {
        console.error('Error al obtener clientes', err)
        return { success: false }
      }
    },

    async fetchInventoryMovements() {
      try {
        const response = await api.get('/Inventory/movements')
        this.inventoryMovements = response.data || []
        return { success: true }
      } catch (err) {
        console.error('Error al obtener movimientos de inventario', err)
        return { success: false }
      }
    },

    async updateOrderPaymentStatus(orderId, status = 'paid') {
      try {
        await api.patch(`/Orders/${orderId}/payment-status`, { status })
        await this.fetchOrders()
        return { success: true }
      } catch (err) {
        const msg = err.response?.data?.message || 'Error al actualizar el estado de pago'
        return { success: false, message: msg }
      }
    },

    async updateOrderStatus(orderId, status) {
      try {
        await api.patch(`/Orders/${orderId}/order-status`, { status })
        await this.fetchOrders()
        return { success: true }
      } catch (err) {
        const msg = err.response?.data?.message || 'Error al actualizar el estado del pedido'
        return { success: false, message: msg }
      }
    }
  }
})
