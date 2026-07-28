<template>
  <div class="modal fade" id="productModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ editing ? 'Editar Producto' : 'Nuevo Producto' }}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="modal-body">
            <div class="row">
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label">Nombre</label>
                  <input type="text" class="form-control" v-model="form.name" required>
                </div>
              </div>
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label">Categoría</label>
                  <select class="form-select" v-model="form.categoryId" required>
                    <option value="">Seleccionar categoría</option>
                    <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                  </select>
                </div>
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label">Descripción</label>
              <textarea class="form-control" v-model="form.description" rows="2"></textarea>
            </div>
            <div class="row">
              <div class="col-md-4">
                <div class="mb-3">
                  <label class="form-label">Precio</label>
                  <input type="number" step="0.01" class="form-control" v-model="form.price" required>
                </div>
              </div>
              <div class="col-md-4">
                <div class="mb-3">
                  <label class="form-label">Stock</label>
                  <input type="number" class="form-control" v-model="form.stock" required>
                </div>
              </div>
              <div class="col-md-4">
                <div class="mb-3">
                  <label class="form-label">Stock Mínimo</label>
                  <input type="number" class="form-control" v-model="form.minStock">
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label">Tiempo de Preparación</label>
                  <input type="text" class="form-control" v-model="form.preparationTime" placeholder="Ej: 25 seg">
                </div>
              </div>
              <div class="col-md-6">
                <div class="mb-3">
                  <label class="form-label">Origen</label>
                  <input type="text" class="form-control" v-model="form.origin" placeholder="Ej: Huila, Colombia">
                </div>
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label">Notas de Sabor</label>
              <input type="text" class="form-control" v-model="form.flavorNotes" placeholder="Ej: Panela y cereza roja">
            </div>
            <div class="mb-3">
              <label class="form-label">URL de Imagen</label>
              <input type="text" class="form-control" v-model="form.imageUrl" placeholder="https://ejemplo.com/imagen.jpg">
            </div>
            <div class="form-check">
              <input type="checkbox" class="form-check-input" id="isAvailable" v-model="form.isAvailable">
              <label class="form-check-label" for="isAvailable">Disponible</label>
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
  product: {
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

watch(() => props.product, (newVal) => {
  if (newVal) {
    form.value = { ...newVal }
  }
}, { immediate: true })

const submitForm = async () => {
  loading.value = true
  let result
  if (props.editing) {
    result = await adminStore.updateProduct(props.product.id, form.value)
  } else {
    result = await adminStore.createProduct(form.value)
  }
  loading.value = false
  if (result.success) {
    const modal = document.getElementById('productModal')
    const bsModal = bootstrap.Modal.getInstance(modal)
    if (bsModal) bsModal.hide()
    emit('saved')
  }
}
</script>