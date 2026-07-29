<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
      <div>
        <h2 class="fw-bold m-0" style="color: #ffffff;">
          <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
            Catálogo de Productos
          </span>
        </h2>
        <p style="color:white">
          Consulta de disponibilidad, precios y niveles de stock de productos
        </p>
      </div>
      <div>
        <button class="btn btn-outline-success btn-sm d-flex align-items-center gap-1" @click="refreshProducts" :disabled="workerStore.loading">
          <span v-if="workerStore.loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
          <span>Actualizar Lista</span>
        </button>
      </div>
    </div>

    <!-- Filters & Search Bar -->
    <div class="row g-3 mb-4">
      <div class="col-md-8">
        <div class="input-group shadow-sm">
          <span class="input-group-text bg-white border-end-0"><i class="bi bi-search"></i></span>
          <input type="text" class="form-control border-start-0" placeholder="Buscar producto por nombre..." v-model="searchQuery">
          <button v-if="searchQuery" class="btn btn-link text-muted" @click="searchQuery = ''">Limpiar</button>
        </div>
      </div>
      <div class="col-md-4">
        <select class="form-select shadow-sm" v-model="selectedCategory">
          <option value="">Todas las Categorías</option>
          <option v-for="cat in workerStore.categories" :key="cat.id" :value="cat.id">
            {{ cat.name }}
          </option>
        </select>
      </div>
    </div>

    <!-- Products Table (READ-ONLY) -->
    <div class="admin-card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-modern align-middle mb-0">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Categoría</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Estado</th>
                <th class="text-end">Acción</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filteredProducts.length === 0">
                <td colspan="7" class="text-center py-4 text-muted">No se encontraron productos en el catálogo.</td>
              </tr>
              <tr v-for="product in filteredProducts" :key="product.id" :class="{'table-danger-subtle': Number(product.stock) <= Number(product.minStock || 0)}">
                <td>#{{ product.id }}</td>
                <td class="fw-bold">
                  {{ product.name }}
                  <span v-if="Number(product.stock) <= Number(product.minStock || 0)" class="badge bg-danger ms-2">Stock Bajo</span>
                </td>
                <td>{{ getCategoryName(product.categoryId, product.categoryName) }}</td>
                <td class="fw-bold text-success">Bs. {{ Number(product.price).toFixed(2) }}</td>
                <td>
                  <span class="fw-bold" :class="Number(product.stock) <= Number(product.minStock || 0) ? 'text-danger fw-extrabold' : 'text-dark'">
                    {{ product.stock }}
                  </span>
                  <small class="text-muted ms-1">(Mín: {{ product.minStock || 0 }})</small>
                </td>
                <td>
                  <span class="badge badge-modern" :class="product.isAvailable ? 'badge-verde' : 'badge-rojo'">
                    {{ product.isAvailable ? 'Disponible' : 'No disponible' }}
                  </span>
                </td>
                <td class="text-end">
                  <button class="btn btn-sm btn-outline-info" @click="openProductDetailModal(product)">
                    Ver Detalle
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Product Detail Modal (READ-ONLY) -->
    <div class="modal fade" id="productDetailModal" tabindex="-1" aria-hidden="true" ref="productModalRef">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content modal-modern" v-if="selectedProduct">
          <div class="modal-header">
            <h5 class="modal-title text-white">Detalle de Producto: {{ selectedProduct.name }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div v-if="selectedProduct.imageUrl" class="text-center mb-3">
              <img :src="selectedProduct.imageUrl" :alt="selectedProduct.name" class="img-fluid rounded shadow-sm" style="max-height: 200px; object-fit: cover;">
            </div>

            <div class="mb-2"><strong>Categoría:</strong> {{ getCategoryName(selectedProduct.categoryId, selectedProduct.categoryName) }}</div>
            <div class="mb-2"><strong>Precio:</strong> <span class="text-success fw-bold">Bs. {{ Number(selectedProduct.price).toFixed(2) }}</span></div>
            <div class="mb-2"><strong>Stock Actual:</strong> {{ selectedProduct.stock }} (Mínimo: {{ selectedProduct.minStock || 0 }})</div>
            <div class="mb-2"><strong>Estado:</strong> {{ selectedProduct.isAvailable ? 'Disponible' : 'No disponible' }}</div>
            <div class="mb-2" v-if="selectedProduct.preparationTime"><strong>Tiempo de Preparación:</strong> {{ selectedProduct.preparationTime }} min</div>
            <div class="mb-2" v-if="selectedProduct.origin"><strong>Origen del Grano/Insumo:</strong> {{ selectedProduct.origin }}</div>
            <div class="mb-2" v-if="selectedProduct.flavorNotes"><strong>Notas de Sabor:</strong> {{ selectedProduct.flavorNotes }}</div>
            <hr>
            <div>
              <strong>Descripción:</strong>
              <p class="text-muted mt-1 mb-0">{{ selectedProduct.description || 'Sin descripción disponible.' }}</p>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useWorkerStore } from '../../stores/worker'

const workerStore = useWorkerStore()
const searchQuery = ref('')
const selectedCategory = ref('')
const selectedProduct = ref(null)
const productModalRef = ref(null)
let modalInstance = null

onMounted(async () => {
  await Promise.all([
    workerStore.fetchProducts(),
    workerStore.fetchCategories()
  ])
  if (window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(productModalRef.value)
  }
})

const refreshProducts = async () => {
  await workerStore.fetchProducts()
}

const filteredProducts = computed(() => {
  let list = workerStore.products

  if (selectedCategory.value) {
    list = list.filter(p => Number(p.categoryId) === Number(selectedCategory.value))
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim()
    list = list.filter(p => p.name && p.name.toLowerCase().includes(q))
  }

  return list
})

const getCategoryName = (catId, fallbackName) => {
  if (fallbackName) return fallbackName
  const cat = workerStore.categories.find(c => Number(c.id) === Number(catId))
  return cat ? cat.name : 'General'
}

const openProductDetailModal = (product) => {
  selectedProduct.value = product
  if (!modalInstance && window.bootstrap) {
    modalInstance = new window.bootstrap.Modal(productModalRef.value)
  }
  if (modalInstance) {
    modalInstance.show()
  }
}
</script>

<style scoped>
.table-danger-subtle {
  background-color: rgba(220, 53, 69, 0.08);
}
</style>
