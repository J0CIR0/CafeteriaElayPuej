<template>
  <div class="container-fluid px-3 px-lg-4 py-3">
    <div class="mb-4">
      <h2 class="fw-bold m-0" style="color: #ffffff;">
        <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
          Carrito de Compras
        </span>
      </h2>
      <p class="text-muted small m-0 mt-1 ms-3">
        Revisa los productos seleccionados antes de confirmar tu pedido
      </p>
    </div>

    <div v-if="cartStore.items.length === 0" class="admin-card text-center py-5">
      <div class="card-body">
        <i class="bi bi-cart-x text-muted display-1 d-block mb-3"></i>
        <h4 class="fw-bold" style="color: var(--color-cafe-tostado);">Tu carrito está vacío</h4>
        <p class="text-muted mb-4">Aún no has agregado ningún producto a tu pedido.</p>
        <router-link to="/products" class="btn btn-primary btn-lg">
          <i class="bi bi-cup-hot me-2"></i>Ver Menú de Productos
        </router-link>
      </div>
    </div>

    <div v-else class="row g-4">
      <div class="col-lg-8">
        <div class="admin-card">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Productos en tu Carrito ({{ cartStore.totalItems }})
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th class="text-end">Precio Unitario</th>
                    <th class="text-center">Cantidad</th>
                    <th class="text-end">Subtotal</th>
                    <th class="text-center">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in cartStore.items" :key="item.id">
                    <td class="fw-bold">
                      <div>{{ item.name }}</div>
                      <small class="text-muted" v-if="item.categoryName">{{ item.categoryName }}</small>
                    </td>
                    <td class="text-end fw-semibold text-muted">
                      Bs. {{ Number(item.price).toFixed(2) }}
                    </td>
                    <td class="text-center">
                      <div class="input-group input-group-sm justify-content-center mx-auto" style="max-width: 120px;">
                        <button class="btn btn-outline-secondary" @click="updateQuantity(item, item.quantity - 1)">-</button>
                        <input type="number" class="form-control text-center bg-white" v-model.number="item.quantity" min="1" @change="updateQuantity(item, item.quantity)">
                        <button class="btn btn-outline-secondary" @click="updateQuantity(item, item.quantity + 1)">+</button>
                      </div>
                    </td>
                    <td class="text-end fw-bold text-success">
                      Bs. {{ (Number(item.price) * Number(item.quantity)).toFixed(2) }}
                    </td>
                    <td class="text-center">
                      <button class="btn btn-sm btn-outline-danger" @click="removeItemWithConfirm(item)" title="Eliminar producto">
                        <i class="bi bi-trash"></i>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="d-flex flex-wrap justify-content-between align-items-center mt-3 gap-2">
          <router-link to="/products" class="btn btn-outline-dark">
            <i class="bi bi-arrow-left me-1"></i> Seguir Comprando
          </router-link>
          <button class="btn btn-outline-danger" @click="clearCartWithConfirm">
            <i class="bi bi-trash me-1"></i> Vaciar Carrito
          </button>
        </div>
      </div>

      <div class="col-lg-4">
        <div class="admin-card">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Resumen del Pedido
          </div>
          <div class="card-body">
            <div class="d-flex justify-content-between mb-3 fs-5 fw-bold text-dark">
              <span>Total a Pagar:</span>
              <span class="text-success fs-4">Bs. {{ cartStore.totalPrice.toFixed(2) }}</span>
            </div>

            <p class="text-muted small mb-4">Precios finales en Bolivianos (Bs.). Sin recargos ni impuestos adicionales.</p>

            <div v-if="!authStore.isAuthenticated" class="alert alert-warning py-2 mb-3">
              <small>Debes iniciar sesión o registrarte para realizar el pedido.</small>
            </div>

            <button class="btn btn-success btn-lg w-100 fw-bold shadow-sm" @click="proceedToCheckout">
              {{ authStore.isAuthenticated ? 'Proceder al Pago' : 'Inicia Sesión para Pagar' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

const updateQuantity = (item, quantity) => {
  if (quantity <= 0) {
    removeItemWithConfirm(item)
  } else {
    cartStore.updateQuantity(item.id, quantity)
  }
}

const removeItemWithConfirm = (item) => {
  if (confirm(`¿Desea eliminar "${item.name}" del carrito?`)) {
    cartStore.removeItem(item.id)
  }
}

const clearCartWithConfirm = () => {
  if (confirm('¿Está seguro de que desea vaciar todo el carrito?')) {
    cartStore.clearCart()
  }
}

const proceedToCheckout = () => {
  if (!authStore.isAuthenticated) {
    router.push('/login')
    return
  }
  router.push('/checkout')
}
</script>

<style scoped>
</style>