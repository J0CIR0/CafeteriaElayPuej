<template>
  <div class="container-fluid px-3 px-lg-4 py-3">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Monitor General de Pedidos
          </span>
        </h2>
        <p style="color:white">
          Supervisión global en tiempo real del flujo de pedidos de la cafetería
        </p>
      </div>
      <div>
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="adminStore.fetchOrders()" :disabled="adminStore.loading">
          <span v-if="adminStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Actualizar Lista</span>
        </button>
      </div>
    </div>

    <!-- Role Responsibilities Notice Banner -->
    <div class="alert alert-info d-flex align-items-center gap-3 mb-4 shadow-sm">
      <i class="bi bi-shield-lock-fill fs-3 text-info"></i>
      <div>
        <h6 class="fw-bold m-0 text-dark">Supervisión Operativa del Administrador</h6>
        <small class="text-muted">
          Los cambios de estado operacional (Confirmación de Pago, En Preparación, Listo para Recoger y Entregado) son procesados 
          <strong>exclusivamente por los Trabajadores</strong> desde el Panel de Trabajador.
        </small>
      </div>
    </div>

    <!-- Filters -->
    <div class="row g-2 mb-4">
      <div class="col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filter === 'all' ? 'btn-dark' : 'btn-outline-dark'" @click="filter = 'all'">
          Todos los Pedidos ({{ adminStore.orders.length }})
        </button>
      </div>
      <div class="col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filter === 'pending' ? 'btn-warning' : 'btn-outline-warning'" @click="filter = 'pending'">
          Pendientes de Pago ({{ pendingOrders.length }})
        </button>
      </div>
      <div class="col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filter === 'paid' ? 'btn-success' : 'btn-outline-success'" @click="filter = 'paid'">
          Pagados / En Proceso ({{ paidOrders.length }})
        </button>
      </div>
      <div class="col-md-3">
        <button class="btn w-100 btn-sm text-nowrap" :class="filter === 'delivered' ? 'btn-info text-white' : 'btn-outline-info'" @click="filter = 'delivered'">
          Entregados ({{ deliveredOrders.length }})
        </button>
      </div>
    </div>

    <!-- Orders Table -->
    <div class="admin-card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-modern align-middle mb-0">
            <thead>
              <tr>
                <th>Pedido</th>
                <th>Cliente</th>
                <th>Método</th>
                <th>Estado Pago</th>
                <th>Estado Orden</th>
                <th class="text-end">Total</th>
                <th>Fecha y Hora</th>
                <th class="text-center">Acción</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="order in filteredOrders" :key="order.id">
                <td class="fw-bold text-dark">#{{ order.orderNumber }}</td>
                <td>
                  <div class="fw-semibold">{{ order.user?.fullName || 'Cliente #' + order.userId }}</div>
                  <small class="text-muted" v-if="order.user?.email">{{ order.user.email }}</small>
                </td>
        <td class="text-uppercase small font-monospace fw-bold">{{ formatPaymentMethod(order.paymentMethod) }}</td>
                <td>
                  <span class="badge badge-modern" :class="order.paymentStatus === 'paid' ? 'badge-verde' : 'badge-amarillo'">
                    {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}
                  </span>
                </td>
                <td>
                  <span class="badge badge-modern" :class="getOrderStatusBadgeClass(order.orderStatus)">
                    {{ getOrderStatusText(order.orderStatus) }}
                  </span>
                </td>
                <td class="text-end fw-bold text-success">
                  Bs. {{ Number(order.total).toFixed(2) }}
                </td>
                <td class="small text-muted">{{ formatDate(order.createdAt) }}</td>
                <td class="text-center">
                  <button class="btn btn-sm btn-outline-primary btn-action-icon" @click="viewDetails(order)" title="Ver Detalle del Pedido">
                    <i class="bi bi-eye-fill"></i>
                  </button>
                </td>
              </tr>
              <tr v-if="filteredOrders.length === 0">
                <td colspan="8" class="text-center py-4 text-muted">
                  No hay pedidos registrados en esta categoría.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Order Detail Modal -->
    <div class="modal fade" id="adminOrderDetailModal" tabindex="-1" aria-hidden="true" ref="modalRef">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content modal-modern" v-if="selectedOrder">
          <div class="modal-header">
            <h5 class="modal-title text-white">Monitoreo de Pedido #{{ selectedOrder.orderNumber }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div class="row g-3 mb-3">
              <div class="col-md-6">
                <div class="card border-0 bg-light p-3">
                  <h6 class="fw-bold text-dark mb-2">Datos del Cliente</h6>
                  <div><strong>Nombre:</strong> {{ selectedOrder.user?.fullName || 'N/A' }}</div>
                  <div><strong>Email:</strong> {{ selectedOrder.user?.email || 'N/A' }}</div>
                  <div><strong>Teléfono:</strong> {{ selectedOrder.user?.phone || 'Sin registrar' }}</div>
                </div>
              </div>
              <div class="col-md-6">
                <div class="card border-0 bg-light p-3">
                  <h6 class="fw-bold text-dark mb-2">Estado del Pedido</h6>
                  <div><strong>Pago:</strong> {{ selectedOrder.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}</div>
                  <div><strong>Preparación:</strong> {{ getOrderStatusText(selectedOrder.orderStatus) }}</div>
                  <div><strong>Método de Pago:</strong> {{ formatPaymentMethod(selectedOrder.paymentMethod) }}</div>
                  <div><strong>Fecha:</strong> {{ formatDate(selectedOrder.createdAt) }}</div>
                </div>
              </div>
            </div>

            <div v-if="selectedOrder.notes" class="alert alert-info py-2 mb-3">
              <strong>Notas:</strong> {{ selectedOrder.notes }}
            </div>

            <h6 class="fw-bold text-dark mb-2">Ítems del Pedido</h6>
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
              <div class="fs-4 text-success mt-1">Total Pedido: <strong class="ms-2">Bs. {{ Number(selectedOrder.total).toFixed(2) }}</strong></div>
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
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()
const filter = ref('all')
const selectedOrder = ref(null)
const modalRef = ref(null)
let modalInstance = null

onMounted(async () => {
  await adminStore.fetchOrders()
  if (window.bootstrap && modalRef.value) {
    modalInstance = new window.bootstrap.Modal(modalRef.value)
  }
})

const pendingOrders = computed(() => adminStore.orders.filter(o => o.paymentStatus === 'pending'))
const paidOrders = computed(() => adminStore.orders.filter(o => o.paymentStatus === 'paid'))
const deliveredOrders = computed(() => adminStore.orders.filter(o => o.orderStatus === 'delivered'))

const filteredOrders = computed(() => {
  if (filter.value === 'pending') return pendingOrders.value
  if (filter.value === 'paid') return paidOrders.value
  if (filter.value === 'delivered') return deliveredOrders.value
  return adminStore.orders
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

const getOrderStatusText = (status) => {
  const map = {
    pending: 'Pendiente de Preparación',
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

const viewDetails = (order) => {
  selectedOrder.value = order
  if (!modalInstance && window.bootstrap && modalRef.value) {
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