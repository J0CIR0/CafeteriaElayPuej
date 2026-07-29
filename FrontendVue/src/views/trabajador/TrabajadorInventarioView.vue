<template>
    <div class="d-flex">
        <!-- Sidebar del Trabajador para mantener el diseño de la segunda imagen -->
        

        <div class="flex-grow-1 p-4">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h2 class="fw-bold" style="color: var(--color-cafe);">
                    <span style="border-left: 4px solid var(--color-cafe); padding-left: 12px;">Control de Inventario</span>
                </h2>
                <button class="btn btn-cafe" @click="openMovementModal">
                    Registrar Movimiento
                </button>
            </div>

            <!-- Tarjetas de Resumen (Mismo diseño que Admin) -->
            <div class="row g-4 mb-4">
                <div class="col-md-3">
                    <div class="stat-card">
                        <div class="stat-number">{{ trabajadorStore.products.length }}</div>
                        <div class="stat-label">Total Productos</div>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="stat-card red">
                        <div class="stat-number">{{ trabajadorStore.products.filter(p => p.stock <= p.minStock).length }}</div>
                        <div class="stat-label">Stock Bajo</div>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="stat-card yellow">
                        <div class="stat-number">{{ trabajadorStore.products.filter(p => p.stock === 0).length }}</div>
                        <div class="stat-label">Sin Stock</div>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="stat-card green">
                        <div class="stat-number">{{ trabajadorStore.products.reduce((sum, p) => sum + p.stock, 0) }}</div>
                        <div class="stat-label">Stock Total</div>
                    </div>
                </div>
            </div>

            <!-- Tabla de Productos -->
            <div class="admin-card">
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-cafe">
                            <thead>
                                <tr>
                                    <th>ID</th>
                                    <th>Producto</th>
                                    <th>Stock Actual</th>
                                    <th>Stock Minimo</th>
                                    <th>Estado</th>
                                    <th>Acciones</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="product in trabajadorStore.products" :key="product.id">
                                    <td>#{{ product.id }}</td>
                                    <td>{{ product.name }}</td>
                                    <td>
                                        <span :class="product.stock <= product.minStock ? 'text-danger fw-bold' : ''">
                                            {{ product.stock }}
                                        </span>
                                    </td>
                                    <td>{{ product.minStock }}</td>
                                    <td>
                                        <span class="badge" :class="product.stock === 0 ? 'badge-rojo' : product.stock <= product.minStock ? 'badge-amarillo' : 'badge-verde'">
                                            {{ product.stock === 0 ? 'Agotado' : product.stock <= product.minStock ? 'Bajo' : 'Normal' }}
                                        </span>
                                    </td>
                                    <td>
                                        <button class="btn btn-sm btn-cafe" @click="openStockUpdate(product)">Ajustar Stock</button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <!-- Sección de Últimos Movimientos -->
            <div class="mt-4">
                <h4 class="fw-bold" style="color: var(--color-cafe);">Ultimos Movimientos</h4>
                <div class="admin-card">
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-cafe">
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
                                    <tr v-for="movement in trabajadorStore.inventoryMovements.slice(0, 20)" :key="movement.id">
                                        <td>{{ movement.product?.name || 'N/A' }}</td>
                                        <td>
                                            <span class="badge" :class="movement.movementType === 'entry' ? 'badge-verde' : movement.movementType === 'exit' ? 'badge-rojo' : 'badge-amarillo'">
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
                </div>
            </div>
        </div>

        <!-- Modales -->
        <InventoryMovementForm :products="trabajadorStore.products" @saved="onSaved" />

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
                            <button type="submit" class="btn btn-cafe" :disabled="loading">Guardar</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue'
    import { Modal } from 'bootstrap'
    import { useTrabajadorStore } from '../../stores/trabajador'
    import TrabajadorSidebar from '@/components/trabajador/TrabajadorSidebar.vue'
    import InventoryMovementForm from '../../components/admin/InventoryMovementForm.vue'

    const trabajadorStore = useTrabajadorStore()

    onMounted(async () => {
        // Asegúrate de que estos métodos existan en trabajadorStore
        await trabajadorStore.fetchProducts()
        await trabajadorStore.fetchInventoryMovements()
    })

    // ... (Resto de funciones: openMovementModal, openStockUpdate, submitStockUpdate, onSaved) ...
    const openMovementModal = () => {
        const el = document.getElementById('inventoryModal')
        if (el) {
            const modal = Modal.getOrCreateInstance(el)
            modal.show()
        }
    }

    const openStockUpdate = (product) => {
        stockProduct.value = product
        stockNewValue.value = product.stock
        stockReason.value = ''
        const el = document.getElementById('stockModal')
        if (el) {
            const modal = Modal.getOrCreateInstance(el)
            modal.show()
        }
    }

    const submitStockUpdate = async () => {
        loading.value = true
        const result = await adminStore.updateStock(stockProduct.value.id, stockNewValue.value, stockReason.value)
        loading.value = false
        if (result.success) {
            const modal = Modal.getInstance(document.getElementById('stockModal'))
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