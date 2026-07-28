import { defineStore } from 'pinia'
import api from '../services/api'

export const useProductsStore = defineStore('products', {
  state: () => ({
    products: [],
    categories: [],
    loading: false,
    error: null
  }),
  getters: {
    getProductsByCategory: (state) => (categoryId) => {
      return state.products.filter((product) => Number(product.categoryId) === Number(categoryId))
    }
  },
  actions: {
    async fetchProducts() {
      this.loading = true
      try {
        const response = await api.get('/Products')
        this.products = response.data
        this.error = null
      } catch (error) {
        this.error = 'Error al cargar productos'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    async fetchAvailableProducts() {
      this.loading = true
      try {
        const response = await api.get('/Products/available')
        this.products = response.data
        this.error = null
      } catch (error) {
        this.error = 'Error al cargar productos disponibles'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    async fetchCategories() {
      try {
        const response = await api.get('/Categories')
        this.categories = response.data
      } catch (error) {
        console.error('Error al cargar categorías', error)
      }
    },
    async createProduct(product) {
      try {
        const response = await api.post('/Products', product)
        await this.fetchProducts()
        return { success: true, data: response.data }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al crear producto' }
      }
    },
    async updateProduct(id, product) {
      try {
        await api.put(`/Products/${id}`, { ...product, id })
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
    async updateStock(id, newStock, reason) {
      try {
        await api.patch(`/Products/${id}/stock`, { newStock, reason })
        await this.fetchProducts()
        return { success: true }
      } catch (error) {
        return { success: false, message: error.response?.data?.message || 'Error al actualizar stock' }
      }
    }
  }
})
