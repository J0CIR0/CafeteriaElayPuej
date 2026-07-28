<template>
  <div class="container-fluid mt-4">
    <div class="row">
      <div class="col-md-3 col-lg-2">
        <AdminSidebar />
      </div>
      <div class="col-md-9 col-lg-10">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h2>Control de Inventario</h2>
          <button class="btn btn-primary" @click="openMovementModal">Registrar Movimiento</button>
        </div>
        
        <div class="row mb-4">
          <div class="col-md-3">
            <div class="card bg-light">
              <div class="card-body text-center">
                <h5>Total Productos</h5>
                <h3>{{ adminStore.products.length }}</h3>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-danger text-white">
              <div class="card-body text-center">
                <h5>Stock Bajo</h5>
                <h3>{{ adminStore.products.filter(p => p.stock <= p.minStock).length }}</h3>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-warning">
              <div class="card-body text-center">
                <h5>Sin Stock</h5>
                <h3>{{ adminStore.products.filter(p => p.stock === 0).length }}</h3>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-success text-white">
              <div class="card-body text-center">
                <h5>Stock Total</h5>
                <h3>{{ adminStore.products.reduce((sum, p) => sum + p.stock, 0) }}</h3>
              </div>
            </div>
          </div>
        </div>
        
        <div class="table-responsive">
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>ID</th>
                <th>Producto</th>
                <th>Stock Actual</th>
                <th>Stock Mínimo</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in adminStore.products" :key="product.id">
                <td>{{ product.id }}</td>
                <td>{{ product.name }}</td>
                <td>
                  <span :class="product.stock <= product.minStock ? 'text-danger fw-bold' : ''">
                    {{ product.stock }}
                  </span>
                </td>
                <td>{{ product.minStock }}</td>
                <td>
                  <span class="badge" :class="product.stock === 0 ? 'bg-danger' : product.stock <= product.minStock ? 'bg-warning' : 'bg-success'">
                    {{ product.stock === 0 ? 'Agotado' : product.stock <= product.minStock ? 'Bajo' : 'Normal' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-sm btn-primary" @click="openStockUpdate(product)">Ajustar Stock</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        
        <div class="mt-4">
          <h4>Últimos Movimientos</h4>
          <div class="table-responsive">
            <table class="table table-sm">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th>Tipo</th>
                  <th>Cantidad</th>
                  <th>Motivo</th>
                  <th>Fecha</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="movement in adminStore.inventoryMovements.slice(0, 20)" :key="movement.id">
                  <td>{{ movement.product?.name || 'N/A' }}</td>
                  <td>
                    <span class="badge" :class="movement.movementType === 'entry' ? 'bg-success' : movement.movementType === 'exit' ? 'bg-danger' : 'bg-warning'">
                      {{ movement.movementType === 'entry' ? 'Entrada' : movement.movementType === 'exit' ? 'Salida' : 'Ajuste' }}
                    </span>
                  </td>
                  <td>{{ movement.quantity }}</td>
                  <td>{{ movement.reason || 'N/A' }}</td>
                  <td>{{ new Date(movement.createdAt).toLocaleString() }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        
        <InventoryMovementForm :products="adminStore.products" @saved="onSaved" />
        
        <div class="modal fade" id="stockModal" tabindex="-1">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Ajustar Stock</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
              </div>
              <form @submit.prevent="submitStockUpdate">
                <div class="modal-body">
                  <p>Producto: <strong>{{ stockProduct?.name }}</strong></p>
                  <p>Stock actual: <strong>{{ stockProduct?.stock }}</strong></p>
                  <div class="mb-3">
                    <label class="form-label">Nuevo Stock</label>
                    <input type="number" class="form-control" v-model="stockNewValue" required>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Motivo</label>
                    <input type="text" class="form-control" v-model="stockReason" placeholder="Ej: Ajuste manual">
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
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import AdminSidebar from '../../components/admin/AdminSidebar.vue'
import InventoryMovementForm from '../../components/admin/InventoryMovementForm.vue'

const adminStore = useAdminStore()
const loading = ref(false)
const stockProduct = ref(null)
const stockNewValue = ref(0)
const stockReason = ref('')

onMounted(async () => {
  await adminStore.fetchProducts()
  await adminStore.fetchInventoryMovements()
})

const openMovementModal = () => {
  const modal = new bootstrap.Modal(document.getElementById('inventoryModal'))
  modal.show()
}

const openStockUpdate = (product) => {
  stockProduct.value = product
  stockNewValue.value = product.stock
  stockReason.value = ''
  const modal = new bootstrap.Modal(document.getElementById('stockModal'))
  modal.show()
}

const submitStockUpdate = async () => {
  loading.value = true
  const result = await adminStore.updateStock(stockProduct.value.id, stockNewValue.value, stockReason.value)
  loading.value = false
  if (result.success) {
    const modal = bootstrap.Modal.getInstance(document.getElementById('stockModal'))
    if (modal) modal.hide()
    await adminStore.fetchProducts()
    await adminStore.fetchInventoryMovements()
  }
}

const onSaved = async () => {
  await adminStore.fetchProducts()
  await adminStore.fetchInventoryMovements()
}
</script>