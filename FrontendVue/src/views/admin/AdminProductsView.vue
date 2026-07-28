<template>
  <div class="container-fluid mt-4">
    <div class="row">
      <div class="col-md-3 col-lg-2">
        <AdminSidebar />
      </div>
      <div class="col-md-9 col-lg-10">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h2>Gestión de Productos</h2>
          <button class="btn btn-primary" @click="openCreateModal">Nuevo Producto</button>
        </div>
        
        <div class="table-responsive">
          <table class="table table-striped table-hover">
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
              <tr v-for="product in adminStore.products" :key="product.id">
                <td>{{ product.id }}</td>
                <td>{{ product.name }}</td>
                <td>{{ product.categoryName || 'Sin categoría' }}</td>
                <td>${{ product.price.toFixed(2) }}</td>
                <td>
                  <span :class="product.stock <= product.minStock ? 'text-danger' : ''">
                    {{ product.stock }}
                  </span>
                  <span v-if="product.stock <= product.minStock" class="badge bg-danger ms-1">Bajo</span>
                </td>
                <td>
                  <span class="badge" :class="product.isAvailable ? 'bg-success' : 'bg-danger'">
                    {{ product.isAvailable ? 'Disponible' : 'No Disponible' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-sm btn-warning me-1" @click="openEditModal(product)">Editar</button>
                  <button class="btn btn-sm btn-danger" @click="confirmDelete(product.id)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        
        <ProductForm ref="productForm" :editing="editing" :product="selectedProduct" @saved="onSaved" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import AdminSidebar from '../../components/admin/AdminSidebar.vue'
import ProductForm from '../../components/admin/ProductForm.vue'

const adminStore = useAdminStore()
const productForm = ref(null)
const editing = ref(false)
const selectedProduct = ref(null)

onMounted(async () => {
  await adminStore.fetchProducts()
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
  if (confirm('¿Estás seguro de eliminar este producto?')) {
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