<template>
  <div class="modal fade modal-modern" id="recipeModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Receta del Producto: <span class="text-primary">{{ productName }}</span></h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body">
          <div v-if="alert.show" :class="['alert', alert.type === 'success' ? 'alert-success' : 'alert-danger', 'alert-dismissible fade show mb-3']" role="alert">
            {{ alert.message }}
            <button type="button" class="btn-close" @click="alert.show = false"></button>
          </div>

          <!-- Financial Calculation Card -->
          <div class="card mb-4 bg-light border-0 shadow-sm">
            <div class="card-body py-3">
              <div class="row text-center">
                <div class="col-md-3">
                  <div class="small text-muted">Precio de Venta</div>
                  <div class="h5 mb-0 font-weight-bold text-dark">Bs. {{ salePrice.toFixed(2) }}</div>
                </div>
                <div class="col-md-3">
                  <div class="small text-muted">Costo Estimado</div>
                  <div class="h5 mb-0 font-weight-bold text-danger">Bs. {{ calculatedCost.toFixed(2) }}</div>
                </div>
                <div class="col-md-3">
                  <div class="small text-muted">Ganancia Neta</div>
                  <div class="h5 mb-0 font-weight-bold text-success">Bs. {{ (salePrice - calculatedCost).toFixed(2) }}</div>
                </div>
                <div class="col-md-3">
                  <div class="small text-muted">Margen (%)</div>
                  <div class="h5 mb-0 font-weight-bold" :class="profitMarginPercent >= 50 ? 'text-success' : 'text-warning'">
                    {{ profitMarginPercent.toFixed(1) }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <h6 class="fw-bold mb-3">Insumos Requeridos por Porción / Unidad</h6>

          <div v-if="recipeItems.length === 0" class="text-center py-3 text-muted">
            No se han agregado insumos a esta receta.
          </div>

          <div v-else class="table-responsive mb-3">
            <table class="table table-sm align-middle">
              <thead>
                <tr>
                  <th>Insumo</th>
                  <th style="width: 140px;">Cantidad</th>
                  <th>Unidad</th>
                  <th>Costo U.</th>
                  <th>Subtotal</th>
                  <th style="width: 50px;"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in recipeItems" :key="index">
                  <td>
                    <select class="form-select form-select-sm" v-model="item.ingredientId" @change="onIngredientSelect(item)">
                      <option value="">Seleccionar Insumo</option>
                      <option v-for="ing in adminStore.ingredients" :key="ing.id" :value="ing.id">
                        {{ ing.name }} (Bs {{ ing.unitCost }}/{{ ing.unitOfMeasure }})
                      </option>
                    </select>
                  </td>
                  <td>
                    <input type="number" step="0.01" min="0.01" class="form-control form-control-sm" v-model.number="item.quantityRequired">
                  </td>
                  <td><span class="badge bg-secondary">{{ getIngredientUnit(item.ingredientId) }}</span></td>
                  <td>Bs {{ getIngredientCost(item.ingredientId).toFixed(2) }}</td>
                  <td class="fw-bold">Bs {{ (item.quantityRequired * getIngredientCost(item.ingredientId)).toFixed(2) }}</td>
                  <td>
                    <button class="btn btn-outline-danger btn-sm p-1 px-2" @click="removeItem(index)">
                      &times;
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <button class="btn btn-sm btn-outline-primary" @click="addItem">
            + Agregar Insumo a la Receta
          </button>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="saving" @click="saveRecipe">
            {{ saving ? 'Guardando...' : 'Guardar Receta' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue'
import { Modal } from 'bootstrap'
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()

const productId = ref(null)
const productName = ref('')
const salePrice = ref(0)
const recipeItems = ref([])
const saving = ref(false)

const alert = reactive({
  show: false,
  message: '',
  type: 'success'
})

const getIngredientUnit = (id) => {
  const ing = adminStore.ingredients.find(i => i.id === Number(id))
  return ing ? ing.unitOfMeasure : '-'
}

const getIngredientCost = (id) => {
  const ing = adminStore.ingredients.find(i => i.id === Number(id))
  return ing ? Number(ing.unitCost) : 0
}

const onIngredientSelect = (item) => {
  if (!item.quantityRequired) item.quantityRequired = 1
}

const calculatedCost = computed(() => {
  return recipeItems.value.reduce((sum, item) => {
    const cost = getIngredientCost(item.ingredientId)
    return sum + ((Number(item.quantityRequired) || 0) * cost)
  }, 0)
})

const profitMarginPercent = computed(() => {
  if (!salePrice.value || salePrice.value <= 0) return 0
  const profit = salePrice.value - calculatedCost.value
  return (profit / salePrice.value) * 100
})

const addItem = () => {
  recipeItems.value.push({ ingredientId: '', quantityRequired: 1 })
}

const removeItem = (index) => {
  recipeItems.value.splice(index, 1)
}

const open = async (product) => {
  productId.value = product.id
  productName.value = product.name
  salePrice.value = Number(product.price) || 0
  alert.show = false

  await adminStore.fetchIngredients()
  const res = await adminStore.fetchProductRecipe(product.id)

  if (res.success && res.data && res.data.ingredients) {
    recipeItems.value = res.data.ingredients.map(i => ({
      ingredientId: i.ingredientId,
      quantityRequired: i.quantityRequired
    }))
  } else {
    recipeItems.value = []
  }

  const modalEl = document.getElementById('recipeModal')
  if (modalEl) {
    Modal.getOrCreateInstance(modalEl).show()
  }
}

const saveRecipe = async () => {
  saving.value = true
  alert.show = false

  const validItems = recipeItems.value
    .filter(i => i.ingredientId && Number(i.quantityRequired) > 0)
    .map(i => ({
      ingredientId: Number(i.ingredientId),
      quantityRequired: Number(i.quantityRequired)
    }))

  const result = await adminStore.saveProductRecipe(productId.value, validItems)
  saving.value = false

  if (result.success) {
    const modalEl = document.getElementById('recipeModal')
    if (modalEl) Modal.getOrCreateInstance(modalEl).hide()
    setTimeout(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove())
      document.body.classList.remove('modal-open')
    }, 300)
  } else {
    alert.message = result.message || 'Error al guardar la receta'
    alert.type = 'danger'
    alert.show = true
  }
}

defineExpose({ open })
</script>
