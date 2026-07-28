<template>
  <div class="modal fade modal-modern" id="categoryModal" tabindex="-1">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ editing ? 'Editar Categoria' : 'Nueva Categoria' }}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="modal-body">
            <div v-if="errorMessage" class="alert alert-danger alert-dismissible fade show" role="alert">
              {{ errorMessage }}
              <button type="button" class="btn-close" @click="errorMessage = ''"></button>
            </div>
            <div class="mb-3">
              <label class="form-label" style="font-size:0.85rem;font-weight:500;">Nombre</label>
              <input type="text" class="form-modern" v-model="form.name" required>
            </div>
            <div class="mb-3">
              <label class="form-label" style="font-size:0.85rem;font-weight:500;">Descripcion</label>
              <textarea class="form-modern" v-model="form.description" rows="2"></textarea>
            </div>
            <div class="mb-3">
              <label class="form-label" style="font-size:0.85rem;font-weight:500;">Icono</label>
              <input type="text" class="form-modern" v-model="form.icon" placeholder="coffee, breakfast, bakery">
            </div>
            <div class="form-check">
              <input type="checkbox" class="form-check-input" id="catIsActive" v-model="form.isActive">
              <label class="form-check-label" for="catIsActive" style="font-size:0.85rem;">Activa</label>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" style="font-size:0.85rem;">Cancelar</button>
            <button type="submit" class="btn btn-primary" :disabled="loading" style="font-size:0.85rem;">{{ loading ? 'Guardando...' : 'Guardar' }}</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Modal } from 'bootstrap'
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
const errorMessage = ref('')

const defaultForm = () => ({
  name: '',
  description: '',
  icon: '',
  isActive: true
})

const form = ref(defaultForm())

watch(() => props.category, (newVal) => {
  errorMessage.value = ''
  if (newVal) {
    form.value = { ...newVal }
  } else {
    form.value = defaultForm()
  }
}, { immediate: true })

const submitForm = async () => {
  loading.value = true
  errorMessage.value = ''
  let result
  if (props.editing && props.category) {
    result = await adminStore.updateCategory(props.category.id, form.value)
  } else {
    result = await adminStore.createCategory(form.value)
  }
  loading.value = false
  if (result.success) {
    const modalEl = document.getElementById('categoryModal')
    if (modalEl) {
      const bsModal = Modal.getOrCreateInstance(modalEl)
      bsModal.hide()
    }
    setTimeout(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove())
      document.body.classList.remove('modal-open')
      document.body.style.removeProperty('overflow')
      document.body.style.removeProperty('padding-right')
    }, 300)
    emit('saved', { isEdit: props.editing, name: form.value.name })
  } else {
    errorMessage.value = result.message || 'Error al guardar categoría'
  }
}
</script>