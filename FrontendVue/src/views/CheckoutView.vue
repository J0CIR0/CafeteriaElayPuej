<template>
  <div class="container-fluid px-3 px-lg-4 py-3">
    <div class="mb-4">
      <h2 class="fw-bold m-0" style="color: #ffffff;">
        <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
          Finalizar Pedido (Checkout)
        </span>
      </h2>
      <p class="text-muted small m-0 mt-1 ms-3">
        Selecciona tu método de pago y confirma los datos del pedido
      </p>
    </div>

    <div v-if="orderCreatedSuccess" class="row justify-content-center my-4">
      <div class="col-md-8 col-lg-6 text-center">
        <div class="admin-card p-4 p-lg-5 shadow-lg">
          <div class="rounded-circle bg-success text-white p-3 d-inline-flex mb-3">
            <i class="bi bi-check-circle-fill display-4"></i>
          </div>
          <h3 class="fw-bold" style="color: var(--color-cafe-tostado);">¡Pedido Realizado con Éxito!</h3>
          <p class="text-muted">Tu número de orden es: <strong class="text-dark fs-5">#{{ createdOrderNumber }}</strong></p>

          <div v-if="paymentMethod === 'qr'" class="card border-0 bg-light p-3 mb-4">
            <h5 class="fw-bold text-dark mb-2">Escanea el Código QR para Pagar</h5>
            <div class="my-2">
              <img src="/images/qr-code.jpeg" alt="Código QR de Pago" class="img-fluid rounded border shadow-sm" style="max-width: 220px;">
            </div>
            <div class="fs-5 text-success fw-bold mt-2">Monto Total a Pagar: Bs. {{ createdOrderTotal.toFixed(2) }}</div>
            <p class="text-muted small mb-0 mt-1">Transfiere mediante tu banca móvil. El trabajador confirmará tu pago inmediatamente.</p>
          </div>

          <div v-else class="alert alert-success p-3 mb-4">
            <h5 class="fw-bold mb-2">Pago en Efectivo</h5>
            <p class="m-0">Tu pedido ha sido registrado. Por favor abona <strong>Bs. {{ createdOrderTotal.toFixed(2) }}</strong> al momento de recoger tu pedido en la caja de la cafetería.</p>
          </div>

          <div class="d-grid gap-2 col-md-8 mx-auto">
            <router-link to="/orders" class="btn btn-primary btn-lg">
              <i class="bi bi-receipt me-2"></i>Ver Mis Pedidos
            </router-link>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="row g-4">
      <div class="col-lg-7">
        <div class="admin-card h-100">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Resumen de Productos
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-modern align-middle mb-0">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th class="text-center">Cantidad</th>
                    <th class="text-end">Precio Unitario</th>
                    <th class="text-end">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in cartStore.items" :key="item.id">
                    <td class="fw-bold">{{ item.name }}</td>
                    <td class="text-center">{{ item.quantity }}</td>
                    <td class="text-end">Bs. {{ Number(item.price).toFixed(2) }}</td>
                    <td class="text-end fw-bold">Bs. {{ (Number(item.price) * Number(item.quantity)).toFixed(2) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <th colspan="3" class="text-end fs-5">Total a Pagar:</th>
                    <th class="text-end fs-4 text-success">Bs. {{ cartStore.totalPrice.toFixed(2) }}</th>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-5">
        <div class="admin-card">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Método de Pago y Notas
          </div>
          <div class="card-body">
            <form @submit.prevent="confirmOrder">
              <div class="mb-4">
                <label class="form-label fw-bold mb-2">Selecciona el Método de Pago</label>
                <div class="form-check p-3 border rounded mb-2 bg-light d-flex align-items-center gap-2">
                  <input class="form-check-input ms-0 me-2" type="radio" id="qr" value="qr" v-model="paymentMethod">
                  <label class="form-check-label fw-bold cursor-pointer" for="qr">
                    <i class="bi bi-qr-code me-2 text-primary"></i>Pago con QR Simple (Transferencia)
                  </label>
                </div>
                <div class="form-check p-3 border rounded bg-light d-flex align-items-center gap-2">
                  <input class="form-check-input ms-0 me-2" type="radio" id="cash" value="cash" v-model="paymentMethod">
                  <label class="form-check-label fw-bold cursor-pointer" for="cash">
                    <i class="bi bi-cash-stack me-2 text-success"></i>Pago en Efectivo (Al recojo)
                  </label>
                </div>
              </div>

              <div class="mb-4">
                <label class="form-label fw-bold">Notas Adicionales (Opcional)</label>
                <textarea class="form-control form-modern" v-model="notes" rows="3" placeholder="Ej: Sin azúcar, para llevar a las 16:30, etc."></textarea>
              </div>

              <div v-if="error" class="alert alert-danger mb-3">{{ error }}</div>

              <button type="submit" class="btn btn-success btn-lg w-100 fw-bold shadow-sm" :disabled="loading || cartStore.items.length === 0">
                <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                <span>{{ loading ? 'Procesando Pedido...' : 'Confirmar y Realizar Pedido' }}</span>
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
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

const orderCreatedSuccess = ref(false)
const createdOrderNumber = ref('')
const createdOrderTotal = ref(0)

onMounted(() => {
  if (cartStore.items.length === 0 && !orderCreatedSuccess.value) {
    router.push('/cart')
  }
})

const confirmOrder = async () => {
  if (cartStore.items.length === 0) {
    error.value = 'El carrito está vacío'
    return
  }

  const totalAmount = cartStore.totalPrice.toFixed(2)
  const methodLabel = paymentMethod.value === 'qr' ? 'Pago con QR Simple' : 'Pago en Efectivo'

  if (!confirm(`¿Confirmas realizar este pedido por un total de Bs. ${totalAmount} mediante ${methodLabel}?`)) {
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
  loading.value = false

  if (result.success && result.data) {
    createdOrderNumber.value = result.data.orderNumber || '0001'
    createdOrderTotal.value = Number(result.data.total || cartStore.totalPrice)
    orderCreatedSuccess.value = true
    cartStore.clearCart()
  } else {
    error.value = result.message || 'Error al procesar la orden'
  }
}
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
</style>