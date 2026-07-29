<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Gestión de Clientes
          </span>
        </h2>
        <p style="color:white">
          Consulta de clientes registrados e historial de compras
        </p>
      </div>
      <div>
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="refreshClients" :disabled="workerStore.loading">
          <span v-if="workerStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Actualizar Lista</span>
        </button>
      </div>
    </div>

    <!-- Search Input -->
    <div class="card mb-4 border-0 shadow-sm">
      <div class="card-body p-2">
        <div class="input-group">
          <span class="input-group-text bg-white border-0"><i class="bi bi-search"></i></span>
          <input type="text" class="form-control border-0" placeholder="Buscar cliente por nombre completo o email..." v-model="searchQuery">
          <button v-if="searchQuery" class="btn btn-link text-muted" @click="searchQuery = ''">Limpiar</button>
        </div>
      </div>
    </div>

    <!-- Clients Table -->
    <div class="admin-card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-modern align-middle mb-0">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre Completo</th>
                <th>Email</th>
                <th>Teléfono</th>
                <th class="text-center">Total Pedidos</th>
                <th class="text-end">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filteredClients.length === 0">
                <td colspan="6" class="text-center py-4 text-muted">No se encontraron clientes registrados.</td>
              </tr>
              <tr v-for="client in filteredClients" :key="client.id">
                <td>#{{ client.id }}</td>
                <td class="fw-bold">{{ client.fullName || 'Sin Nombre' }}</td>
                <td>{{ client.email }}</td>
                <td>{{ client.phone || 'N/A' }}</td>
                <td class="text-center">
                  <span class="badge bg-secondary rounded-pill px-3 py-1">
                    {{ getClientOrdersCount(client.id) }}
                  </span>
                </td>
                <td class="text-end">
                  <button class="btn btn-sm btn-outline-primary" @click="openClientHistoryModal(client)">
                    Ver Historial
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Client History Modal -->
    <div class="modal fade" id="clientHistoryModal" tabindex="-1" aria-hidden="true" ref="historyModalRef">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content modal-modern" v-if="selectedClient">
          <div class="modal-header">
            <h5 class="modal-title text-white">Historial de Pedidos: {{ selectedClient.fullName }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div class="alert alert-light border mb-3">
              <div><strong>Email:</strong> {{ selectedClient.email }}</div>
              <div><strong>Teléfono:</strong> {{ selectedClient.phone || 'N/A' }}</div>
              <div><strong>Total de Pedidos Realizados:</strong> {{ selectedClientOrders.length }}</div>
            </div>

            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Orden</th>
                    <th>Fecha</th>
                    <th>Estado Pago</th>
                    <th>Estado Pedido</th>
                    <th>Monto Total</th>
                    <th>Detalle</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="selectedClientOrders.length === 0">
                    <td colspan="6" class="text-center py-3 text-muted">Este cliente no ha realizado pedidos aún.</td>
                  </tr>
                  <tr v-for="order in selectedClientOrders" :key="order.id">
                    <td class="fw-bold">#{{ order.orderNumber }}</td>
                    <td>{{ formatDate(order.createdAt) }}</td>
                    <td>
                      <span class="badge badge-modern" :class="order.paymentStatus === 'paid' ? 'badge-verde' : 'badge-amarillo'">
                        {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}
                      </span>
                    </td>
                    <td>
                      <span class="badge badge-modern" :class="getOrderStatusBadgeClass(order.orderStatus)">
                        {{ getOrderStatusLabel(order.orderStatus) }}
                      </span>
                    </td>
                    <td class="fw-bold text-success">Bs. {{ order.total.toFixed(2) }}</td>
                    <td>
                      <button class="btn btn-sm btn-outline-secondary" @click="viewOrderFromHistory(order)">
                        Ver Productos
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useWorkerStore } from '../../stores/worker'

const workerStore = useWorkerStore()
const searchQuery = ref('')
const selectedClient = ref(null)
const historyModalRef = ref(null)
let modalInstance = null

onMounted(async () => {
  await Promise.all([
    workerStore.fetchClients(),
    workerStore.fetchOrders()
  ])
  if (window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(historyModalRef.value)
  }
})

const refreshClients = async () => {
  await Promise.all([
    workerStore.fetchClients(),
    workerStore.fetchOrders()
  ])
}

const filteredClients = computed(() => {
  let list = workerStore.clients
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim()
    list = list.filter(c =>
      (c.fullName && c.fullName.toLowerCase().includes(q)) ||
      (c.email && c.email.toLowerCase().includes(q))
    )
  }
  return list
})

const getClientOrdersCount = (clientId) => {
  return workerStore.orders.filter(o => o.userId === clientId || o.user?.id === clientId).length
}

const selectedClientOrders = computed(() => {
  if (!selectedClient.value) return []
  return workerStore.orders.filter(o => o.userId === selectedClient.value.id || o.user?.id === selectedClient.value.id)
})

const openClientHistoryModal = (client) => {
  selectedClient.value = client
  if (!modalInstance && window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(historyModalRef.value)
  }
  if (modalInstance) {
    modalInstance.show()
  }
}

const viewOrderFromHistory = (order) => {
  alert(`Pedido #${order.orderNumber}\nTotal: Bs. ${order.total.toFixed(2)}\nNotas: ${order.notes || 'Sin notas'}`)
}

const getOrderStatusBadgeClass = (status) => {
  const map = {
    pending: 'badge-amarillo',
    preparing: 'badge-azul',
    ready: 'badge-cafe',
    delivered: 'badge-verde'
  }
  return map[status] || 'badge-gris'
}

const getOrderStatusLabel = (status) => {
  const map = {
    pending: 'Pendiente',
    preparing: 'En Preparación',
    ready: 'Listo para Recoger',
    delivered: 'Entregado'
  }
  return map[status] || status
}

const formatDate = (dateStr) => {
  if (!dateStr) return 'N/A'
  const d = new Date(dateStr)
  return d.toLocaleDateString()
}
</script>

<style scoped>
.badge-azul {
  background-color: var(--color-terracota, #c85a32);
  color: #fff;
}
</style>
