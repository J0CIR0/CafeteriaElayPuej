<template>
  <div class="container-fluid admin-page px-3 px-lg-4">
    <section class="admin-hero admin-card mb-4 p-4 p-lg-5">
      <div class="row align-items-end g-4">
        <div class="col-lg-8">
          <span class="eyebrow-chip">Casona cruceña • café tradicional • gestión viva</span>
          <h1 class="admin-title mt-3 mb-2">¿Qué se te antoja, pariente?</h1>
          <p class="admin-lead mb-0">
            Administrá el horneao, los masacos, los refrescos y el personal desde un panel cálido, simple y bien camba.
            Todo sale directo del backend y se refleja al instante en el menú público.
          </p>
        </div>
        <div class="col-lg-4 text-lg-end">
          <div class="d-inline-flex flex-column gap-2 align-items-lg-end">
            <span class="badge-modern badge-cafe">Un cafecito pa' la tarde</span>
            <span class="text-muted small">{{ currentDateLabel }}</span>
          </div>
        </div>
      </div>
    </section>

    <div class="row g-3 mb-4">
      <div class="col-md-3" v-for="stat in statsCards" :key="stat.label">
        <div class="stat-card" :class="stat.variant">
          <div class="stat-label">{{ stat.label }}</div>
          <div class="stat-number">{{ stat.value }}</div>
          <div class="text-muted small mt-1">{{ stat.helper }}</div>
        </div>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-7">
        <div class="admin-card h-100">
          <div class="card-header d-flex justify-content-between align-items-center">
            <span>Menú de la casa</span>
            <router-link to="/products" class="btn btn-sm btn-primary-outline">Ver menú público</router-link>
          </div>
          <div class="card-body">
            <div class="row g-3">
              <div v-for="product in adminStore.products.slice(0, 6)" :key="product.id" class="col-md-6">
                <div class="menu-mini-card">
                  <div class="d-flex justify-content-between align-items-start gap-3">
                    <div>
                      <div class="menu-mini-badge">{{ product.categoryName }}</div>
                      <h3 class="menu-mini-title mb-1">{{ product.name }}</h3>
                      <p class="menu-mini-text mb-2">{{ product.description }}</p>
                    </div>
                    <div class="text-end">
                      <div class="price-tag">{{ formatBs(product.price) }}</div>
                      <div class="small text-muted">Stock {{ product.stock }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-5">
        <div class="admin-card h-100">
          <div class="card-header">Atajos del mostrador</div>
          <div class="card-body d-grid gap-3">
            <router-link to="/admin/products" class="btn btn-primary">Gestionar productos</router-link>
            <router-link to="/admin/insumos" class="btn btn-primary">Insumos, Recetas y Vencimientos</router-link>
            <router-link to="/admin/finanzas" class="btn btn-success">Panel Financiero y Pérdidas</router-link>
            <router-link to="/admin/categories" class="btn btn-primary-outline">Gestionar categorías</router-link>
            <router-link to="/admin/users" class="btn btn-primary-outline">Gestionar personal</router-link>
            <router-link to="/admin/orders" class="btn btn-primary-outline">Gestionar pedidos</router-link>
            <div class="tip-box">
              <strong>Tip camba:</strong> al registrar ventas se descuenta automáticamente el stock de insumos según el recetario.
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import { formatBolivianos as formatBs } from '../../stores/cafeteriaData'

const adminStore = useAdminStore()

const currentDateLabel = computed(() => new Date().toLocaleDateString('es-ES', {
  weekday: 'long',
  year: 'numeric',
  month: 'long',
  day: 'numeric'
}))

const statsCards = computed(() => [
  {
    label: 'Productos',
    value: adminStore.statistics.totalProducts,
    helper: 'Sabores listos para vender',
    variant: 'green'
  },
  {
    label: 'Usuarios',
    value: adminStore.statistics.totalUsers,
    helper: 'Personal y clientes registrados',
    variant: 'blue'
  },
  {
    label: 'Pedidos',
    value: adminStore.statistics.totalOrders,
    helper: 'Seguimiento del mostrador',
    variant: 'yellow'
  },
  {
    label: 'Ingresos',
    value: formatBs(adminStore.statistics.totalRevenue),
    helper: 'Acumulado en bolivianos',
    variant: 'red'
  }
])

onMounted(async () => {
  await adminStore.fetchDashboardData()
})
</script>
