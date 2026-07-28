<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="section-title" style="margin-bottom:0;">Gestion de Productos</h5>
      <button class="btn btn-primary" @click="openCreateModal">Nuevo Producto</button>
    </div>

    <div class="admin-card">
      <div class="card-body p-0">
        <div v-if="adminStore.loading" class="text-center py-4">
          <div class="spinner-border" style="color:var(--color-verde-medio);width:2rem;height:2rem;" role="status">
            <span class="visually-hidden">Cargando...</span>
          </div>
        </div>
        <div v-else-if="adminStore.products.length === 0" class="text-center py-4 text-muted">
          No hay productos registrados
        </div>
        <div v-else class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Categoria</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Estado</th>
                <th style="width:160px;">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in adminStore.products" :key="product.id">
                <td>#{{ product.id }}</td>
                <td>
                  <span style="font-weight:500;">{{ product.name }}</span>
                  <div style="font-size:0.75rem;color:var(--color-texto-claro);" v-if="product.flavorNotes">{{ product.flavorNotes }}</div>
                </td>
                <td>{{ product.categoryName || 'Sin categoria' }}</td>
                <td style="font-weight:500;color:var(--color-verde-medio);">${{ product.price.toFixed(2) }}</td>
                <td>
                  <span :class="product.stock <= product.minStock ? 'text-danger' : ''">
                    {{ product.stock }}
                  </span>
                  <span v-if="product.stock <= product.minStock && product.stock > 0" class="badge-modern badge-amarillo ms-1">Bajo</span>
                  <span v-if="product.stock === 0" class="badge-modern badge-rojo ms-1">Agotado</span>
                </td>
                <td>
                  <span class="badge-modern" :class="product.isAvailable ? 'badge-verde' : 'badge-rojo'">
                    {{ product.isAvailable ? 'Disponible' : 'No Disponible' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openEditModal(product)">Editar</button>
                  <button class="btn btn-danger btn-sm" @click="confirmDelete(product.id)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <ProductForm ref="productForm" :editing="editing" :product="selectedProduct" @saved="onSaved" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import ProductForm from '../../components/admin/ProductForm.vue'

const adminStore = useAdminStore()
const productForm = ref(null)
const editing = ref(false)
const selectedProduct = ref(null)

onMounted(async () => {
  await adminStore.fetchProducts()
  await adminStore.fetchCategories()
})

const openCreateModal = () => {
  editing.value = false
  selectedProduct.value = null
  const modal = new bootstrap.Modal(document.getElementById('productModal'))
  modal.show()
}

const openEditModal = (product) => {
  editing.value = true
  selectedProduct.value = { ...product }
  const modal = new bootstrap.Modal(document.getElementById('productModal'))
  modal.show()
}

const confirmDelete = async (id) => {
  if (confirm('Estas seguro de eliminar este producto?')) {
    const result = await adminStore.deleteProduct(id)
    if (result.success) {
      await adminStore.fetchProducts()
    }
  }
}

const onSaved = async () => {
  await adminStore.fetchProducts()
}
</script> 