<template>
  <div class="container-fluid px-3 px-lg-4 py-3">
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Mis Pedidos
          </span>
        </h2>
        <p class="text-muted small m-0 mt-1 ms-3">
          Historial y estado de tus órdenes en tiempo real
        </p>
      </div>
      <div>
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="refreshOrders" :disabled="ordersStore.loading">
          <span v-if="ordersStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Actualizar Lista</span>
        </button>
      </div>
    </div>

    <div class="row g-2 mb-4">
      <div class="col-6 col-md-2">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'all' ? 'btn-dark' : 'btn-outline-dark'" @click="filterStatus = 'all'">
          Todos ({{ ordersStore.myOrders.length }})
        </button>
      </div>
      <div class="col-6 col-md-2">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'pending' ? 'btn-warning' : 'btn-outline-warning'" @click="filterStatus = 'pending'">
          Pendientes Pago ({{ pendingOrdersCount }})
        </button>
      </div>
      <div class="col-6 col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'preparing' ? 'btn-primary' : 'btn-outline-primary'" @click="filterStatus = 'preparing'">
          En Preparación ({{ preparingOrdersCount }})
        </button>
      </div>
      <div class="col-6 col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'ready' ? 'btn-info text-white' : 'btn-outline-info'" @click="filterStatus = 'ready'">
          Listos para Recoger ({{ readyOrdersCount }})
        </button>
      </div>
      <div class="col-6 col-md-2">
        <button class="btn w-100 btn-sm text-nowrap" :class="filterStatus === 'delivered' ? 'btn-success' : 'btn-outline-success'" @click="filterStatus = 'delivered'">
          Entregados ({{ deliveredOrdersCount }})
        </button>
      </div>
    </div>

    <div v-if="ordersStore.loading && ordersStore.myOrders.length === 0" class="text-center py-5">
      <div class="spinner-border text-success" role="status">
        <span class="visually-hidden">Cargando tus pedidos...</span>
      </div>
    </div>

    <div v-else-if="filteredOrders.length === 0" class="admin-card text-center py-5">
      <div class="card-body">
        <i class="bi bi-receipt-cutoff text-muted display-1 d-block mb-3"></i>
        <h4 class="fw-bold" style="color: var(--color-cafe-tostado);">No tienes pedidos registrados en esta categoría</h4>
        <p class="text-muted mb-4">¿Se te antoja un cafecito o un horneao tradicional camba?</p>
        <router-link to="/products" class="btn btn-primary btn-lg">
          <i class="bi bi-cup-hot me-2"></i>Ir al Menú y Hacer un Pedido
        </router-link>
      </div>
    </div>

    <div v-else class="row g-4">
      <div v-for="order in filteredOrders" :key="order.id" class="col-12">
        <div class="admin-card shadow-sm border">
          <div class="card-header d-flex flex-wrap justify-content-between align-items-center gap-2 py-3">
            <div>
              <span class="fw-bold fs-5 me-2" style="color: var(--color-cafe-tostado);">Pedido #{{ order.orderNumber }}</span>
              <span class="badge badge-modern me-2" :class="order.paymentStatus === 'paid' ? 'badge-verde' : 'badge-amarillo'">
                {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente de Pago' }}
              </span>
              <span class="badge badge-modern" :class="getOrderStatusBadgeClass(order.orderStatus)">
                {{ getOrderStatusLabel(order.orderStatus) }}
              </span>
            </div>
            <div class="text-muted small">
              <i class="bi bi-calendar3 me-1"></i>{{ formatDate(order.createdAt) }}
            </div>
          </div>
          <div class="card-body">
            <div class="table-responsive mb-3">
              <table class="table table-sm align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Producto</th>
                    <th class="text-center">Cantidad</th>
                    <th class="text-end">Precio Unitario</th>
                    <th class="text-end">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="detail in order.orderDetails" :key="detail.id">
                    <td>{{ detail.product?.name || 'Producto #' + detail.productId }}</td>
                    <td class="text-center">{{ detail.quantity }}</td>
                    <td class="text-end">Bs. {{ Number(detail.unitPrice).toFixed(2) }}</td>
                    <td class="text-end fw-bold">Bs. {{ Number(detail.subtotal).toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div class="d-flex flex-wrap justify-content-between align-items-center gap-2">
              <div>
                <small v-if="order.notes" class="text-muted d-block"><strong>Notas:</strong> {{ order.notes }}</small>
                <small class="text-muted"><strong>Método de pago:</strong> {{ formatPaymentMethod(order.paymentMethod) }}</small>
              </div>
              <div class="d-flex align-items-center gap-3">
                <div class="fs-5 fw-bold text-success">
                  Total: Bs. {{ Number(order.total).toFixed(2) }}
                </div>
                <button class="btn btn-sm btn-outline-primary" @click="openDetailModal(order)">
                  Ver Detalle Completo
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="modal fade" id="customerOrderDetailModal" tabindex="-1" aria-hidden="true" ref="modalRef">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content modal-modern" v-if="selectedOrder">
          <div class="modal-header">
            <h5 class="modal-title text-white">Detalle de Pedido #{{ selectedOrder.orderNumber }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div class="row g-3 mb-4">
              <div class="col-md-6">
                <div class="card h-100 border-0 bg-light p-3">
                  <h6 class="fw-bold text-dark mb-2">Estado del Pedido</h6>
                  <div><strong>Estado Pago:</strong> {{ selectedOrder.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente de Pago' }}</div>
                  <div><strong>Estado Preparación:</strong> {{ getOrderStatusLabel(selectedOrder.orderStatus) }}</div>
                  <div><strong>Método de Pago:</strong> {{ formatPaymentMethod(selectedOrder.paymentMethod) }}</div>
                  <div v-if="selectedOrder.pickupTime"><strong>Horario de Recojo:</strong> {{ selectedOrder.pickupTime }}</div>
                </div>
              </div>
              <div class="col-md-6">
                <div class="card h-100 border-0 bg-light p-3">
                  <h6 class="fw-bold text-dark mb-2">Fechas y Registro</h6>
                  <div><strong>Fecha de Creación:</strong> {{ formatDate(selectedOrder.createdAt) }}</div>
                  <div><strong>Última Actualización:</strong> {{ formatDate(selectedOrder.updatedAt) }}</div>
                </div>
              </div>
            </div>

            <div v-if="selectedOrder.notes" class="alert alert-info py-2 mb-3">
              <strong>Notas de tu pedido:</strong> {{ selectedOrder.notes }}
            </div>

            <h6 class="fw-bold text-dark mb-2">Desglose de Productos</h6>
            <div class="table-responsive mb-3">
              <table class="table table-bordered table-sm align-middle mb-0">
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

            <div class="d-flex flex-column align-items-end pe-2">
              <div class="fs-4 text-success mt-1"><span>Total Pedido:</span> <strong class="ms-2">Bs. {{ Number(selectedOrder.total).toFixed(2) }}</strong></div>
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
import { useOrdersStore } from '../stores/orders'

const ordersStore = useOrdersStore()
const filterStatus = ref('all')
const selectedOrder = ref(null)
const modalRef = ref(null)
let modalInstance = null
let intervalId = null

onMounted(async () => {
  await ordersStore.fetchMyOrders()
  if (window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(modalRef.value)
  }
  intervalId = setInterval(() => {
    ordersStore.fetchMyOrders()
  }, 30000)
})

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId)
})

const refreshOrders = async () => {
  await ordersStore.fetchMyOrders()
}

const pendingOrdersCount = computed(() => ordersStore.myOrders.filter(o => o.paymentStatus === 'pending').length)
const preparingOrdersCount = computed(() => ordersStore.myOrders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'preparing').length)
const readyOrdersCount = computed(() => ordersStore.myOrders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'ready').length)
const deliveredOrdersCount = computed(() => ordersStore.myOrders.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'delivered').length)

const filteredOrders = computed(() => {
  let list = ordersStore.myOrders

  if (filterStatus.value === 'pending') {
    list = list.filter(o => o.paymentStatus === 'pending')
  } else if (filterStatus.value === 'preparing') {
    list = list.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'preparing')
  } else if (filterStatus.value === 'ready') {
    list = list.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'ready')
  } else if (filterStatus.value === 'delivered') {
    list = list.filter(o => o.paymentStatus === 'paid' && o.orderStatus === 'delivered')
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
    pending: 'Pendiente de Preparación',
    preparing: 'En Preparación',
    ready: 'Listo para Recoger en Caja',
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

const openDetailModal = (order) => {
  selectedOrder.value = order
  if (!modalInstance && window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(modalRef.value)
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