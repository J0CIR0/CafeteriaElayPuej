<template>
  <div class="modal fade" id="categoryModal" tabindex="-1">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ editing ? 'Editar Categoría' : 'Nueva Categoría' }}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Nombre</label>
              <input type="text" class="form-control" v-model="form.name" required>
            </div>
            <div class="mb-3">
              <label class="form-label">Descripción</label>
              <textarea class="form-control" v-model="form.description" rows="2"></textarea>
            </div>
            <div class="mb-3">
              <label class="form-label">Icono</label>
              <input type="text" class="form-control" v-model="form.icon" placeholder="coffee, breakfast, bakery, etc.">
            </div>
            <div class="form-check">
              <input type="checkbox" class="form-check-input" id="catIsActive" v-model="form.isActive">
              <label class="form-check-label" for="catIsActive">Activa</label>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
            <button type="submit" class="btn btn-primary" :disabled="loading">{{ loading ? 'Guardando...' : 'Guardar' }}</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useAdminStore } from '../../stores/admin'

const props = defineProps({
  editing: {
    type: Boolean,
    default: false
  },
  category: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['saved'])

const adminStore = useAdminStore()
const loading = ref(false)
const form = ref({
  name: '',
  description: '',
  icon: '',
  isActive: true
})

watch(() => props.category, (newVal) => {
  if (newVal) {
    form.value = { ...newVal }
  }
}, { immediate: true })

const submitForm = async () => {
  loading.value = true
  let result
  if (props.editing) {
    result = await adminStore.updateCategory(props.category.id, form.value)
  } else {
    result = await adminStore.createCategory(form.value)
  }
  loading.value = false
  if (result.success) {
    const modal = document.getElementById('categoryModal')
    const bsModal = bootstrap.Modal.getInstance(modal)
    if (bsModal) bsModal.hide()
    emit('saved')
  }
}
</script>