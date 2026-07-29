<template>
  <div class="container-fluid menu-page px-3 px-lg-4 py-3">
    <section class="menu-hero admin-card p-4 p-lg-5 mb-4">
      <div class="row align-items-end g-4">
        <div class="col-lg-8">
          <span class="eyebrow-chip">Santa Cruz de la Sierra • Cafetería Tradicional</span>
          <h1 class="menu-title mt-3 mb-2" style="color: var(--color-cafe-tostado);">Menú Tradicional & Especialidades</h1>
          <p class="menu-lead mb-0">Explora nuestro menú completo con horneados típicos, cafés de especialidad, bebidas frías y repostería artesanal.</p>
        </div>
        <div class="col-lg-4 text-lg-end">
          <div class="menu-meta-box">
            <div class="small text-uppercase text-muted">Disponible en menú</div>
            <div class="fw-semibold fs-5 text-success">{{ availableCount }} opciones deliciosas</div>
            <div class="small text-muted">Frescos y listos para disfrutar</div>
          </div>
        </div>
      </div>
    </section>

    <div class="row g-3 mb-4 align-items-center">
      <div class="col-md-7">
        <div class="input-group shadow-sm">
          <span class="input-group-text bg-white border-end-0"><i class="bi bi-search"></i></span>
          <input type="text" class="form-control border-start-0" placeholder="Buscar por nombre de producto..." v-model="searchQuery">
          <button v-if="searchQuery" class="btn btn-link text-muted" @click="searchQuery = ''">Limpiar</button>
        </div>
      </div>
      <div class="col-md-5">
        <select class="form-select shadow-sm" v-model="activeCategory">
          <option value="all">Todas las Categorías</option>
          <option v-for="cat in productsStore.categories" :key="cat.id" :value="cat.id">
            {{ cat.name }}
          </option>
        </select>
      </div>
    </div>

    <div class="row g-2 mb-4">
      <div class="col-auto" v-for="category in categoriesPills" :key="category.id">
        <button class="btn btn-sm px-3 py-2 rounded-pill fw-semibold shadow-sm" :class="activeCategory === category.id ? 'btn-primary' : 'btn-outline-light text-white border-light'" @click="activeCategory = category.id">
          {{ category.name }}
        </button>
      </div>
    </div>

    <div v-if="productsStore.loading" class="text-center py-5">
      <div class="spinner-border text-success" role="status">
        <span class="visually-hidden">Cargando menú...</span>
      </div>
    </div>

    <div v-else class="row g-4">
      <div v-for="product in filteredProducts" :key="product.id" class="col-lg-4 col-md-6">
        <div class="admin-card h-100 d-flex flex-column justify-content-between">
          <div class="position-relative overflow-hidden" style="height: 190px;">
            <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name" class="w-100 h-100 object-fit-cover">
            <div v-else class="w-100 h-100 d-flex align-items-center justify-content-center text-white fw-bold fs-4" :style="menuImageStyle(product)">
              <i class="bi bi-cup-hot fs-1 opacity-75"></i>
            </div>
            <span class="badge position-absolute top-0 start-0 m-3 bg-dark text-white rounded-pill px-3 py-2 shadow-sm opacity-90">
              {{ product.categoryName || 'Especialidad' }}
            </span>
          </div>

          <div class="card-body d-flex flex-column justify-content-between p-3">
            <div>
              <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
                <h4 class="fw-bold m-0" style="color: #2b1b17;">{{ product.name }}</h4>
                <span class="fs-5 fw-bold text-success text-nowrap">Bs. {{ Number(product.price).toFixed(2) }}</span>
              </div>
              <p class="text-muted small mb-3">{{ product.description }}</p>
              
              <div class="d-flex flex-wrap gap-2 mb-3 small">
                <span v-if="product.preparationTime" class="badge bg-light text-dark border"><i class="bi bi-clock me-1"></i>{{ product.preparationTime }}</span>
                <span v-if="product.origin" class="badge bg-light text-dark border"><i class="bi bi-geo-alt me-1"></i>{{ product.origin }}</span>
                <span v-if="product.flavorNotes" class="badge bg-light text-dark border"><i class="bi bi-tag me-1"></i>{{ product.flavorNotes }}</span>
              </div>
            </div>

            <div class="pt-3 border-top d-flex justify-content-between align-items-center mt-auto">
              <span class="small font-weight-bold" :class="product.stock <= product.minStock ? 'text-danger' : 'text-secondary'">
                {{ product.stock === 0 ? 'Agotado' : 'Stock: ' + product.stock + ' u.' }}
              </span>
              <button class="btn btn-primary px-3 fw-bold shadow-sm" @click="addToCart(product)" :disabled="product.stock === 0 || !product.isAvailable">
                <i class="bi bi-cart-plus me-1"></i>
                {{ product.stock === 0 || !product.isAvailable ? 'Agotado' : 'Añadir' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useProductsStore } from '../stores/products'
import { useCartStore } from '../stores/cart'

const productsStore = useProductsStore()
const cartStore = useCartStore()
const activeCategory = ref('all')
const searchQuery = ref('')

onMounted(async () => {
  await productsStore.fetchAvailableProducts()
  await productsStore.fetchCategories()
})

const categoriesPills = computed(() => [
  { id: 'all', name: 'Todo el menú' },
  ...productsStore.categories.filter((c) => c.isActive)
])

const filteredProducts = computed(() => {
  let list = productsStore.products

  if (activeCategory.value !== 'all') {
    list = list.filter((p) => Number(p.categoryId) === Number(activeCategory.value))
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim()
    list = list.filter((p) => p.name.toLowerCase().includes(q) || (p.description && p.description.toLowerCase().includes(q)))
  }

  return list
})

const availableCount = computed(() => {
  return productsStore.products.filter((p) => p.isAvailable && p.stock > 0).length
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
.object-fit-cover {
  object-fit: cover;
}
</style>
