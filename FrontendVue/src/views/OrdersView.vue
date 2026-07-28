<template>
  <div class="container mt-4">
    <h2 class="mb-4">Mis Pedidos</h2>
    
    <div v-if="ordersStore.loading" class="text-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Cargando...</span>
      </div>
    </div>
    
    <div v-else>
      <div v-if="ordersStore.myOrders.length === 0" class="text-center">
        <p class="lead">No tienes pedidos aún</p>
        <router-link to="/products" class="btn btn-primary">Hacer un Pedido</router-link>
      </div>
      
      <div v-else>
        <div v-for="order in ordersStore.myOrders" :key="order.id" class="card mb-3">
          <div class="card-header d-flex justify-content-between align-items-center">
            <span>
              <strong>Pedido #{{ order.orderNumber }}</strong>
              <span class="badge ms-2" :class="getPaymentStatusClass(order.paymentStatus)">
                {{ getPaymentStatusText(order.paymentStatus) }}
              </span>
              <span class="badge ms-2" :class="getOrderStatusClass(order.orderStatus)">
                {{ getOrderStatusText(order.orderStatus) }}
              </span>
            </span>
            <span class="text-muted small">{{ new Date(order.createdAt).toLocaleString() }}</span>
          </div>
          <div class="card-body">
            <div class="table-responsive">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th>Cantidad</th>
                    <th>Precio</th>
                    <th>Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="detail in order.orderDetails" :key="detail.id">
                    <td>{{ detail.product.name }}</td>
                    <td>{{ detail.quantity }}</td>
                    <td>Bs {{ detail.unitPrice.toFixed(2) }}</td>
                    <td>Bs {{ detail.subtotal.toFixed(2) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <th colspan="3" class="text-end">Subtotal:</th>
                    <th>Bs {{ order.subtotal.toFixed(2) }}</th>
                  </tr>
                  <tr>
                    <th colspan="3" class="text-end">IVA:</th>
                    <th>Bs {{ order.tax.toFixed(2) }}</th>
                  </tr>
                  <tr>
                    <th colspan="3" class="text-end">Total:</th>
                    <th>Bs {{ order.total.toFixed(2) }}</th>
                  </tr>
                </tfoot>
              </table>
            </div>
            <p v-if="order.notes" class="text-muted small">Notas: {{ order.notes }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useOrdersStore } from '../stores/orders'

const ordersStore = useOrdersStore()

onMounted(async () => {
  await ordersStore.fetchMyOrders()
})

const getPaymentStatusClass = (status) => {
  const classes = {
    pending: 'bg-warning',
    paid: 'bg-success',
    cancelled: 'bg-danger'
  }
  return classes[status] || 'bg-secondary'
}

const getPaymentStatusText = (status) => {
  const texts = {
    pending: 'Pendiente de Pago',
    paid: 'Pagado',
    cancelled: 'Cancelado'
  }
  return texts[status] || status
}

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
</script>