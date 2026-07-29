<template>
  <div class="row g-4">
    <div class="col-md-6">
      <div class="admin-card">
        <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">Pedidos por Estado</div>
        <div class="card-body">
          <div v-if="orders.length === 0" class="text-center text-muted py-4">
            No hay datos disponibles
          </div>
          <div v-else>
            <div v-for="status in orderStatuses" :key="status.key" class="mb-2">
              <div class="d-flex justify-content-between">
                <span>{{ status.label }}</span>
                <span>{{ status.count }}</span>
              </div>
              <div class="progress" style="height: 8px;">
                <div class="progress-bar" :class="status.color" :style="{ width: status.percentage + '%' }"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="col-md-6">
      <div class="admin-card">
        <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">Ingresos por Periodo</div>
        <div class="card-body">
          <div v-if="!hasRevenueData" class="text-center text-muted py-4">
            No hay datos de ingresos
          </div>
          <div v-else>
            <div class="d-flex justify-content-around">
              <div class="text-center">
                <div class="stat-number" style="font-size: 1.1rem;">Bs. {{ revenueToday.toFixed(2) }}</div>
                <div class="stat-label">Hoy</div>
              </div>
              <div class="text-center">
                <div class="stat-number" style="font-size: 1.1rem;">Bs. {{ revenueWeek.toFixed(2) }}</div>
                <div class="stat-label">Esta Semana</div>
              </div>
              <div class="text-center">
                <div class="stat-number" style="font-size: 1.1rem;">Bs. {{ revenueMonth.toFixed(2) }}</div>
                <div class="stat-label">Este Mes</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  orders: {
    type: Array,
    required: true
  }
})

const orderStatuses = computed(() => {
  const statusMap = {
    pending: { label: 'Pendientes', color: 'bg-secondary' },
    preparing: { label: 'En Preparación', color: 'bg-info' },
    ready: { label: 'Listos para Recoger', color: 'bg-primary' },
    delivered: { label: 'Entregados', color: 'bg-success' }
  }
  
  const total = props.orders.length || 1
  return Object.keys(statusMap).map(key => {
    const count = props.orders.filter(o => o.orderStatus === key).length
    return {
      key,
      label: statusMap[key].label,
      color: statusMap[key].color,
      count,
      percentage: (count / total) * 100
    }
  })
})

const revenueToday = computed(() => {
  const today = new Date().toDateString()
  return props.orders
    .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt).toDateString() === today)
    .reduce((sum, o) => sum + o.total, 0)
})

const revenueWeek = computed(() => {
  const weekAgo = new Date()
  weekAgo.setDate(weekAgo.getDate() - 7)
  return props.orders
    .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt) >= weekAgo)
    .reduce((sum, o) => sum + o.total, 0)
})

const revenueMonth = computed(() => {
  const monthAgo = new Date()
  monthAgo.setMonth(monthAgo.getMonth() - 1)
  return props.orders
    .filter(o => o.paymentStatus === 'paid' && new Date(o.createdAt) >= monthAgo)
    .reduce((sum, o) => sum + o.total, 0)
})

const hasRevenueData = computed(() => {
  return revenueToday.value > 0 || revenueWeek.value > 0 || revenueMonth.value > 0
})
</script>