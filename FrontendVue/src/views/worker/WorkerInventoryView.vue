<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Control de Inventario
          </span>
        </h2>
        <p style="color:white">
          Monitoreo de niveles de existencias e historial de movimientos
        </p>
      </div>
      <div>
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="refreshInventory" :disabled="workerStore.loading">
          <span v-if="workerStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Actualizar Inventario</span>
        </button>
      </div>
    </div>

    <!-- Summary KPI Cards -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-md-3">
        <div class="stat-card blue h-100">
          <div class="stat-label">Productos Totales</div>
          <div class="stat-number">{{ workerStore.products.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="stat-card yellow h-100">
          <div class="stat-label">Productos Stock Bajo</div>
          <div class="stat-number text-warning">{{ workerStore.lowStockProducts.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="stat-card red h-100">
          <div class="stat-label">Productos Sin Stock</div>
          <div class="stat-number text-danger">{{ workerStore.outOfStockProducts.length }}</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="stat-card green h-100">
          <div class="stat-label">Stock Total de Unidades</div>
          <div class="stat-number text-success">{{ workerStore.totalStock }}</div>
        </div>
      </div>
    </div>

    <!-- Navigation Tabs (Stock Table vs Movement History) -->
    <ul class="nav nav-pills admin-pills mb-3" id="inventoryTabs" role="tablist">
      <li class="nav-item" role="presentation">
        <button class="nav-link active" id="stock-tab" data-bs-toggle="tab" data-bs-target="#stock-panel" type="button" role="tab">
          Estado de Existencias
        </button>
      </li>
      <li class="nav-item" role="presentation">
        <button class="nav-link" id="movements-tab" data-bs-toggle="tab" data-bs-target="#movements-panel" type="button" role="tab">
          Historial de Movimientos
        </button>
      </li>
    </ul>

    <!-- Tab Contents -->
    <div class="tab-content" id="inventoryTabContent">
      <!-- Stock Table Panel -->
      <div class="tab-pane fade show active" id="stock-panel" role="tabpanel">
        <div class="admin-card">
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Nombre del Producto</th>
                    <th class="text-center">Stock Actual</th>
                    <th class="text-center">Stock Mínimo</th>
                    <th class="text-center">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="workerStore.products.length === 0">
                    <td colspan="4" class="text-center py-4 text-muted">No hay productos disponibles.</td>
                  </tr>
                  <tr v-for="product in workerStore.products" :key="product.id">
                    <td class="fw-bold">{{ product.name }}</td>
                    <td class="text-center fw-bold">{{ product.stock }}</td>
                    <td class="text-center text-muted">{{ product.minStock || 0 }}</td>
                    <td class="text-center">
                      <span v-if="Number(product.stock) === 0" class="badge badge-modern badge-rojo">
                        Agotado
                      </span>
                      <span v-else-if="Number(product.stock) <= Number(product.minStock || 0)" class="badge badge-modern badge-amarillo">
                        Stock Bajo
                      </span>
                      <span v-else class="badge badge-modern badge-verde">
                        Normal
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Movement History Panel -->
      <div class="tab-pane fade" id="movements-panel" role="tabpanel">
        <div class="admin-card">
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Fecha</th>
                    <th>Producto</th>
                    <th>Tipo Movimiento</th>
                    <th class="text-center">Cantidad</th>
                    <th>Motivo / Referencia</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="workerStore.inventoryMovements.length === 0">
                    <td colspan="5" class="text-center py-4 text-muted">No hay registros de movimientos de inventario.</td>
                  </tr>
                  <tr v-for="movement in workerStore.inventoryMovements" :key="movement.id">
                    <td>{{ formatDate(movement.createdAt) }}</td>
                    <td class="fw-bold">{{ movement.product?.name || 'Producto #' + movement.productId }}</td>
                    <td>
                      <span class="badge badge-modern" :class="getMovementBadgeClass(movement.movementType)">
                        {{ getMovementLabel(movement.movementType) }}
                      </span>
                    </td>
                    <td class="text-center fw-bold">{{ movement.quantity }}</td>
                    <td class="text-muted small">{{ movement.reason || 'Sin motivo registrado' }}</td>
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
import { onMounted } from 'vue'
import { useWorkerStore } from '../../stores/worker'

const workerStore = useWorkerStore()

onMounted(async () => {
  await Promise.all([
    workerStore.fetchProducts(),
    workerStore.fetchInventoryMovements()
  ])
})

const refreshInventory = async () => {
  await Promise.all([
    workerStore.fetchProducts(),
    workerStore.fetchInventoryMovements()
  ])
}

const getMovementBadgeClass = (type) => {
  const t = (type || '').toLowerCase()
  if (t === 'entry' || t === 'entrada') return 'badge-verde'
  if (t === 'exit' || t === 'salida') return 'badge-rojo'
  return 'badge-amarillo'
}

const getMovementLabel = (type) => {
  const t = (type || '').toLowerCase()
  if (t === 'entry' || t === 'entrada') return 'Entrada (+)'
  if (t === 'exit' || t === 'salida') return 'Salida (-)'
  return 'Ajuste (=)'
}

const formatDate = (dateStr) => {
  if (!dateStr) return 'N/A'
  const d = new Date(dateStr)
  return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
</style>
