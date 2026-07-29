import { defineStore } from 'pinia'
import api from '../services/api'

export const useTrabajadorStore = defineStore('trabajador', {
    state: () => ({
        pedidos: [],
        loading: false
    }),
    actions: {
        //Obtener solo los pedidos pendientes (para que el trabajador trabaje)
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

        // Actualizar el estado del pedido
        async actualizarEstadoPedido(id, nuevoEstado) {
            try {
                // Ajusta la ruta controlador
                await api.put(`/Orders/update-status/${id}`, { status: nuevoEstado })
                return { success: true }
            } catch (error) {
                console.error('Error al actualizar estado:', error)
                return { success: false, message: error.response?.data?.message || 'Error al actualizar' }
            }
        }
    }
})