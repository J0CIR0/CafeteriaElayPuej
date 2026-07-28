<template>
  <div class="container-fluid mt-4">
    <div class="row">
      <div class="col-md-3 col-lg-2">
        <AdminSidebar />
      </div>
      <div class="col-md-9 col-lg-10">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h2>Gestión de Categorías</h2>
          <button class="btn btn-primary" @click="openCreateModal">Nueva Categoría</button>
        </div>
        
        <div class="row">
          <div v-for="category in adminStore.categories" :key="category.id" class="col-md-4 mb-3">
            <div class="card">
              <div class="card-body">
                <h5 class="card-title">
                  {{ category.name }}
                  <span class="badge" :class="category.isActive ? 'bg-success' : 'bg-danger'">
                    {{ category.isActive ? 'Activa' : 'Inactiva' }}
                  </span>
                </h5>
                <p class="card-text text-muted">{{ category.description || 'Sin descripción' }}</p>
                <p class="card-text"><small>Icono: {{ category.icon || 'N/A' }}</small></p>
                <button class="btn btn-sm btn-warning me-1" @click="openEditModal(category)">Editar</button>
                <button class="btn btn-sm btn-danger" @click="confirmDelete(category.id)">Eliminar</button>
              </div>
            </div>
          </div>
        </div>
        
        <CategoryForm ref="categoryForm" :editing="editing" :category="selectedCategory" @saved="onSaved" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import AdminSidebar from '../../components/admin/AdminSidebar.vue'
import CategoryForm from '../../components/admin/CategoryForm.vue'

const adminStore = useAdminStore()
const categoryForm = ref(null)
const editing = ref(false)
const selectedCategory = ref(null)

onMounted(async () => {
  await adminStore.fetchCategories()
})

const openCreateModal = () => {
  editing.value = false
  selectedCategory.value = null
  const modal = new bootstrap.Modal(document.getElementById('categoryModal'))
  modal.show()
}

const openEditModal = (category) => {
  editing.value = true
  selectedCategory.value = { ...category }
  const modal = new bootstrap.Modal(document.getElementById('categoryModal'))
  modal.show()
}

const confirmDelete = async (id) => {
  if (confirm('¿Estás seguro de eliminar esta categoría?')) {
    const result = await adminStore.deleteCategory(id)
    if (result.success) {
      await adminStore.fetchCategories()
    }
  }
}

const onSaved = async () => {
  await adminStore.fetchCategories()
}
</script>