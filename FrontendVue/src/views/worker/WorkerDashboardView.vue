<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Panel Operativo del Trabajador
          </span>
        </h2>
        <p style="color:white">
          Gestión en tiempo real de pedidos, atención a clientes y control de inventario
        </p>
      </div>
      <div class="d-flex align-items-center gap-2">
        <span class="badge bg-light text-dark border p-2">
          <i class="bi bi-clock me-1"></i> Auto-refresco: 30s
        </span>
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="refreshData" :disabled="workerStore.loading">
          <span v-if="workerStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Refrescar</span>
        </button>
      </div>
    </div>

    <!-- Alert for new orders -->
    <div v-if="workerStore.hasNewOrdersAlert" class="alert alert-warning alert-dismissible fade show shadow-sm mb-4" role="alert">
      <div class="d-flex align-items-center">
        <strong class="me-2">¡Nuevos Pedidos Recibidos!</strong> Se han detectado nuevos pedidos en el sistema.
        <button class="btn btn-sm btn-dark ms-auto me-3" @click="clearAlertAndGo">Ver Pedidos</button>
      </div>
      <button type="button" class="btn-close" @click="workerStore.clearNewOrdersAlert()"></button>
    </div>

    <!-- Summary KPI Cards -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-md-4 col-xl-2">
        <div class="stat-card yellow h-100">
          <div class="stat-label">Pendientes de Pago</div>
          <div class="stat-number text-warning">{{ workerStore.pendingPaymentOrders.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-4 col-xl-2">
        <div class="stat-card blue h-100">
          <div class="stat-label">En Preparación</div>
          <div class="stat-number text-primary">{{ workerStore.preparingOrders.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-4 col-xl-2">
        <div class="stat-card green h-100">
          <div class="stat-label">Listos para Recoger</div>
          <div class="stat-number text-info">{{ workerStore.readyOrders.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-4 col-xl-2">
        <div class="stat-card green h-100">
          <div class="stat-label">Entregados Hoy</div>
          <div class="stat-number text-success">{{ workerStore.todayDeliveredOrders.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-4 col-xl-2">
        <div class="stat-card blue h-100">
          <div class="stat-label">Total Pedidos Hoy</div>
          <div class="stat-number">{{ workerStore.todayOrders.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-4 col-xl-2">
        <div class="stat-card green h-100">
          <div class="stat-label">Ingresos del Día</div>
          <div class="stat-number text-success">Bs. {{ workerStore.todayRevenue.toFixed(2) }}</div>
        </div>
      </div>
    </div>

    <!-- Main Content Section: Recent Orders & Low Stock Alert -->
    <div class="row g-4">
      <!-- Recent Orders Table -->
      <div class="col-lg-8">
        <div class="admin-card h-100">
          <div class="card-header d-flex justify-content-between align-items-center">
            <span class="fw-bold" style="color: var(--color-cafe-tostado);">Pedidos Recientes</span>
            <router-link to="/dashboard/orders" class="btn btn-sm btn-primary">
              Ver Todos los Pedidos
            </router-link>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Orden</th>
                    <th>Cliente</th>
                    <th>Total</th>
                    <th>Método Pago</th>
                    <th>Pago</th>
                    <th>Estado</th>
                    <th>Acción</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="recentOrders.length === 0">
                    <td colspan="7" class="text-center py-4 text-muted">No hay pedidos registrados hoy.</td>
                  </tr>
                  <tr v-for="order in recentOrders" :key="order.id">
                    <td class="fw-bold">#{{ order.orderNumber }}</td>
                    <td>{{ order.user?.fullName || 'Cliente General' }}</td>
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
                    <td>
                      <button v-if="order.paymentStatus === 'pending'" class="btn btn-sm btn-warning" @click="confirmAction(order.id, 'paid', 'Marcar como Pagado')">
                        Pagar
                      </button>
                      <button v-else-if="order.paymentStatus === 'paid' && order.orderStatus === 'preparing'" class="btn btn-sm btn-primary" @click="confirmAction(order.id, 'ready', 'Marcar como Listo')">
                        Listo
                      </button>
                      <button v-else-if="order.paymentStatus === 'paid' && order.orderStatus === 'ready'" class="btn btn-sm btn-success" @click="confirmAction(order.id, 'delivered', 'Marcar como Entregado')">
                        Entregar
                      </button>
                      <span v-else class="text-muted small">Finalizado</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Low Stock Alert Box & Info -->
      <div class="col-lg-4">
        <div class="admin-card h-100">
          <div class="card-header d-flex justify-content-between align-items-center bg-danger text-white">
            <span class="fw-bold text-white">Alertas de Stock Bajo</span>
            <span class="badge bg-white text-danger fw-bold">{{ workerStore.lowStockProducts.length }}</span>
          </div>
          <div class="card-body">
            <div v-if="workerStore.lowStockProducts.length === 0" class="text-center py-4 text-muted">
              <i class="bi bi-check-circle-fill text-success fs-2 d-block mb-2"></i>
              Todos los productos tienen stock suficiente.
            </div>
            <div v-else class="list-group list-group-flush">
              <div v-for="product in workerStore.lowStockProducts.slice(0, 6)" :key="product.id" class="list-group-item d-flex justify-content-between align-items-center px-0 py-2 border-bottom">
                <div>
                  <div class="fw-bold text-dark">{{ product.name }}</div>
                  <small class="text-muted">Mínimo requerido: {{ product.minStock }}</small>
                </div>
                <span class="badge bg-danger rounded-pill fs-6 px-3 py-1">
                  Stock: {{ product.stock }}
                </span>
              </div>
            </div>
            <div class="mt-3 text-center">
              <router-link to="/dashboard/inventory" class="btn btn-sm btn-outline-danger w-100">
                Ver Todo el Inventario
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useWorkerStore } from '../../stores/worker'

const router = useRouter()
const workerStore = useWorkerStore()

let intervalId = null

const recentOrders = computed(() => {
  return [...workerStore.orders].slice(0, 8)
})

onMounted(async () => {
  await workerStore.fetchDashboardData()
  intervalId = setInterval(() => {
    workerStore.fetchOrders()
  }, 30000)
})

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId)
})

const refreshData = async () => {
  await workerStore.fetchDashboardData()
}

const clearAlertAndGo = () => {
  workerStore.clearNewOrdersAlert()
  router.push('/dashboard/orders')
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

const formatPaymentMethod = (pm) => {
  if (!pm) return 'EFECTIVO'
  const lower = pm.toLowerCase()
  if (lower === 'cash' || lower === 'efectivo') return 'EFECTIVO'
  if (lower === 'qr') return 'QR'
  return pm.toUpperCase()
}

const confirmAction = async (orderId, targetState, label) => {
  if (!confirm(`¿Está seguro de realizar la acción: "${label}" para la orden #${orderId}?`)) {
    return
  }

  if (targetState === 'paid') {
    const res = await workerStore.updateOrderPaymentStatus(orderId, 'paid')
    if (!res.success) alert(res.message)
  } else {
    const res = await workerStore.updateOrderStatus(orderId, targetState)
    if (!res.success) alert(res.message)
  }
}
</script>

<style scoped>
.badge-azul {
  background-color: var(--color-terracota, #c85a32);
  color: #fff;
}
</style>
