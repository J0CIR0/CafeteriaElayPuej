<template>
  <div class="container-fluid mt-4">
    <div class="row">
      <div class="col-md-3 col-lg-2">
        <AdminSidebar />
      </div>
      <div class="col-md-9 col-lg-10">
        <h2 class="mb-4">Dashboard Administrativo</h2>
        
        <div v-if="adminStore.loading" class="text-center">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Cargando...</span>
          </div>
        </div>
        
        <div v-else>
          <div class="row">
            <div class="col-md-3">
              <div class="card bg-primary text-white mb-3">
                <div class="card-body">
                  <h5 class="card-title">Productos</h5>
                  <h2 class="display-6">{{ adminStore.statistics.totalProducts }}</h2>
                  <small>Activos</small>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="card bg-success text-white mb-3">
                <div class="card-body">
                  <h5 class="card-title">Usuarios</h5>
                  <h2 class="display-6">{{ adminStore.statistics.totalUsers }}</h2>
                  <small>Registrados</small>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="card bg-info text-white mb-3">
                <div class="card-body">
                  <h5 class="card-title">Pedidos</h5>
                  <h2 class="display-6">{{ adminStore.statistics.totalOrders }}</h2>
                  <small>Pendientes: {{ adminStore.statistics.pendingOrders }}</small>
                </div>
              </div>
            </div>
            <div class="col-md-3">
              <div class="card bg-warning text-dark mb-3">
                <div class="card-body">
                  <h5 class="card-title">Ingresos</h5>
                  <h2 class="display-6">${{ adminStore.statistics.totalRevenue.toFixed(2) }}</h2>
                  <small>Total</small>
                </div>
              </div>
            </div>
          </div>
          
          <div class="row mt-4">
            <div class="col-md-6">
              <div class="card">
                <div class="card-header">
                  <h5>Pedidos Recientes</h5>
                </div>
                <div class="card-body">
                  <div v-if="adminStore.orders.length === 0" class="text-center text-muted">
                    No hay pedidos
                  </div>
                  <div v-else>
                    <div v-for="order in adminStore.orders.slice(0, 5)" :key="order.id" class="border-bottom py-2">
                      <div class="d-flex justify-content-between">
                        <span>#{{ order.orderNumber }}</span>
                        <span>${{ order.total.toFixed(2) }}</span>
                        <span class="badge" :class="order.paymentStatus === 'paid' ? 'bg-success' : 'bg-warning'">
                          {{ order.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente' }}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-md-6">
              <div class="card">
                <div class="card-header">
                  <h5>Stock Bajo</h5>
                </div>
                <div class="card-body">
                  <div v-if="adminStore.products.filter(p => p.stock <= p.minStock).length === 0" class="text-center text-muted">
                    Todos los productos tienen stock suficiente
                  </div>
                  <div v-else>
                    <div v-for="product in adminStore.products.filter(p => p.stock <= p.minStock).slice(0, 5)" :key="product.id" class="border-bottom py-2">
                      <div class="d-flex justify-content-between">
                        <span>{{ product.name }}</span>
                        <span class="text-danger">Stock: {{ product.stock }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <div class="row mt-4">
            <div class="col-md-4">
              <div class="card">
                <div class="card-body text-center">
                  <h6>Ingresos Hoy</h6>
                  <h3>${{ adminStore.getRevenueToday.toFixed(2) }}</h3>
                </div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="card">
                <div class="card-body text-center">
                  <h6>Ingresos Esta Semana</h6>
                  <h3>${{ adminStore.getRevenueThisWeek.toFixed(2) }}</h3>
                </div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="card">
                <div class="card-body text-center">
                  <h6>Ingresos Este Mes</h6>
                  <h3>${{ adminStore.getRevenueThisMonth.toFixed(2) }}</h3>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import AdminSidebar from '../../components/admin/AdminSidebar.vue'

const adminStore = useAdminStore()

onMounted(async () => {
  await adminStore.fetchDashboardData()
})
</script>