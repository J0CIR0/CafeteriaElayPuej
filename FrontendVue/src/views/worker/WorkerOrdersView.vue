<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Gestión de Pedidos
          </span>
        </h2>
      </div>
      <div class="d-flex align-items-center gap-2">
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="refreshOrders" :disabled="workerStore.loading">
          <span v-if="workerStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Actualizar Lista</span>
        </button>
      </div>
    </div>

    <!-- Filter Buttons -->
    <div class="row g-2 mb-3">
      <div class="col-6 col-md-2">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'all' ? 'btn-dark' : 'btn-outline-dark'" @click="filterStatus = 'all'">
          Todos ({{ workerStore.orders.length }})
        </button>
      </div>
      <div class="col-6 col-md-2">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'pending' ? 'btn-warning' : 'btn-outline-warning'" @click="filterStatus = 'pending'">
          Pendientes de Pago ({{ workerStore.pendingPaymentOrders.length }})
        </button>
      </div>
      <div class="col-6 col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'preparing' ? 'btn-primary' : 'btn-outline-primary'" @click="filterStatus = 'preparing'">
          Pagados / En Preparación ({{ workerStore.preparingOrders.length }})
        </button>
      </div>
      <div class="col-6 col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'ready' ? 'btn-info text-white' : 'btn-outline-info'" @click="filterStatus = 'ready'">
          Listos para Recoger ({{ workerStore.readyOrders.length }})
        </button>
      </div>
      <div class="col-6 col-md-2">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'delivered' ? 'btn-success' : 'btn-outline-success'" @click="filterStatus = 'delivered'">
          Entregados ({{ workerStore.deliveredOrders.length }})
        </button>
      </div>
    </div>

    <!-- Search Input -->
    <div class="card mb-4 border-0 shadow-sm">
      <div class="card-body p-2">
        <div class="input-group">
          <span class="input-group-text bg-white border-0"><i class="bi bi-search"></i></span>
          <input type="text" class="form-control border-0" placeholder="Buscar por número de orden o nombre de cliente..." v-model="searchQuery">
          <button v-if="searchQuery" class="btn btn-link text-muted" @click="searchQuery = ''">Limpiar</button>
        </div>
      </div>
    </div>

    <!-- Orders Table -->
    <div class="admin-card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-modern align-middle mb-0">
            <thead>
              <tr>
                <th>Número de Orden</th>
                <th>Cliente</th>
                <th>Total</th>
                <th>Método Pago</th>
                <th>Estado Pago</th>
                <th>Estado Pedido</th>
                <th>Fecha</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filteredOrders.length === 0">
                <td colspan="8" class="text-center py-4 text-muted">No se encontraron pedidos con el criterio seleccionado.</td>
              </tr>
              <tr v-for="order in filteredOrders" :key="order.id">
                <td class="fw-bold">#{{ order.orderNumber }}</td>
                <td>
                  <div class="fw-bold">{{ order.user?.fullName || 'Cliente General' }}</div>
                  <small class="text-muted">{{ order.user?.email || 'N/A' }}</small>
                </td>
                <td class="fw-bold text-success">Bs. {{ order.total.toFixed(2) }}</td>
                <td>
                  <span class="badge badge-modern" :class="order.paymentMethod?.toLowerCase() === 'qr' ? 'badge-azul' : 'badge-cafe'">
                    {{ formatPaymentMethod(order.paymentMethod) }}
                  </span>
                </td>
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
                <td>{{ formatDate(order.createdAt) }}</td>
                <td>
                  <div class="d-flex align-items-center gap-1">
                    <button v-if="order.paymentStatus === 'pending'" class="btn btn-sm btn-warning me-1 fw-semibold" @click="confirmStatusChange(order, 'paid', 'Marcar como Pagado')">
                      <i class="bi bi-cash-stack me-1"></i>Marcar Pagado
                    </button>
                    <button v-else-if="order.paymentStatus === 'paid' && order.orderStatus === 'preparing'" class="btn btn-sm btn-primary me-1 fw-semibold" @click="confirmStatusChange(order, 'ready', 'Marcar como Listo')">
                      <i class="bi bi-box-seam me-1"></i>Marcar Listo
                    </button>
                    <button v-else-if="order.paymentStatus === 'paid' && order.orderStatus === 'ready'" class="btn btn-sm btn-success me-1 fw-semibold" @click="confirmStatusChange(order, 'delivered', 'Marcar como Entregado')">
                      <i class="bi bi-check-circle-fill me-1"></i>Marcar Entregado
                    </button>

                    <button class="btn btn-sm btn-outline-secondary btn-action-icon" @click="openDetailModal(order)" title="Ver Detalle del Pedido">
                      <i class="bi bi-eye-fill"></i>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Order Detail Modal -->
    <div class="modal fade" id="orderDetailModal" tabindex="-1" aria-hidden="true" ref="detailModalRef">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content modal-modern" v-if="selectedOrder">
          <div class="modal-header">
            <h5 class="modal-title text-white">Detalle de Pedido #{{ selectedOrder.orderNumber }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <!-- Client Info & Order Metadata -->
            <div class="row g-3 mb-4">
              <div class="col-md-6">
                <div class="card h-100 border-0 bg-light p-3">
                  <h6 class="fw-bold text-dark mb-2">Información del Cliente</h6>
                  <div><strong>Nombre:</strong> {{ selectedOrder.user?.fullName || 'N/A' }}</div>
                  <div><strong>Email:</strong> {{ selectedOrder.user?.email || 'N/A' }}</div>
                  <div><strong>Teléfono:</strong> {{ selectedOrder.user?.phone || 'Sin teléfono' }}</div>
                </div>
              </div>
              <div class="col-md-6">
                <div class="card h-100 border-0 bg-light p-3">
                  <h6 class="fw-bold text-dark mb-2">Información del Pedido</h6>
                  <div><strong>Fecha:</strong> {{ formatDate(selectedOrder.createdAt) }}</div>
                  <div><strong>Método de Pago:</strong> {{ formatPaymentMethod(selectedOrder.paymentMethod) }}</div>
                  <div><strong>Estado Pago:</strong> {{ selectedOrder.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}</div>
                  <div><strong>Estado Pedido:</strong> {{ getOrderStatusLabel(selectedOrder.orderStatus) }}</div>
                  <div v-if="selectedOrder.pickupTime"><strong>Horario de Recojo:</strong> {{ selectedOrder.pickupTime }}</div>
                </div>
              </div>
            </div>

            <!-- Notes if existing -->
            <div v-if="selectedOrder.notes" class="alert alert-info py-2 mb-3">
              <strong>Notas del cliente:</strong> {{ selectedOrder.notes }}
            </div>

            <!-- Products List Table -->
            <h6 class="fw-bold text-dark mb-2">Productos Solicitados</h6>
            <div class="table-responsive mb-3">
              <table class="table table-bordered table-sm align-middle">
                <thead class="table-light">
                  <tr>
                    <th>Producto</th>
                    <th class="text-center">Cantidad</th>
                    <th class="text-end">Precio Unitario</th>
                    <th class="text-end">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in selectedOrder.orderDetails" :key="item.id">
                    <td>{{ item.product?.name || 'Producto #' + item.productId }}</td>
                    <td class="text-center">{{ item.quantity }}</td>
                    <td class="text-end">Bs. {{ Number(item.unitPrice).toFixed(2) }}</td>
                    <td class="text-end fw-bold">Bs. {{ Number(item.subtotal).toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Totals breakdown -->
            <div class="d-flex flex-column align-items-end pe-2">
              <div class="fs-5 text-success mt-1"><span>Total Pedido:</span> <strong class="ms-2">Bs. {{ Number(selectedOrder.total).toFixed(2) }}</strong></div>
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
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useWorkerStore } from '../../stores/worker'

const workerStore = useWorkerStore()
const filterStatus = ref('all')
const searchQuery = ref('')
const selectedOrder = ref(null)
const detailModalRef = ref(null)
let modalInstance = null
let intervalId = null

onMounted(async () => {
  await workerStore.fetchOrders()
  if (window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(detailModalRef.value)
  }
  intervalId = setInterval(() => {
    workerStore.fetchOrders()
  }, 30000)
})

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId)
})

const refreshOrders = async () => {
  await workerStore.fetchOrders()
}

const filteredOrders = computed(() => {
  let list = workerStore.orders

  if (filterStatus.value === 'pending') {
    list = list.filter(o => o.paymentStatus === 'pending')
  } else if (filterStatus.value === 'preparing') {
    list = list.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'preparing')
  } else if (filterStatus.value === 'ready') {
    list = list.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'ready')
  } else if (filterStatus.value === 'delivered') {
    list = list.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'delivered')
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim()
    list = list.filter(o => 
      (o.orderNumber && o.orderNumber.toLowerCase().includes(q)) ||
      (o.user?.fullName && o.user.fullName.toLowerCase().includes(q))
    )
  }

  return list
})

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
  return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

const formatPaymentMethod = (pm) => {
  if (!pm) return 'EFECTIVO'
  const lower = pm.toLowerCase()
  if (lower === 'cash' || lower === 'efectivo') return 'EFECTIVO'
  if (lower === 'qr') return 'QR'
  return pm.toUpperCase()
}

const confirmStatusChange = async (order, targetState, actionLabel) => {
  if (!confirm(`¿Confirmar acción "${actionLabel}" para la orden #${order.orderNumber}?`)) {
    return
  }

  let res
  if (targetState === 'paid') {
    res = await workerStore.updateOrderPaymentStatus(order.id, 'paid')
  } else {
    res = await workerStore.updateOrderStatus(order.id, targetState)
  }

  if (!res.success) {
    alert(res.message)
  }
}

const openDetailModal = (order) => {
  selectedOrder.value = order
  if (!modalInstance && window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(detailModalRef.value)
  }
  if (modalInstance) {
    modalInstance.show()
  }
}
</script>

<style scoped>
.badge-azul {
  background-color: var(--color-terracota, #c85a32);
  color: #fff;
}
</style>
