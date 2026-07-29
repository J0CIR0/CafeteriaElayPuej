import { defineStore } from 'pinia'
import api from '../services/api'

export const useTrabajadorStore = defineStore('trabajador', {
    state: () => ({
        pedidos: [],
        products: [], // NUEVO: Para guardar la lista de productos
        inventoryMovements: [], // NUEVO: Para guardar los movimientos
        loading: false
    }),
    actions: {
        // --- MÉTODOS DE PEDIDOS ---
        async fetchPedidosPendientes() {
            this.loading = true
            try {
                const response = await api.get('/Orders/pending')
                this.pedidos = response.data
            } catch (error) {
                console.error('Error al cargar pedidos:', error)
                this.pedidos = []
            } finally {
                this.loading = false
            }
        },

        async actualizarEstadoPedido(id, nuevoEstado) {
            try {
                await api.patch(`/Orders/${id}/order-status`, { status: nuevoEstado })
                return { success: true }
            } catch (error) {
                console.error('Error al actualizar estado:', error)
                return { success: false, message: error.response?.data?.message || 'Error al actualizar' }
            }
        },

        // --- NUEVOS MÉTODOS PARA INVENTARIO ---
        async fetchProducts() {
            try {
                const response = await api.get('/Products') // Ajusta la ruta según tu API
                this.products = response.data
            } catch (error) {
                console.error('Error al cargar productos:', error)
            }
        },

        async fetchInventoryMovements() {
            try {
                const response = await api.get('/InventoryMovements') // Ajusta la ruta según tu API
                this.inventoryMovements = response.data
            } catch (error) {
                console.error('Error al cargar movimientos:', error)
            }
        },

        async updateStock(id, newStock, reason) {
            try {
                await api.post(`/Products/${id}/adjust-stock`, { stock: newStock, reason }) // Ajusta según tu API
                return { success: true }
            } catch (error) {
                console.error('Error al ajustar stock:', error)
                return { success: false }
            }
        }
    }
})