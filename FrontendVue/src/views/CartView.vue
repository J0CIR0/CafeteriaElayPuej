<template>
  <div class="container mt-4">
    <h2 class="mb-4">Carrito de Compras</h2>
    
    <div v-if="cartStore.items.length === 0" class="text-center">
      <p class="lead">Tu carrito está vacío</p>
      <router-link to="/products" class="btn btn-primary">Ver Menú</router-link>
    </div>
    
    <div v-else>
      <div class="table-responsive">
        <table class="table table-hover">
          <thead>
            <tr>
              <th>Producto</th>
              <th>Precio</th>
              <th>Cantidad</th>
              <th>Subtotal</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in cartStore.items" :key="item.id">
              <td>{{ item.name }}</td>
              <td>Bs {{ item.price.toFixed(2) }}</td>
              <td>
                <div class="input-group" style="width: 120px;">
                  <button class="btn btn-outline-secondary" @click="updateQuantity(item.id, item.quantity - 1)">-</button>
                  <input type="number" class="form-control text-center" v-model.number="item.quantity" min="1" @change="updateQuantity(item.id, item.quantity)">
                  <button class="btn btn-outline-secondary" @click="updateQuantity(item.id, item.quantity + 1)">+</button>
                </div>
              </td>
              <td>Bs {{ (item.price * item.quantity).toFixed(2) }}</td>
              <td>
                <button class="btn btn-danger btn-sm" @click="cartStore.removeItem(item.id)">Eliminar</button>
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr>
              <th colspan="3" class="text-end">Total:</th>
              <th>Bs {{ cartStore.totalPrice.toFixed(2) }}</th>
              <th></th>
            </tr>
          </tfoot>
        </table>
      </div>
      
      <div class="row mt-4">
        <div class="col-md-6">
          <router-link to="/products" class="btn btn-secondary">Seguir Comprando</router-link>
          <button class="btn btn-danger ms-2" @click="cartStore.clearCart">Vaciar Carrito</button>
        </div>
        <div class="col-md-6 text-end">
          <button class="btn btn-success btn-lg" @click="proceedToCheckout" :disabled="authStore.isAuthenticated === false">
            {{ authStore.isAuthenticated ? 'Proceder al Pago' : 'Inicia Sesión para Pagar' }}
          </button>
          <p v-if="!authStore.isAuthenticated" class="text-muted small">
            <router-link to="/login">Inicia sesión</router-link> para realizar tu pedido
          </p>
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

const updateQuantity = (productId, quantity) => {
  cartStore.updateQuantity(productId, quantity)
}

const proceedToCheckout = () => {
  if (!authStore.isAuthenticated) {
    router.push('/login')
    return
  }
  router.push('/checkout')
}
</script>