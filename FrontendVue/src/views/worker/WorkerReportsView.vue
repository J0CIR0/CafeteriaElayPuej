<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header (Hidden on print) -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2 no-print">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Reportes Operativos del Día
          </span>
        </h2>
        <p style="color:white">
          Resumen de ventas, productos más populares e ingresos por método de pago
        </p>
      </div>
      <div class="d-flex align-items-center gap-2">
        <button class="btn btn-outline-secondary btn-sm d-flex align-items-center gap-1" @click="printReport">
          <i class="bi bi-printer"></i>
          <span>Imprimir Reporte</span>
        </button>
        <button class="btn btn-primary btn-sm d-flex align-items-center gap-1" @click="exportToPdf">
          <i class="bi bi-file-earmark-pdf"></i>
          <span>Exportar a PDF</span>
        </button>
      </div>
    </div>

    <!-- Print Header (Visible only on print) -->
    <div class="d-none d-print-block mb-4 text-center">
      <h2 class="fw-bold m-0">Cafetería Elay Puej</h2>
      <h4 class="text-muted">Reporte Operativo de Ventas del Día</h4>
      <p class="small text-muted">Fecha de Generación: {{ currentFormattedDate }}</p>
      <hr>
    </div>

    <!-- Summary Income Section -->
    <div class="row g-3 mb-4">
      <div class="col-md-4">
        <div class="stat-card green h-100">
          <div class="stat-label">Total Ingresos Efectivo</div>
          <div class="stat-number text-success">Bs. {{ workerStore.todayCashRevenue.toFixed(2) }}</div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="stat-card blue h-100">
          <div class="stat-label">Total Ingresos QR</div>
          <div class="stat-number text-primary">Bs. {{ workerStore.todayQrRevenue.toFixed(2) }}</div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="stat-card green h-100">
          <div class="stat-label">Total Ingresos General</div>
          <div class="stat-number text-success fw-bold">Bs. {{ workerStore.todayRevenue.toFixed(2) }}</div>
        </div>
      </div>
    </div>

    <!-- Report Tabs / Grid -->
    <div class="row g-4 mb-4">
      <!-- Top 10 Best Selling Products -->
      <div class="col-lg-6">
        <div class="admin-card h-100">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Top 10 Productos Más Vendidos del Día
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Producto</th>
                    <th class="text-center">Unidades Vendidas</th>
                    <th class="text-end">Monto Total</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="topProducts.length === 0">
                    <td colspan="4" class="text-center py-4 text-muted">No se registran ventas de productos hoy.</td>
                  </tr>
                  <tr v-for="(prod, idx) in topProducts" :key="prod.id">
                    <td class="fw-bold">{{ idx + 1 }}</td>
                    <td>{{ prod.name }}</td>
                    <td class="text-center fw-bold">{{ prod.quantitySold }}</td>
                    <td class="text-end text-success fw-bold">Bs. {{ prod.totalRevenue.toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Orders of the Day -->
      <div class="col-lg-6">
        <div class="admin-card h-100">
          <div class="card-header d-flex justify-content-between align-items-center" style="color: var(--color-cafe-tostado);">
            <span class="fw-bold">Pedidos del Día</span>
            <span class="badge bg-success">Total: {{ workerStore.todayOrders.length }}</span>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Orden</th>
                    <th>Cliente</th>
                    <th>Método</th>
                    <th>Estado</th>
                    <th class="text-end">Total</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="workerStore.todayOrders.length === 0">
                    <td colspan="5" class="text-center py-4 text-muted">No hay pedidos registrados hoy.</td>
                  </tr>
                  <tr v-for="order in workerStore.todayOrders" :key="order.id">
                    <td class="fw-bold">#{{ order.orderNumber }}</td>
                    <td>{{ order.user?.fullName || 'Cliente General' }}</td>
                    <td>{{ formatPaymentMethod(order.paymentMethod) }}</td>
                    <td>
                      <span class="badge badge-modern" :class="order.paymentStatus === 'paid' ? 'badge-verde' : 'badge-amarillo'">
                        {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}
                      </span>
                    </td>
                    <td class="text-end fw-bold text-success">Bs. {{ order.total.toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useWorkerStore } from '../../stores/worker'

const workerStore = useWorkerStore()

onMounted(async () => {
  await Promise.all([
    workerStore.fetchOrders(),
    workerStore.fetchProducts()
  ])
})

const currentFormattedDate = computed(() => {
  return new Date().toLocaleDateString('es-ES', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
})

const topProducts = computed(() => {
  const map = {}

  workerStore.todayOrders.forEach(order => {
    if (order.orderDetails && Array.isArray(order.orderDetails)) {
      order.orderDetails.forEach(item => {
        const pId = item.productId
        const name = item.product?.name || `Producto #${pId}`
        const qty = item.quantity || 0
        const subtotal = Number(item.subtotal) || 0

        if (!map[pId]) {
          map[pId] = { id: pId, name, quantitySold: 0, totalRevenue: 0 }
        }
        map[pId].quantitySold += qty
        map[pId].totalRevenue += subtotal
      })
    }
  })

  return Object.values(map)
    .sort((a, b) => b.quantitySold - a.quantitySold)
    .slice(0, 10)
})

const printReport = () => {
  window.print()
}

const exportToPdf = () => {
  window.print()
}

const formatPaymentMethod = (pm) => {
  if (!pm) return 'EFECTIVO'
  const lower = pm.toLowerCase()
  if (lower === 'cash' || lower === 'efectivo') return 'EFECTIVO'
  if (lower === 'qr') return 'QR'
  return pm.toUpperCase()
}
</script>

<style scoped>
@media print {
  .no-print {
    display: none !important;
  }
}
</style>
