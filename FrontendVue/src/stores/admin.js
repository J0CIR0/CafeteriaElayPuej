import { defineStore } from 'pinia'
import api from '../services/api'

export const useAdminStore = defineStore('admin', {
  state: () => ({
    products: [],
    categories: [],
    users: [],
    orders: [],
    inventoryMovements: [],
    statistics: {
      totalProducts: 0,
      totalUsers: 0,
      totalOrders: 0,
      totalRevenue: 0,
      pendingOrders: 0,
      lowStockProducts: 0
    },
    loading: false,
    error: null
  }),
  getters: {
    getProductById: (state) => (id) => {
      return state.products.find(p => p.id === id)
    },
    getCategoryById: (state) => (id) => {
      return state.categories.find(c => c.id === id)
    },
    getOrdersByStatus: (state) => (status) => {
      return state.orders.filter(o => o.orderStatus === status)
    },
    getRevenueToday: (state) => {
      const today = new Date().toDateString()
      return state.orders
        .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt).toDateString() === today)
        .reduce((sum, o) => sum + o.total, 0)
    },
    getRevenueThisWeek: (state) => {
      const weekAgo = new Date()
      weekAgo.setDate(weekAgo.getDate() - 7)
      return state.orders
        .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt) >= weekAgo)
        .reduce((sum, o) => sum + o.total, 0)
    },
    getRevenueThisMonth: (state) => {
      const monthAgo = new Date()
      monthAgo.setMonth(monthAgo.getMonth() - 1)
      return state.orders
        .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt) >= monthAgo)
        .reduce((sum, o) => sum + o.total, 0)
    }
  },
  actions: {
    async fetchDashboardData() {
      this.loading = true
      this.error = null
      try {
        await Promise.all([
          this.fetchProducts(),
          this.fetchCategories(),
          this.fetchUsers(),
          this.fetchOrders(),
          this.fetchInventoryMovements(),
          this.fetchStatistics()
        ])
      } catch (error) {
        this.error = 'Error al cargar datos del dashboard'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    async fetchProducts() {
      try {
        const response = await api.get('/Products')
        this.products = response.data
      } catch (error) {
        console.error('Error al cargar productos', error)
      }
    },
    async fetchCategories() {
      try {
        const response = await api.get('/Categories')
        this.categories = response.data
      } catch (error) {
        console.error('Error al cargar categorias', error)
      }
    },
    async fetchUsers() {
      try {
        const response = await api.get('/Users')
        this.users = response.data
      } catch (error) {
        console.error('Error al cargar usuarios', error)
      }
    },
    async fetchOrders() {
      try {
        const response = await api.get('/Orders')
        this.orders = response.data
      } catch (error) {
        console.error('Error al cargar pedidos', error)
      }
    },
    async fetchInventoryMovements() {
      try {
        const response = await api.get('/Inventory/movements')
        this.inventoryMovements = response.data
      } catch (error) {
        console.error('Error al cargar movimientos de inventario', error)
      }
    },
    async fetchStatistics() {
      try {
        const response = await api.get('/Inventory/summary')
        this.statistics = {
          totalProducts: this.products.length,
          totalUsers: this.users.length,
          totalOrders: this.orders.length,
          totalRevenue: this.orders
            .filter(o => o.paymentStatus === 'paid')
            .reduce((sum, o) => sum + o.total, 0),
          pendingOrders: this.orders.filter(o => o.paymentStatus === 'pending').length,
          lowStockProducts: response.data.lowStockCount || 0
        }
      } catch (error) {
        console.error('Error al cargar estadisticas', error)
      }
    },
    async createProduct(productData) {
      try {
        const response = await api.post('/Products', productData)
        await this.fetchProducts()
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al crear producto' }
      }
    },
    async updateProduct(id, productData) {
      try {
        await api.put(`/Products/${id}`, productData)
        await this.fetchProducts()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar producto' }
      }
    },
    async deleteProduct(id) {
      try {
        await api.delete(`/Products/${id}`)
        await this.fetchProducts()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al eliminar producto' }
      }
    },
    async updateStock(productId, newStock, reason) {
      try {
        await api.patch(`/Products/${productId}/stock`, { newStock, reason })
        await this.fetchProducts()
        await this.fetchInventoryMovements()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar stock' }
      }
    },
    async createCategory(categoryData) {
      try {
        const response = await api.post('/Categories', categoryData)
        await this.fetchCategories()
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al crear categoria' }
      }
    },
    async updateCategory(id, categoryData) {
      try {
        await api.put(`/Categories/${id}`, categoryData)
        await this.fetchCategories()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar categoria' }
      }
    },
    async deleteCategory(id) {
      try {
        await api.delete(`/Categories/${id}`)
        await this.fetchCategories()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al eliminar categoria' }
      }
    },
    async updateOrderPaymentStatus(orderId, status) {
      try {
        await api.patch(`/Orders/${orderId}/payment-status`, { status })
        await this.fetchOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar estado de pago' }
      }
    },
    async updateOrderStatus(orderId, status) {
      try {
        await api.patch(`/Orders/${orderId}/order-status`, { status })
        await this.fetchOrders()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar estado del pedido' }
      }
    },
    async toggleUserStatus(userId) {
      try {
        await api.patch(`/Users/${userId}/status`)
        await this.fetchUsers()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al cambiar estado del usuario' }
      }
    },
    async deleteUser(userId) {
      try {
        await api.delete(`/Users/${userId}`)
        await this.fetchUsers()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al eliminar usuario' }
      }
    },
    async updateUser(id, userData) {
      try {
        await api.put(`/Users/${id}`, userData)
        await this.fetchUsers()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar usuario' }
      }
    },
    async createMovement(movementData) {
      try {
        const response = await api.post('/Inventory/movements', movementData)
        await this.fetchInventoryMovements()
        await this.fetchProducts()
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al registrar movimiento' }
      }
    }
  }
})