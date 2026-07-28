<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <h5 class="section-title mb-1">Gestion de Insumos y Materias Primas</h5>
        <p class="text-muted small mb-0">Administra los ingredientes, costos unitarios, stock y fechas de vencimiento de tu cafetería.</p>
      </div>
      <button class="btn btn-primary" @click="openCreateModal">+ Nuevo Insumo</button>
    </div>

    <!-- Alert Notification Banner -->
    <div v-if="alert.show" :class="['alert', alert.type === 'success' ? 'alert-success' : 'alert-danger', 'alert-dismissible fade show mb-4']" role="alert">
      <strong>{{ alert.type === 'success' ? '¡Éxito!' : '¡Error!' }}</strong> {{ alert.message }}
      <button type="button" class="btn-close" @click="alert.show = false"></button>
    </div>

    <div class="admin-card mb-4">
      <div class="card-body p-0">
        <div v-if="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Cargando insumos...</span>
          </div>
        </div>
        <div v-else-if="adminStore.ingredients.length === 0" class="text-center py-4 text-muted">
          No hay insumos registrados en el inventario.
        </div>
        <div v-else class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre del Insumo</th>
                <th>Stock Actual</th>
                <th>Unidad</th>
                <th>Costo Unitario</th>
                <th>Fecha Vencimiento</th>
                <th>Estado Stock</th>
                <th style="width:200px;">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="ing in adminStore.ingredients" :key="ing.id">
                <td>#{{ ing.id }}</td>
                <td><strong class="text-dark">{{ ing.name }}</strong></td>
                <td>
                  <span :class="ing.stockQuantity <= ing.minStockQuantity ? 'text-danger fw-bold' : ''">
                    {{ ing.stockQuantity }}
                  </span>
                </td>
                <td><span class="badge bg-secondary">{{ ing.unitOfMeasure }}</span></td>
                <td>Bs {{ Number(ing.unitCost).toFixed(2) }} / {{ ing.unitOfMeasure }}</td>
                <td>
                  <span v-if="ing.expirationDate" :class="isExpired(ing.expirationDate) ? 'badge bg-danger' : isNearExpiration(ing.expirationDate) ? 'badge bg-warning text-dark' : 'text-muted'">
                    {{ formatDate(ing.expirationDate) }}
                  </span>
                  <span v-else class="text-muted">-</span>
                </td>
                <td>
                  <span v-if="ing.stockQuantity <= 0" class="badge-modern badge-rojo">Agotado</span>
                  <span v-else-if="ing.stockQuantity <= ing.minStockQuantity" class="badge-modern badge-amarillo">Stock Bajo</span>
                  <span v-else class="badge-modern badge-verde">Suficiente</span>
                </td>
                <td>
                  <button class="btn btn-outline-warning btn-sm me-1" @click="openWasteModal(ing)" title="Registrar Pérdida por Vencimiento/Merma">
                    Pérdida
                  </button>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openEditModal(ing)">Editar</button>
                  <button class="btn btn-danger btn-sm" @click="confirmDelete(ing)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal Form (Crear / Editar Insumo) -->
    <div class="modal fade modal-modern" id="ingredientModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editing ? 'Editar Insumo' : 'Nuevo Insumo' }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form @submit.prevent="submitForm">
            <div class="modal-body">
              <div v-if="formError" class="alert alert-danger mb-3">{{ formError }}</div>

              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Nombre del Insumo / Materia Prima</label>
                <input type="text" class="form-modern" v-model="form.name" placeholder="Ej: Café en Grano, Azúcar, Leche, Harina" required>
              </div>

              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Unidad de Medida</label>
                  <select class="form-modern-select" v-model="form.unitOfMeasure" required>
                    <option value="g">Gramos (g)</option>
                    <option value="kg">Kilogramos (kg)</option>
                    <option value="ml">Mililitros (ml)</option>
                    <option value="L">Litros (L)</option>
                    <option value="unidad">Unidad (ud)</option>
                  </select>
                </div>
                <div class="col-md-6 mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Stock Inicial / Actual</label>
                  <input type="number" step="0.01" class="form-modern" v-model.number="form.stockQuantity" required>
                </div>
              </div>

              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Costo Unitario (Bs)</label>
                  <input type="number" step="0.01" class="form-modern" v-model.number="form.unitCost" placeholder="Ej: 0.05 Bs por gramo" required>
                </div>
                <div class="col-md-6 mb-3">
                  <label class="form-label" style="font-size:0.85rem;font-weight:500;">Stock Mínimo Alerta</label>
                  <input type="number" step="0.01" class="form-modern" v-model.number="form.minStockQuantity">
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Fecha de Vencimiento (opcional)</label>
                <input type="date" class="form-modern" v-model="form.expirationDate">
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
              <button type="submit" class="btn btn-primary" :disabled="formLoading">
                {{ formLoading ? 'Guardando...' : 'Guardar Insumo' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal Registrar Baja por Vencimiento / Merma -->
    <div class="modal fade modal-modern" id="wasteModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header bg-warning text-dark">
            <h5 class="modal-title">Registrar Pérdida / Baja de Insumo</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form @submit.prevent="submitWaste">
            <div class="modal-body">
              <div v-if="wasteError" class="alert alert-danger mb-3">{{ wasteError }}</div>

              <p class="mb-3">
                Insumo: <strong>{{ selectedIngredient?.name }}</strong><br>
                Stock Disponible: <strong>{{ selectedIngredient?.stockQuantity }} {{ selectedIngredient?.unitOfMeasure }}</strong><br>
                Costo Unitario: <strong>Bs {{ Number(selectedIngredient?.unitCost || 0).toFixed(2) }}</strong>
              </p>

              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Cantidad Mermada / Vencida</label>
                <input type="number" step="0.01" min="0.01" class="form-modern" v-model.number="wasteForm.quantity" required>
              </div>

              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Motivo de la Pérdida</label>
                <select class="form-modern-select mb-2" v-model="wasteForm.reason">
                  <option value="Insumo Vencido">Insumo Vencido</option>
                  <option value="Dañado o Derramado">Dañado / Derramado</option>
                  <option value="Falta de Calidad / Mal Estado">Falta de Calidad / Mal Estado</option>
                  <option value="Ajuste por Diferencia de Inventario">Ajuste de Inventario</option>
                </select>
              </div>

              <div v-if="wasteForm.quantity > 0" class="alert alert-info py-2 small">
                Pérdida Económica Calculada: <strong>Bs {{ (wasteForm.quantity * Number(selectedIngredient?.unitCost || 0)).toFixed(2) }}</strong>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
              <button type="submit" class="btn btn-warning" :disabled="wasteLoading">
                {{ wasteLoading ? 'Registrando...' : 'Confirmar Pérdida' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { Modal } from 'bootstrap'
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()
const loading = ref(false)
const editing = ref(false)
const formLoading = ref(false)
const formError = ref('')
const selectedIngredient = ref(null)

const wasteLoading = ref(false)
const wasteError = ref('')
const wasteForm = reactive({
  quantity: 0,
  reason: 'Insumo Vencido'
})

const alert = reactive({
  show: false,
  message: '',
  type: 'success'
})

const showAlert = (msg, type = 'success') => {
  alert.message = msg
  alert.type = type
  alert.show = true
  setTimeout(() => { alert.show = false }, 4000)
}

const defaultForm = () => ({
  id: 0,
  name: '',
  unitOfMeasure: 'g',
  stockQuantity: 0,
  minStockQuantity: 100,
  unitCost: 0,
  expirationDate: ''
})

const form = ref(defaultForm())

onMounted(async () => {
  loading.value = true
  await adminStore.fetchIngredients()
  loading.value = false
})

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleDateString('es-ES')
}

const isExpired = (dateStr) => {
  if (!dateStr) return false
  return new Date(dateStr) < new Date()
}

const isNearExpiration = (dateStr) => {
  if (!dateStr) return false
  const exp = new Date(dateStr)
  const now = new Date()
  const diffDays = (exp - now) / (1000 * 3600 * 24)
  return diffDays >= 0 && diffDays <= 7
}

const openCreateModal = () => {
  editing.value = false
  formError.value = ''
  form.value = defaultForm()
  const modalEl = document.getElementById('ingredientModal')
  if (modalEl) Modal.getOrCreateInstance(modalEl).show()
}

const openEditModal = (ing) => {
  editing.value = true
  formError.value = ''
  selectedIngredient.value = ing
  form.value = {
    id: ing.id,
    name: ing.name,
    unitOfMeasure: ing.unitOfMeasure,
    stockQuantity: ing.stockQuantity,
    minStockQuantity: ing.minStockQuantity,
    unitCost: ing.unitCost,
    expirationDate: ing.expirationDate ? ing.expirationDate.split('T')[0] : ''
  }
  const modalEl = document.getElementById('ingredientModal')
  if (modalEl) Modal.getOrCreateInstance(modalEl).show()
}

const submitForm = async () => {
  formLoading.value = true
  formError.value = ''

  let res
  if (editing.value) {
    res = await adminStore.updateIngredient(form.value.id, form.value)
  } else {
    res = await adminStore.createIngredient(form.value)
  }

  formLoading.value = false

  if (res.success) {
    const modalEl = document.getElementById('ingredientModal')
    if (modalEl) Modal.getOrCreateInstance(modalEl).hide()
    setTimeout(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove())
      document.body.classList.remove('modal-open')
    }, 300)
    showAlert(`Insumo "${form.value.name}" ${editing.value ? 'editado' : 'creado'} exitosamente.`, 'success')
  } else {
    formError.value = res.message || 'Error al guardar insumo'
  }
}

const openWasteModal = (ing) => {
  selectedIngredient.value = ing
  wasteError.value = ''
  wasteForm.quantity = 0
  wasteForm.reason = 'Insumo Vencido'
  const modalEl = document.getElementById('wasteModal')
  if (modalEl) Modal.getOrCreateInstance(modalEl).show()
}

const submitWaste = async () => {
  if (!selectedIngredient.value) return
  wasteLoading.value = true
  wasteError.value = ''

  const res = await adminStore.registerWaste(selectedIngredient.value.id, wasteForm)
  wasteLoading.value = false

  if (res.success) {
    const modalEl = document.getElementById('wasteModal')
    if (modalEl) Modal.getOrCreateInstance(modalEl).hide()
    setTimeout(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove())
      document.body.classList.remove('modal-open')
    }, 300)
    showAlert(`Pérdida por vencimiento de "${selectedIngredient.value.name}" registrada exitosamente.`, 'success')
  } else {
    wasteError.value = res.message || 'Error al registrar pérdida'
  }
}

const confirmDelete = async (ing) => {
  if (confirm(`¿Estás seguro de eliminar el insumo "${ing.name}"?`)) {
    const res = await adminStore.deleteIngredient(ing.id)
    if (res.success) {
      showAlert(`Insumo "${ing.name}" eliminado exitosamente.`, 'success')
    } else {
      showAlert(res.message || 'Error al eliminar el insumo.', 'danger')
    }
  }
}
</script>
