<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2 class="fw-bold" style="color: var(--color-cafe);">
        <span style="border-left: 4px solid var(--color-cafe); padding-left: 12px;">Dashboard Administrativo</span>
      </h2>
      <span class="text-muted">{{ new Date().toLocaleDateString('es-ES', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</span>
    </div>

    <div v-if="adminStore.loading" class="text-center py-5">
      <div class="spinner-border text-cafe" role="status">
        <span class="visually-hidden">Cargando...</span>
      </div>
      <p class="mt-2 text-muted">Cargando datos del sistema...</p>
    </div>

    <div v-else>
      <StatsCards :stats="adminStore.statistics" />

      <div class="row g-4 mb-4">
        <div class="col-md-8">
          <Charts :orders="adminStore.orders" />
        </div>
        <div class="col-md-4">
          <StockAlerts @update-stock="openStockUpdate" />
        </div>
      </div>

      <div class="row g-4">
        <div class="col-md-6">
          <div class="admin-card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <span>Ultimos Productos</span>
              <router-link to="/admin/products" class="btn btn-sm btn-cafe-outline">Ver todos</router-link>
            </div>
            <div class="card-body p-0">
              <div v-if="adminStore.products.length === 0" class="text-center text-muted py-3">
                No hay productos registrados
              </div>
              <div v-else>
                <div v-for="product in adminStore.products.slice(0, 5)" :key="product.id" class="d-flex justify-content-between align-items-center border-bottom px-3 py-2">
                  <div>
                    <span class="fw-bold">{{ product.name }}</span>
                    <span class="text-muted ms-2">({{ product.categoryName || 'Sin categoria' }})</span>
                  </div>
                  <div>
                    <span class="text-success">${{ product.price.toFixed(2) }}</span>
                    <span class="badge bg-secondary ms-2">Stock: {{ product.stock }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-6">
          <div class="admin-card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <span>Ultimos Pedidos</span>
              <router-link to="/admin/orders" class="btn btn-sm btn-cafe-outline">Ver todos</router-link>
            </div>
            <div class="card-body p-0">
              <div v-if="adminStore.orders.length === 0" class="text-center text-muted py-3">
                No hay pedidos registrados
              </div>
              <div v-else>
                <div v-for="order in adminStore.orders.slice(0, 5)" :key="order.id" class="d-flex justify-content-between align-items-center border-bottom px-3 py-2">
                  <div>
                    <span class="fw-bold">#{{ order.orderNumber }}</span>
                    <span class="text-muted ms-2">{{ order.user?.fullName || 'N/A' }}</span>
                  </div>
                  <div>
                    <span class="text-success">${{ order.total.toFixed(2) }}</span>
                    <span class="badge ms-2" :class="order.paymentStatus === 'paid' ? 'bg-success' : 'bg-warning'">
                      {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="row mt-4">
        <div class="col-12">
          <ReportGenerator />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useAdminStore } from '../../stores/admin'
import StatsCards from '../../components/admin/StatsCards.vue'
import Charts from '../../components/admin/Charts.vue'
import StockAlerts from '../../components/admin/StockAlerts.vue'
import ReportGenerator from '../../components/admin/ReportGenerator.vue'

const adminStore = useAdminStore()

onMounted(async () => {
  await adminStore.fetchDashboardData()
})

const openStockUpdate = (product) => {
  console.log('Ajustar stock para:', product)
}
</script>