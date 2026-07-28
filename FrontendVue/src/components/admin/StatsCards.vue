<template>
  <div class="row g-4 mb-4">
    <div class="col-md-3">
      <div class="stat-card">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">{{ stats.totalProducts }}</div>
            <div class="stat-label">Productos</div>
          </div>
          <div class="stat-icon">P</div>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="stat-card green">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">{{ stats.totalOrders }}</div>
            <div class="stat-label">Pedidos</div>
          </div>
          <div class="stat-icon">O</div>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="stat-card yellow">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">${{ stats.totalRevenue.toFixed(2) }}</div>
            <div class="stat-label">Ingresos</div>
          </div>
          <div class="stat-icon">R</div>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="stat-card" :class="stats.lowStockProducts > 0 ? 'red' : 'green'">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">{{ stats.lowStockProducts }}</div>
            <div class="stat-label">Stock Bajo</div>
          </div>
          <div class="stat-icon">A</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  stats: {
    type: Object,
    required: true
  }
})
</script>