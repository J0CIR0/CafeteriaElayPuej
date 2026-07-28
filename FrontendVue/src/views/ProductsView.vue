<template>
  <div class="container-fluid menu-page px-3 px-lg-4 py-4">
    <section class="menu-hero admin-card p-4 p-lg-5 mb-4">
      <div class="row align-items-end g-4">
        <div class="col-lg-8">
          <span class="eyebrow-chip">Santa Cruz de la Sierra • cafetería tradicional</span>
          <h1 class="menu-title mt-3 mb-2">¿Qué se te antoja, pariente?</h1>
          <p class="menu-lead mb-0">Para el horneao, un cafecito pa' la tarde o unos masacos bien cambas. Todo en Bs y con sabor de casa.</p>
        </div>
        <div class="col-lg-4 text-lg-end">
          <div class="menu-meta-box">
            <div class="small text-uppercase text-muted">Disponible hoy</div>
            <div class="fw-semibold">{{ availableCount }} delicias</div>
            <div class="small text-muted">Tarde de tertulia asegurada</div>
          </div>
        </div>
      </div>
    </section>

    <div class="row g-3 mb-4">
      <div class="col-md-3" v-for="category in categories" :key="category.id">
        <button class="menu-filter w-100" :class="activeCategory === category.id ? 'active' : ''" @click="activeCategory = category.id">
          {{ category.name }}
        </button>
      </div>
    </div>

    <div v-if="productsStore.loading" class="text-center py-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Cargando...</span>
      </div>
    </div>

    <div v-else class="row g-4">
      <div v-for="product in filteredProducts" :key="product.id" class="col-lg-4 col-md-6">
        <article class="menu-card h-100">
          <div class="menu-card__image" :style="menuImageStyle(product)">
            <span class="menu-card__badge">{{ product.categoryName }}</span>
          </div>
          <div class="menu-card__body">
            <div class="d-flex justify-content-between gap-3 mb-2">
              <div>
                <h2 class="menu-card__title mb-1">{{ product.name }}</h2>
                <p class="menu-card__text mb-0">{{ product.description }}</p>
              </div>
              <div class="menu-card__price">{{ formatBs(product.price) }}</div>
            </div>
            <div class="menu-card__details mb-3">
              <span v-if="product.origin">Origen: {{ product.origin }}</span>
              <span v-if="product.preparationTime">Preparación: {{ product.preparationTime }}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-3">
              <span class="badge-modern" :class="product.stock > 0 && product.isAvailable ? 'badge-verde' : 'badge-rojo'">
                {{ product.stock > 0 && product.isAvailable ? 'Listo pa\' salir' : 'Agotado' }}
              </span>
              <span class="text-muted small">Stock {{ product.stock }}</span>
            </div>
            <button class="btn btn-primary w-100" @click="addToCart(product)" :disabled="product.stock === 0 || !product.isAvailable">
              {{ product.stock === 0 || !product.isAvailable ? 'Agotado' : 'Añadir al carrito' }}
            </button>
          </div>
        </article>
      </div>
      <div v-if="filteredProducts.length === 0" class="col-12 text-center py-5 text-muted">
        No hay productos en esta categoría, pero se viene algo rico, pariente.
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useProductsStore } from '../stores/products'
import { useCartStore } from '../stores/cart'
import { formatBolivianos as formatBs } from '../stores/cafeteriaData'

const productsStore = useProductsStore()
const cartStore = useCartStore()
const activeCategory = ref('all')

onMounted(async () => {
  await productsStore.fetchAvailableProducts()
  await productsStore.fetchCategories()
})

const categories = computed(() => [
  { id: 'all', name: 'Todo el menú' },
  ...productsStore.categories.filter((category) => category.isActive)
])

const filteredProducts = computed(() => {
  if (activeCategory.value === 'all') {
    return productsStore.products
  }

  return productsStore.products.filter((product) => Number(product.categoryId) === Number(activeCategory.value))
})

const availableCount = computed(() => productsStore.products.filter((product) => product.stock > 0 && product.isAvailable).length)

const menuImageStyle = (product) => {
  const palettes = [
    'linear-gradient(135deg, #3E2723 0%, #C85A32 100%)',
    'linear-gradient(135deg, #1B4332 0%, #3E2723 100%)',
    'linear-gradient(135deg, #C85A32 0%, #FAF6F0 100%)'
  ]

  if (product.imageUrl) {
    return { background: `url(${product.imageUrl}) center/cover` }
  }

  return { background: palettes[Number(product.id) % palettes.length] }
}

const addToCart = (product) => {
  cartStore.addItem(product)
}
</script>
