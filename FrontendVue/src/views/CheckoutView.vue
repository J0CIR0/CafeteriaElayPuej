<template>
  <div class="container mt-4">
    <h2 class="mb-4">Finalizar Pedido</h2>
    
    <div class="row">
      <div class="col-md-8">
        <div class="card">
          <div class="card-body">
            <h5 class="card-title">Resumen del Pedido</h5>
            <div class="table-responsive">
              <table class="table">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th>Cantidad</th>
                    <th>Precio</th>
                    <th>Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in cartStore.items" :key="item.id">
                    <td>{{ item.name }}</td>
                    <td>{{ item.quantity }}</td>
                    <td>${{ item.price.toFixed(2) }}</td>
                    <td>${{ (item.price * item.quantity).toFixed(2) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <th colspan="3" class="text-end">Subtotal:</th>
                    <th>${{ cartStore.totalPrice.toFixed(2) }}</th>
                  </tr>
                  <tr>
                    <th colspan="3" class="text-end">IVA (13%):</th>
                    <th>${{ (cartStore.totalPrice * 0.13).toFixed(2) }}</th>
                  </tr>
                  <tr>
                    <th colspan="3" class="text-end">Total:</th>
                    <th>${{ (cartStore.totalPrice * 1.13).toFixed(2) }}</th>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>
      </div>
      
      <div class="col-md-4">
        <div class="card">
          <div class="card-body">
            <h5 class="card-title">Método de Pago</h5>
            <div class="mb-3">
              <div class="form-check">
                <input class="form-check-input" type="radio" id="qr" value="qr" v-model="paymentMethod">
                <label class="form-check-label" for="qr">QR</label>
              </div>
              <div class="form-check">
                <input class="form-check-input" type="radio" id="cash" value="cash" v-model="paymentMethod">
                <label class="form-check-label" for="cash">Efectivo</label>
              </div>
            </div>
            
            <div class="mb-3">
              <label class="form-label">Notas (opcional)</label>
              <textarea class="form-control" v-model="notes" rows="3"></textarea>
            </div>
            
            <button class="btn btn-success w-100" @click="createOrder" :disabled="loading">
              {{ loading ? 'Procesando...' : 'Confirmar Pedido' }}
            </button>
            
            <p v-if="error" class="text-danger mt-2">{{ error }}</p>
          </div>
        </div>
        
        <div class="card mt-3" v-if="qrPayment">
          <div class="card-body text-center">
            <h6 class="card-title">QR para Pago</h6>
            <img src="/images/qr-code.png" alt="QR Code" class="img-fluid" style="max-width: 200px;">
            <p class="mt-2">Monto a pagar: <strong>${{ (cartStore.totalPrice * 1.13).toFixed(2) }}</strong></p>
            <p class="text-muted small">Escanea el QR para realizar el pago</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import { useOrdersStore } from '../stores/orders'

const router = useRouter()
const cartStore = useCartStore()
const ordersStore = useOrdersStore()
const paymentMethod = ref('qr')
const notes = ref('')
const loading = ref(false)
const error = ref('')
const qrPayment = ref(false)

const createOrder = async () => {
  if (cartStore.items.length === 0) {
    error.value = 'El carrito está vacío'
    return
  }
  
  loading.value = true
  error.value = ''
  
  const orderData = {
    orderItems: cartStore.items.map(item => ({
      productId: item.id,
      quantity: item.quantity
    })),
    paymentMethod: paymentMethod.value,
    notes: notes.value
  }
  
  const result = await ordersStore.createOrder(orderData)
  
  if (result.success) {
    if (paymentMethod.value === 'qr') {
      qrPayment.value = true
      await ordersStore.fetchPendingOrders()
    } else {
      cartStore.clearCart()
      router.push('/orders')
    }
  } else {
    error.value = result.message
  }
  
  loading.value = false
}
</script>