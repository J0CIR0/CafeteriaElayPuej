<template>
  <div class="admin-card">
    <div class="card-header d-flex justify-content-between align-items-center">
      <span>Alertas de Stock Bajo</span>
      <span class="badge bg-danger">{{ lowStockProducts.length }}</span>
    </div>
    <div class="card-body">
      <div v-if="lowStockProducts.length === 0" class="text-center text-muted py-3">
        Todos los productos tienen stock suficiente
      </div>
      <div v-else>
        <div v-for="product in lowStockProducts" :key="product.id" class="d-flex justify-content-between align-items-center border-bottom py-2">
          <div>
            <span class="fw-bold">{{ product.name }}</span>
            <span class="text-muted ms-2">({{ product.categoryName || 'Sin categoria' }})</span>
          </div>
          <div>
            <span class="badge bg-danger">Stock: {{ product.stock }}</span>
            <span class="text-muted ms-2">Minimo: {{ product.minStock }}</span>
            <button class="btn btn-sm btn-cafe ms-2" @click="$emit('update-stock', product)">Ajustar</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()

const lowStockProducts = computed(() => {
  return adminStore.products
    .filter(p => p.isAvailable && p.stock <= p.minStock)
    .sort((a, b) => a.stock - b.stock)
})

defineEmits(['update-stock'])
</script>