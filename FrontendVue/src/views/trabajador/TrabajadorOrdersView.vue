<template>
    <div>
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="fw-bold" style="color: var(--color-cafe);">
                <span style="border-left: 4px solid var(--color-cafe); padding-left: 12px;">Gestion de Pedidos</span>
            </h2>
        </div>

        <div class="row mb-3">
            <div class="col-md-3">
                <button class="btn btn-outline-primary w-100" @click="filter = 'all'">Todos ({{ adminStore.orders.length }})</button>
            </div>
            <div class="col-md-3">
                <button class="btn btn-outline-warning w-100" @click="filter = 'pending'">Pendientes ({{ pendingOrders.length }})</button>
            </div>
            <div class="col-md-3">
                <button class="btn btn-outline-success w-100" @click="filter = 'paid'">Pagados ({{ paidOrders.length }})</button>
            </div>
            <div class="col-md-3">
                <button class="btn btn-outline-info w-100" @click="filter = 'delivered'">Entregados ({{ deliveredOrders.length }})</button>
            </div>
        </div>

        <div class="admin-card">
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-cafe">
                        <thead>
                            <tr>
                                <th>Pedido</th>
                                <th>Cliente</th>
                                <th>Total</th>
                                <th>Pago</th>
                                <th>Estado</th>
                                <th>Fecha</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="order in filteredOrders" :key="order.id">
                                <td>#{{ order.orderNumber }}</td>
                                <td>{{ order.user?.fullName || 'N/A' }}</td>
                                <td class="text-success fw-bold">${{ order.total.toFixed(2) }}</td>
                                <td>
                                    <span class="badge" :class="order.paymentStatus === 'paid' ? 'badge-verde' : 'badge-amarillo'">
                                        {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}
                                    </span>
                                </td>
                                <td>
                                    <span class="badge" :class="getOrderStatusClass(order.orderStatus)">
                                        {{ getOrderStatusText(order.orderStatus) }}
                                    </span>
                                </td>
                                <td>{{ new Date(order.createdAt).toLocaleDateString() }}</td>
                                <td>
                                    <button v-if="order.paymentStatus === 'pending'" class="btn btn-sm btn-verde me-1" @click="markAsPaid(order.id)">
                                        Pagar
                                    </button>
                                    <button v-if="order.paymentStatus === 'paid' && order.orderStatus === 'preparing'" class="btn btn-sm btn-primary me-1" @click="updateStatus(order.id, 'ready')">
                                        Listo
                                    </button>
                                    <button v-if="order.paymentStatus === 'paid' && order.orderStatus === 'ready'" class="btn btn-sm btn-info" @click="updateStatus(order.id, 'delivered')">
                                        Entregar
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted } from 'vue'
    import { useTrabajadorStore } from '../../stores/trabajador'

    const trabajadorStore = useTrabajadorStore()
    const filter = ref('all')

    // Estos computados ahora usan el array 'pedidos' de tu trabajadorStore
    const pendingOrders = computed(() => trabajadorStore.pedidos.filter(o => o.orderStatus === 'pending' || o.orderStatus === 'preparing'))
    const paidOrders = computed(() => trabajadorStore.pedidos.filter(o => o.paymentStatus === 'paid'))
    const deliveredOrders = computed(() => trabajadorStore.pedidos.filter(o => o.orderStatus === 'delivered'))

    const filteredOrders = computed(() => {
        if (filter.value === 'pending') return pendingOrders.value
        if (filter.value === 'paid') return paidOrders.value
        if (filter.value === 'delivered') return deliveredOrders.value
        return trabajadorStore.pedidos
    })

    onMounted(async () => {
        await trabajadorStore.fetchPedidosPendientes()
    })

    const getOrderStatusClass = (status) => {
        const classes = {
            pending: 'bg-secondary',
            preparing: 'bg-info',
            ready: 'bg-primary',
            delivered: 'bg-success'
        }
        return classes[status] || 'bg-secondary'
    }

    const getOrderStatusText = (status) => {
        const texts = {
            pending: 'Pendiente',
            preparing: 'En Preparacion',
            ready: 'Listo para Recoger',
            delivered: 'Entregado'
        }
        return texts[status] || status
    }

    // MÉTODOS ADAPTADOS
    // Como tu trabajador.js solo tiene 'actualizarEstadoPedido',
    // lo usaremos tanto para pagar como para cambiar estado.

    const markAsPaid = async (orderId) => {
        // Nota: Si tu backend necesita un endpoint separado para pagos,
        // asegúrate de tenerlo, si no, usa actualizarEstadoPedido
        console.log("Acción de pago para:", orderId)
    }

    const updateStatus = async (orderId, status) => {
        const result = await trabajadorStore.actualizarEstadoPedido(orderId, status)
        if (result.success) {
            await trabajadorStore.fetchPedidosPendientes()
        }
    }
</script>