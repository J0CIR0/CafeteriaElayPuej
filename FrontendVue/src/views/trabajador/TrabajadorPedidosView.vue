<template>
    <div>
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h5 class="section-title" style="margin-bottom:0;">Gestión de Pedidos</h5>
        </div>

        <!-- Alert Notification -->
        <div v-if="alert.show" :class="['alert', alert.type === 'success' ? 'alert-success' : 'alert-danger', 'alert-dismissible fade show mb-4']" role="alert">
            <strong>{{ alert.type === 'success' ? '¡Éxito!' : '¡Error!' }}</strong> {{ alert.message }}
            <button type="button" class="btn-close" @click="alert.show = false"></button>
        </div>

        <div class="admin-card">
            <div class="card-body p-0">
                <div v-if="trabajadorStore.loading" class="text-center py-4">
                    <div class="spinner-border" style="color:var(--color-verde-medio);width:2rem;height:2rem;" role="status"></div>
                </div>

                <div v-else-if="!trabajadorStore.pedidos || trabajadorStore.pedidos.length === 0" class="text-center py-4 text-muted">
                    No hay pedidos pendientes
                </div>

                <div v-else class="table-responsive">
                    <table class="table-modern">
                        <thead>
                            <tr>
                                <th>ID Pedido</th>
                                <th>Total</th>
                                <th>Estado</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="pedido in trabajadorStore.pedidos" :key="pedido.id">
                                <td>#{{ pedido.id }}</td>
                                <td style="font-weight:500;">{{ pedido.total }} Bs</td>
                                <td>
                                    <span class="badge-modern badge-verde">
                                        {{ pedido.order_status }}
                                    </span>
                                </td>
                                <td>
                                    <button class="btn btn-primary-outline btn-sm" @click="marcarComoListo(pedido.id)">
                                        Marcar como Listo
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
import { reactive, onMounted } from 'vue'
import { useTrabajadorStore } from '../../stores/trabajador'

const trabajadorStore = useTrabajadorStore()

const alert = reactive({ show: false, message: '', type: 'success' })

onMounted(async () => {
  await trabajadorStore.fetchPedidosPendientes()
})

const marcarComoListo = async (id) => {
  const result = await trabajadorStore.actualizarEstadoPedido(id, 'ready')
  if (result.success) {
    alert.message = 'Pedido actualizado con éxito'
    alert.type = 'success'
    alert.show = true
    await trabajadorStore.fetchPedidosPendientes()
  } else {
    alert.message = 'Error al actualizar el pedido'
    alert.type = 'danger'
    alert.show = true
  }
}
</script>