<template>
  <div class="row g-4 mb-4">
    <div class="col-md-3">
      <div class="stat-card shadow-sm">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">{{ stats.totalProducts }}</div>
            <div class="stat-label">Productos</div>
          </div>
          <div class="stat-icon"><i class="bi bi-box-seam"></i></div>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="stat-card green shadow-sm">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">{{ stats.totalOrders }}</div>
            <div class="stat-label">Pedidos</div>
          </div>
          <div class="stat-icon"><i class="bi bi-bag-check"></i></div>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="stat-card yellow shadow-sm">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">Bs. {{ Number(stats.totalRevenue || 0).toFixed(2) }}</div>
            <div class="stat-label">Ingresos</div>
          </div>
          <div class="stat-icon"><i class="bi bi-cash-stack"></i></div>
        </div>
      </div>
    </div>
    <div class="col-md-3">
      <div class="stat-card shadow-sm" :class="stats.lowStockProducts > 0 ? 'red' : 'green'">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <div class="stat-number">{{ stats.lowStockProducts }}</div>
            <div class="stat-label">Stock Bajo</div>
          </div>
          <div class="stat-icon"><i class="bi bi-exclamation-triangle"></i></div>
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