import { defineStore } from 'pinia'
import api from '../services/api'

const normalizeRoleFromApi = (role) => {
  const normalized = (role || '').toLowerCase()
  if (normalized === 'worker') return 'mesero'
  if (normalized === 'customer') return 'cliente'
  return normalized || 'cliente'
}

const normalizeRoleToApi = (role) => {
  const normalized = (role || '').toLowerCase()
  if (normalized === 'mesero') return 'worker'
  if (normalized === 'cliente') return 'customer'
  return normalized || 'customer'
}

const computeStatistics = (products, users, orders = []) => ({
  totalProducts: products.length,
  totalUsers: users.length,
  totalOrders: orders.length,
  totalRevenue: orders
    .filter((order) => order.paymentStatus === 'paid')
    .reduce((sum, order) => sum + (Number(order.total) || 0), 0),
  pendingOrders: orders.filter((order) => order.paymentStatus === 'pending').length,
  lowStockProducts: products.filter((product) => Number(product.stock) <= Number(product.minStock || 0)).length
})

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
    getProductById: (state) => (id) => state.products.find((product) => Number(product.id) === Number(id)),
    getCategoryById: (state) => (id) => state.categories.find((category) => Number(category.id) === Number(id)),
    getOrdersByStatus: (state) => (status) => state.orders.filter((order) => order.orderStatus === status),
    getRevenueToday: (state) => {
      const today = new Date().toDateString()
      return state.orders
        .filter((order) => order.paymentStatus === 'paid' && new Date(order.createdAt).toDateString() === today)
        .reduce((sum, order) => sum + (Number(order.total) || 0), 0)
    },
    getRevenueThisWeek: (state) => {
      const weekAgo = new Date()
      weekAgo.setDate(weekAgo.getDate() - 7)
      return state.orders
        .filter((order) => order.paymentStatus === 'paid' && new Date(order.createdAt) >= weekAgo)
        .reduce((sum, order) => sum + (Number(order.total) || 0), 0)
    },
    getRevenueThisMonth: (state) => {
      const monthAgo = new Date()
      monthAgo.setMonth(monthAgo.getMonth() - 1)
      return state.orders
        .filter((order) => order.paymentStatus === 'paid' && new Date(order.createdAt) >= monthAgo)
        .reduce((sum, order) => sum + (Number(order.total) || 0), 0)
    }
  },
  actions: {
    refreshStatistics() {
      this.statistics = computeStatistics(this.products, this.users, this.orders)
    },
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
        const response = await api.get('/Products/admin/all')
        this.products = response.data
        this.refreshStatistics()
      } catch (error) {
        console.error('Error al cargar productos', error)
      }
    },
    async fetchCategories() {
      try {
        const response = await api.get('/Categories/admin/all')
        this.categories = response.data
        this.refreshStatistics()
      } catch (error) {
        console.error('Error al cargar categorias', error)
      }
    },
    async fetchUsers() {
      try {
        const response = await api.get('/Users')
        this.users = response.data.map((user) => ({
          ...user,
          role: normalizeRoleFromApi(user.role)
        }))
        this.refreshStatistics()
      } catch (error) {
        console.error('Error al cargar usuarios', error)
      }
    },
    async fetchOrders() {
      try {
        const response = await api.get('/Orders')
        this.orders = response.data
        this.refreshStatistics()
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
        this.statistics = computeStatistics(this.products, this.users, this.orders)
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
        await api.put(`/Products/${id}`, { ...productData, id })
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
        await api.put(`/Categories/${id}`, { ...categoryData, id })
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
    async createUser(userData) {
      try {
        const response = await api.post('/Users', {
          ...userData,
          role: normalizeRoleToApi(userData.role)
        })
        await this.fetchUsers()
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al crear usuario' }
      }
    },
    async updateUser(id, userData) {
      try {
        await api.put(`/Users/${id}`, {
          ...userData,
          id,
          role: normalizeRoleToApi(userData.role)
        })
        await this.fetchUsers()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar usuario' }
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
