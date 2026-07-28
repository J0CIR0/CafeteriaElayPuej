<template>
  <div class="container mt-4">
    <h2 class="mb-4">Panel de Administración</h2>
    
    <ul class="nav nav-tabs mb-4">
      <li class="nav-item">
        <button class="nav-link" :class="{ active: activeTab === 'products' }" @click="activeTab = 'products'">Productos</button>
      </li>
      <li class="nav-item">
        <button class="nav-link" :class="{ active: activeTab === 'categories' }" @click="activeTab = 'categories'">Categorías</button>
      </li>
      <li class="nav-item">
        <button class="nav-link" :class="{ active: activeTab === 'orders' }" @click="activeTab = 'orders'">Pedidos</button>
      </li>
      <li class="nav-item">
        <button class="nav-link" :class="{ active: activeTab === 'inventory' }" @click="activeTab = 'inventory'">Inventario</button>
      </li>
    </ul>
    
    <div v-if="activeTab === 'products'">
      <div class="d-flex justify-content-between mb-3">
        <h4>Gestión de Productos</h4>
        <button class="btn btn-primary" @click="showProductForm = true">Nuevo Producto</button>
      </div>
      
      <div class="table-responsive">
        <table class="table table-striped">
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Categoría</th>
              <th>Precio</th>
              <th>Stock</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="product in productsStore.products" :key="product.id">
              <td>{{ product.id }}</td>
              <td>{{ product.name }}</td>
              <td>{{ product.categoryName }}</td>
              <td>${{ product.price.toFixed(2) }}</td>
              <td>
                <span :class="product.stock <= product.minStock ? 'text-danger' : ''">
                  {{ product.stock }}
                </span>
              </td>
              <td>
                <span class="badge" :class="product.isAvailable ? 'bg-success' : 'bg-danger'">
                  {{ product.isAvailable ? 'Disponible' : 'No Disponible' }}
                </span>
              </td>
              <td>
                <button class="btn btn-sm btn-warning me-1" @click="editProduct(product)">Editar</button>
                <button class="btn btn-sm btn-danger" @click="deleteProduct(product.id)">Eliminar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    
    <div v-if="activeTab === 'orders'">
      <h4>Gestión de Pedidos</h4>
      <div class="row mb-3">
        <div class="col-md-4">
          <button class="btn btn-outline-primary w-100" @click="ordersStore.fetchPendingOrders()">
            Pendientes ({{ ordersStore.pendingOrders.length }})
          </button>
        </div>
        <div class="col-md-4">
          <button class="btn btn-outline-success w-100" @click="ordersStore.fetchPaidOrders()">
            Pagados ({{ ordersStore.paidOrders.length }})
          </button>
        </div>
        <div class="col-md-4">
          <button class="btn btn-outline-secondary w-100" @click="ordersStore.fetchOrders()">
            Todos ({{ ordersStore.orders.length }})
          </button>
        </div>
      </div>
      
      <div class="table-responsive">
        <table class="table table-striped">
          <thead>
            <tr>
              <th># Pedido</th>
              <th>Cliente</th>
              <th>Total</th>
              <th>Pago</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in displayOrders" :key="order.id">
              <td>{{ order.orderNumber }}</td>
              <td>{{ order.user?.fullName || 'N/A' }}</td>
              <td>${{ order.total.toFixed(2) }}</td>
              <td>
                <span class="badge" :class="order.paymentStatus === 'pending' ? 'bg-warning' : 'bg-success'">
                  {{ order.paymentStatus === 'pending' ? 'Pendiente' : 'Pagado' }}
                </span>
              </td>
              <td>
                <span class="badge" :class="getOrderStatusClass(order.orderStatus)">
                  {{ getOrderStatusText(order.orderStatus) }}
                </span>
              </td>
              <td>
                <button v-if="order.paymentStatus === 'pending'" class="btn btn-sm btn-success" @click="markAsPaid(order.id)">
                  Marcar Pagado
                </button>
                <button v-if="order.paymentStatus === 'paid' && order.orderStatus === 'preparing'" class="btn btn-sm btn-primary" @click="updateOrderStatus(order.id, 'ready')">
                  Marcar Listo
                </button>
                <button v-if="order.paymentStatus === 'paid' && order.orderStatus === 'ready'" class="btn btn-sm btn-info" @click="updateOrderStatus(order.id, 'delivered')">
                  Entregar
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useProductsStore } from '../stores/products'
import { useOrdersStore } from '../stores/orders'

const activeTab = ref('products')
const productsStore = useProductsStore()
const ordersStore = useOrdersStore()

onMounted(async () => {
  await productsStore.fetchProducts()
  await ordersStore.fetchOrders()
})

const displayOrders = computed(() => {
  return ordersStore.orders
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
    await ordersStore.fetchOrders()
  }
}

const updateOrderStatus = async (orderId, status) => {
  const result = await ordersStore.updateOrderStatus(orderId, status)
  if (result.success) {
    await ordersStore.fetchOrders()
  }
}

const editProduct = (product) => {
  console.log('Editar producto:', product)
}

const deleteProduct = async (productId) => {
  if (confirm('¿Estás seguro de eliminar este producto?')) {
    const result = await productsStore.deleteProduct(productId)
    if (result.success) {
      await productsStore.fetchProducts()
    }
  }
}
</script>