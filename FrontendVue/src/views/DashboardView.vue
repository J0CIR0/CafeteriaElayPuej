<template>
  <div class="container mt-4">
    <h2 class="mb-4">Dashboard - Trabajador</h2>
    
    <div class="row">
      <div class="col-md-6">
        <div class="card mb-3">
          <div class="card-header">
            <h5>Pedidos Pendientes de Pago</h5>
          </div>
          <div class="card-body">
            <div v-if="ordersStore.pendingOrders.length === 0">
              <p class="text-muted">No hay pedidos pendientes de pago</p>
            </div>
            <div v-else>
              <div v-for="order in ordersStore.pendingOrders" :key="order.id" class="border-bottom mb-2 pb-2">
                <div class="d-flex justify-content-between">
                  <span><strong>#{{ order.orderNumber }}</strong></span>
                  <span>Bs {{ order.total.toFixed(2) }}</span>
                </div>
                <button class="btn btn-sm btn-success" @click="markAsPaid(order.id)">Marcar Pagado</button>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div class="col-md-6">
        <div class="card mb-3">
          <div class="card-header">
            <h5>Pedidos Pagados</h5>
          </div>
          <div class="card-body">
            <div v-if="ordersStore.paidOrders.length === 0">
              <p class="text-muted">No hay pedidos pagados</p>
            </div>
            <div v-else>
              <div v-for="order in ordersStore.paidOrders" :key="order.id" class="border-bottom mb-2 pb-2">
                <div class="d-flex justify-content-between">
                  <span><strong>#{{ order.orderNumber }}</strong></span>
                  <span class="badge" :class="getOrderStatusClass(order.orderStatus)">
                    {{ getOrderStatusText(order.orderStatus) }}
                  </span>
                </div>
                <div class="mt-1">
                  <button v-if="order.orderStatus === 'preparing'" class="btn btn-sm btn-primary" @click="updateOrderStatus(order.id, 'ready')">
                    Marcar Listo
                  </button>
                  <button v-if="order.orderStatus === 'ready'" class="btn btn-sm btn-info" @click="updateOrderStatus(order.id, 'delivered')">
                    Entregar
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <div class="row">
      <div class="col-md-4">
        <div class="card bg-primary text-white">
          <div class="card-body text-center">
            <h3>{{ ordersStore.pendingOrders.length }}</h3>
            <p>Pedidos Pendientes</p>
          </div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="card bg-success text-white">
          <div class="card-body text-center">
            <h3>{{ ordersStore.paidOrders.length }}</h3>
            <p>Pedidos Pagados</p>
          </div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="card bg-info text-white">
          <div class="card-body text-center">
            <h3>{{ totalProducts }}</h3>
            <p>Productos Disponibles</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useOrdersStore } from '../stores/orders'
import { useProductsStore } from '../stores/products'

const ordersStore = useOrdersStore()
const productsStore = useProductsStore()

const totalProducts = computed(() => {
  return productsStore.products.filter(p => p.isAvailable).length
})

onMounted(async () => {
  await ordersStore.fetchPendingOrders()
  await ordersStore.fetchPaidOrders()
  await productsStore.fetchProducts()
})

const getOrderStatusClass = (status) => {
  const classes = {
    pending: 'bg-secondary',
    preparing: 'bg-info',
    ready: 'bg-primary',
    delivered: 'bg-success'
  }
  return classes[status] || 'bg-secondary'
}

const getOrderStatusText = (status) => {
  const texts = {
    pending: 'Pendiente',
    preparing: 'En Preparación',
    ready: 'Listo para Recoger',
    delivered: 'Entregado'
  }
  return texts[status] || status
}

const markAsPaid = async (orderId) => {
  const result = await ordersStore.updatePaymentStatus(orderId, 'paid')
  if (result.success) {
    await ordersStore.fetchPendingOrders()
    await ordersStore.fetchPaidOrders()
  }
}

const updateOrderStatus = async (orderId, status) => {
  const result = await ordersStore.updateOrderStatus(orderId, status)
  if (result.success) {
    await ordersStore.fetchPendingOrders()
    await ordersStore.fetchPaidOrders()
  }
}
</script>