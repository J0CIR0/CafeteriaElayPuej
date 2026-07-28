<template>
  <div class="modal fade modal-modern" id="productModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ editing ? 'Editar Producto' : 'Nuevo Producto' }}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="modal-body">
            <div v-if="errorMessage" class="alert alert-danger alert-dismissible fade show" role="alert">
              {{ errorMessage }}
              <button type="button" class="btn-close" @click="errorMessage = ''"></button>
            </div>
            <div class="row">
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Nombre</label>
                  <input type="text" class="form-modern" v-model="form.name" required>
                </div>
              </div>
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Categoria</label>
                  <select class="form-modern-select" v-model="form.categoryId" required>
                    <option value="">Seleccionar</option>
                    <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                  </select>
                </div>
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label" style="font-size:0.85rem;font-weight:500;">Descripcion</label>
              <textarea class="form-modern" v-model="form.description" rows="2"></textarea>
            </div>
            <div class="row">
              <div class="col-md-4">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Precio</label>
                  <input type="number" step="0.01" class="form-modern" v-model="form.price" required>
                </div>
              </div>
              <div class="col-md-4">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Stock</label>
                  <input type="number" class="form-modern" v-model="form.stock" required>
                </div>
              </div>
              <div class="col-md-4">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Stock Minimo</label>
                  <input type="number" class="form-modern" v-model="form.minStock">
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Tiempo Preparacion</label>
                  <input type="text" class="form-modern" v-model="form.preparationTime" placeholder="25 seg">
                </div>
              </div>
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Origen</label>
                  <input type="text" class="form-modern" v-model="form.origin" placeholder="Huila, Colombia">
                </div>
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label" style="font-size:0.85rem;font-weight:500;">Notas de Sabor</label>
              <input type="text" class="form-modern" v-model="form.flavorNotes" placeholder="Panela y cereza roja">
            </div>
            <div class="mb-3">
              <label class="form-label" style="font-size:0.85rem;font-weight:500;">URL Imagen</label>
              <input type="text" class="form-modern" v-model="form.imageUrl" placeholder="https://ejemplo.com/imagen.jpg">
            </div>
            <div class="form-check">
              <input type="checkbox" class="form-check-input" id="isAvailable" v-model="form.isAvailable">
              <label class="form-check-label" for="isAvailable" style="font-size:0.85rem;">Disponible</label>
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
import { computed, ref, watch } from 'vue'
import { Modal } from 'bootstrap'
import { useAdminStore } from '../../stores/admin'

const props = defineProps({
  editing: {
    type: Boolean,
    default: false
  },
  product: {
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
  price: 0,
  stock: 0,
  minStock: 5,
  categoryId: '',
  preparationTime: '',
  origin: '',
  flavorNotes: '',
  imageUrl: '',
  isAvailable: true
})

const form = ref(defaultForm())

watch(() => props.product, (newVal) => {
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
  if (props.editing && props.product) {
    result = await adminStore.updateProduct(props.product.id, form.value)
  } else {
    result = await adminStore.createProduct(form.value)
  }
  loading.value = false
  if (result.success) {
    const modalEl = document.getElementById('productModal')
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
    errorMessage.value = result.message || 'Error al guardar producto'
  }
}

const categories = computed(() => adminStore.categories)
</script>