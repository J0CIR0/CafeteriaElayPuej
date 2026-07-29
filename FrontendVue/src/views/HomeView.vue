<template>
  <div class="container-fluid menu-page px-3 px-lg-4 py-3">
    <section class="menu-hero admin-card p-4 p-lg-5 mb-4 position-relative overflow-hidden">
      <div class="row align-items-center g-4">
        <div class="col-lg-7">
          <span class="eyebrow-chip fw-bold">
            <i class="bi bi-geo-alt-fill me-1"></i> Santa Cruz de la Sierra • Cafetería Tradicional
          </span>
          <h1 class="menu-title mt-3 mb-3 fw-bold" style="color: var(--color-cafe-tostado);">
            Café que cuenta la historia del origen
          </h1>
          <p class="menu-lead mb-4 text-muted fs-5">
            Granos de origen único, tostados artesanalmente en pequeños lotes. Disfruta de la mejor experiencia gastronómica camba con masacos, horneados tradicionales y café de especialidad.
          </p>
          <div class="d-flex flex-wrap gap-2">
            <router-link to="/products" class="btn btn-primary btn-lg shadow-sm px-4">
              <i class="bi bi-cup-hot-fill me-2"></i>Ver Menú Completo
            </router-link>
            <router-link v-if="authStore.isAuthenticated" to="/orders" class="btn btn-outline-dark btn-lg px-4">
              <i class="bi bi-bag-check-fill me-2"></i>Mis Pedidos
            </router-link>
            <router-link v-else to="/login" class="btn btn-outline-dark btn-lg px-4">
              <i class="bi bi-box-arrow-in-right me-2"></i>Iniciar Sesión
            </router-link>
          </div>
        </div>
        <div class="col-lg-5 text-center">
          <div class="p-4 rounded-4 shadow-sm text-start" style="background: #faf6f0; border: 1px solid rgba(62, 39, 35, 0.12);">
            <div class="d-flex align-items-center gap-3 mb-3">
              <div class="rounded-circle p-3 text-white" style="background: var(--color-verde-selva);">
                <i class="bi bi-clock-history fs-3"></i>
              </div>
              <div>
                <h6 class="fw-bold m-0" style="color: var(--color-cafe-tostado);">Horarios de Atención</h6>
                <small class="text-muted">Lunes a Domingo</small>
              </div>
            </div>
            <ul class="list-unstyled mb-0 small text-muted">
              <li class="d-flex justify-content-between py-1 border-bottom"><span>Mañanas:</span> <strong>07:00 AM - 12:00 PM</strong></li>
              <li class="d-flex justify-content-between py-1 border-bottom"><span>Tardes de Horneao:</span> <strong>03:30 PM - 09:00 PM</strong></li>
              <li class="d-flex justify-content-between pt-2"><span>Atención presencial y pedidos online</span></li>
            </ul>
          </div>
        </div>
      </div>
    </section>

    <section class="mb-5">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 class="fw-bold m-0" style="color: #ffffff;">
            <span style="border-left: 4px solid var(--color-terracota); padding-left: 12px;">
              Destacados de la Casa
            </span>
          </h3>
          <p class="text-muted small m-0 mt-1 ms-3">Nuestras sugerencias más pedidas por nuestros clientes</p>
        </div>
        <router-link to="/products" class="btn btn-sm btn-outline-success">
          Ver Todo el Menú <i class="bi bi-arrow-right ms-1"></i>
        </router-link>
      </div>

      <div v-if="productsStore.loading" class="text-center py-5">
        <div class="spinner-border text-success" role="status">
          <span class="visually-hidden">Cargando destacados...</span>
        </div>
      </div>

      <div v-else class="row g-4">
        <div v-for="product in featuredProducts" :key="product.id" class="col-md-6 col-lg-4">
          <div class="admin-card h-100 d-flex flex-column justify-content-between">
            <div class="position-relative overflow-hidden" style="height: 190px;">
              <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name" class="w-100 h-100 object-fit-cover">
              <div v-else class="w-100 h-100 d-flex align-items-center justify-content-center text-white fw-bold fs-4" :style="menuImageStyle(product)">
                <i class="bi bi-cup-hot fs-1 opacity-75"></i>
              </div>
              <span class="badge position-absolute top-0 start-0 m-3 bg-dark text-white rounded-pill px-3 py-2 shadow-sm opacity-90">
                {{ product.categoryName || 'Sugerencia' }}
              </span>
            </div>

            <div class="card-body d-flex flex-column justify-content-between p-3">
              <div>
                <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
                  <h4 class="fw-bold m-0" style="color: #2b1b17;">{{ product.name }}</h4>
                  <span class="fs-5 fw-bold text-success text-nowrap">Bs. {{ Number(product.price).toFixed(2) }}</span>
                </div>
                <p class="text-muted small mb-3">{{ product.description }}</p>
              </div>

              <div class="pt-3 border-top mt-auto">
                <button class="btn btn-primary w-100 fw-bold shadow-sm" @click="addToCart(product)" :disabled="product.stock === 0 || !product.isAvailable">
                  <i class="bi bi-cart-plus me-1"></i>
                  {{ product.stock === 0 || !product.isAvailable ? 'Agotado' : 'Añadir al Carrito' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="admin-card p-4 p-lg-5">
      <div class="row g-4">
        <div class="col-md-4">
          <div class="d-flex gap-3">
            <i class="bi bi-geo-alt fs-2 text-success"></i>
            <div>
              <h5 class="fw-bold" style="color: var(--color-cafe-tostado);">Ubicación</h5>
              <p class="text-muted small m-0">Av. Principal s/n, Zona Equipetrol / Casco Viejo, Santa Cruz de la Sierra.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="d-flex gap-3">
            <i class="bi bi-telephone fs-2 text-success"></i>
            <div>
              <h5 class="fw-bold" style="color: var(--color-cafe-tostado);">Contacto & Pedidos</h5>
              <p class="text-muted small m-0">Teléfono: (+591) 74686331<br>Email: clarosrocajosue@gmail.com</p>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="d-flex gap-3">
            <i class="bi bi-shield-check fs-2 text-success"></i>
            <div>
              <h5 class="fw-bold" style="color: var(--color-cafe-tostado);">Calidad Garantizada</h5>
              <p class="text-muted small m-0">Granos frescos tostados diariamente con insumos 100% de la región.</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useProductsStore } from '../stores/products'
import { useCartStore } from '../stores/cart'

const authStore = useAuthStore()
const productsStore = useProductsStore()
const cartStore = useCartStore()

onMounted(async () => {
  await productsStore.fetchAvailableProducts()
})

const featuredProducts = computed(() => {
  return [...productsStore.products].slice(0, 3)
})

const menuImageStyle = (product) => {
  const palettes = [
    'linear-gradient(135deg, #3E2723 0%, #C85A32 100%)',
    'linear-gradient(135deg, #1B4332 0%, #3E2723 100%)',
    'linear-gradient(135deg, #C85A32 0%, #FAF6F0 100%)'
  ]

  if (product.imageUrl) {
    return { background: `url(${product.imageUrl}) center/cover` }
  }

  return { background: palettes[Number(product.id || 0) % palettes.length] }
}

const addToCart = (product) => {
  cartStore.addItem(product)
}
</script>

<style scoped>
</style>