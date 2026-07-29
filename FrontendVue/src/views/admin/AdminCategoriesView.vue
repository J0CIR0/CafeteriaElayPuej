<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="section-title" style="margin-bottom:0;">Gestion de Categorias</h5>
      <button class="btn btn-primary" @click="openCreateModal">Nueva Categoria</button>
    </div>

    <!-- Alert Notification Banner -->
    <div v-if="alert.show" :class="['alert', alert.type === 'success' ? 'alert-success' : 'alert-danger', 'alert-dismissible fade show mb-4']" role="alert">
      <strong>{{ alert.type === 'success' ? '¡Éxito!' : '¡Error!' }}</strong> {{ alert.message }}
      <button type="button" class="btn-close" @click="alert.show = false"></button>
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
                  <div class="d-flex gap-1">
                    <button class="btn btn-action-icon btn-action-edit" @click="openEditModal(category)" title="Editar Categoría">
                      <i class="bi bi-pencil-fill"></i>
                    </button>
                    <button class="btn btn-action-icon btn-action-delete" @click="confirmDelete(category)" title="Eliminar Categoría">
                      <i class="bi bi-trash-fill"></i>
                    </button>
                  </div>
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
import { ref, reactive, onMounted } from 'vue'
import { Modal } from 'bootstrap'
import { useAdminStore } from '../../stores/admin'
import CategoryForm from '../../components/admin/CategoryForm.vue'

const adminStore = useAdminStore()
const categoryForm = ref(null)
const editing = ref(false)
const selectedCategory = ref(null)

const alert = reactive({
  show: false,
  message: '',
  type: 'success'
})

const showAlert = (message, type = 'success') => {
  alert.message = message
  alert.type = type
  alert.show = true
  setTimeout(() => {
    alert.show = false
  }, 4000)
}

onMounted(async () => {
  await adminStore.fetchCategories()
})

const openCreateModal = () => {
  editing.value = false
  selectedCategory.value = null
  const el = document.getElementById('categoryModal')
  if (el) {
    const modal = Modal.getOrCreateInstance(el)
    modal.show()
  }
}

const openEditModal = (category) => {
  editing.value = true
  selectedCategory.value = { ...category }
  const el = document.getElementById('categoryModal')
  if (el) {
    const modal = Modal.getOrCreateInstance(el)
    modal.show()
  }
}

const confirmDelete = async (category) => {
  if (confirm(`¿Estás seguro de eliminar la categoría "${category.name}"?`)) {
    const result = await adminStore.deleteCategory(category.id)
    if (result.success) {
      showAlert(`Categoría "${category.name}" eliminada exitosamente.`, 'success')
      await adminStore.fetchCategories()
    } else {
      showAlert(result.message || 'Error al eliminar la categoría. Puede tener productos asociados.', 'danger')
    }
  }
}

const onSaved = async (data) => {
  const actionText = data?.isEdit ? 'editada' : 'creada'
  const nameText = data?.name ? ` "${data.name}"` : ''
  showAlert(`Categoría${nameText} ${actionText} exitosamente.`, 'success')
  await adminStore.fetchCategories()
}
</script>