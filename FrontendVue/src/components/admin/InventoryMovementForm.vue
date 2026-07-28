<template>
  <div class="modal fade" id="inventoryModal" tabindex="-1">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Registrar Movimiento de Inventario</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <form @submit.prevent="submitForm">
          <div class="modal-body">
            <div v-if="errorMessage" class="alert alert-danger alert-dismissible fade show" role="alert">
              {{ errorMessage }}
              <button type="button" class="btn-close" @click="errorMessage = ''"></button>
            </div>
            <div class="mb-3">
              <label class="form-label">Producto</label>
              <select class="form-select" v-model="form.productId" required>
                <option value="">Seleccionar producto</option>
                <option v-for="p in products" :key="p.id" :value="p.id">{{ p.name }}</option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label">Tipo de Movimiento</label>
              <select class="form-select" v-model="form.movementType" required>
                <option value="entry">Entrada</option>
                <option value="exit">Salida</option>
                <option value="adjustment">Ajuste</option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label">Cantidad</label>
              <input type="number" class="form-control" v-model="form.quantity" required>
            </div>
            <div class="mb-3">
              <label class="form-label">Motivo</label>
              <input type="text" class="form-control" v-model="form.reason" placeholder="Ej: Compra a proveedor">
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
            <button type="submit" class="btn btn-primary" :disabled="loading">{{ loading ? 'Guardando...' : 'Registrar' }}</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Modal } from 'bootstrap'
import { useAdminStore } from '../../stores/admin'

const props = defineProps({
  products: {
    type: Array,
    required: true
  }
})

const emit = defineEmits(['saved'])

const adminStore = useAdminStore()
const loading = ref(false)
const errorMessage = ref('')
const form = ref({
  productId: '',
  movementType: 'entry',
  quantity: 0,
  reason: ''
})

const submitForm = async () => {
  loading.value = true
  errorMessage.value = ''
  const result = await adminStore.createMovement(form.value)
  loading.value = false
  if (result.success) {
    const modal = document.getElementById('inventoryModal')
    const bsModal = Modal.getInstance(modal)
    if (bsModal) bsModal.hide()
    emit('saved')
  } else {
    errorMessage.value = result.message || 'Error al registrar movimiento'
  }
}
</script>