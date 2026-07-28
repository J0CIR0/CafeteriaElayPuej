<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="section-title" style="margin-bottom:0;">Gestion de Categorias</h5>
      <button class="btn btn-primary" @click="openCreateModal">Nueva Categoria</button>
    </div>

    <div class="admin-card">
      <div class="card-body p-0">
        <div v-if="adminStore.loading" class="text-center py-4">
          <div class="spinner-border" style="color:var(--color-verde-medio);width:2rem;height:2rem;" role="status">
            <span class="visually-hidden">Cargando...</span>
          </div>
        </div>
        <div v-else-if="adminStore.categories.length === 0" class="text-center py-4 text-muted">
          No hay categorias registradas
        </div>
        <div v-else class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Descripcion</th>
                <th>Icono</th>
                <th>Estado</th>
                <th style="width:160px;">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="category in adminStore.categories" :key="category.id">
                <td>#{{ category.id }}</td>
                <td style="font-weight:500;">{{ category.name }}</td>
                <td>{{ category.description || '-' }}</td>
                <td>{{ category.icon || '-' }}</td>
                <td>
                  <span class="badge-modern" :class="category.isActive ? 'badge-verde' : 'badge-rojo'">
                    {{ category.isActive ? 'Activa' : 'Inactiva' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openEditModal(category)">Editar</button>
                  <button class="btn btn-danger btn-sm" @click="confirmDelete(category.id)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <CategoryForm ref="categoryForm" :editing="editing" :category="selectedCategory" @saved="onSaved" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
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
  const el = document.getElementById('categoryModal')
  if (el) {
    const modal = bootstrap.Modal.getOrCreateInstance(el)
    modal.show()
  }
}

const openEditModal = (category) => {
  editing.value = true
  selectedCategory.value = { ...category }
  const el = document.getElementById('categoryModal')
  if (el) {
    const modal = bootstrap.Modal.getOrCreateInstance(el)
    modal.show()
  }
}

const confirmDelete = async (id) => {
  if (confirm('Estas seguro de eliminar esta categoria?')) {
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