<template>
  <div class="container-fluid admin-page px-3 px-lg-4">
    <section class="admin-hero admin-card mb-4 p-4 p-lg-5">
      <div class="row align-items-end g-4">
        <div class="col-lg-8">
          <span class="eyebrow-chip">Casona cruceña • café tradicional • gestión viva</span>
          <h1 class="admin-title mt-3 mb-2">¿Qué se te antoja, pariente?</h1>
          <p class="admin-lead mb-0">
            Administrá el horneao, los masacos, los refrescos y el personal desde un panel cálido, simple y bien camba.
            Todo sale directo del backend y se refleja al instante en el menú público.
          </p>
        </div>
        <div class="col-lg-4 text-lg-end">
          <div class="d-inline-flex flex-column gap-2 align-items-lg-end">
            <span class="badge-modern badge-cafe">Un cafecito pa' la tarde</span>
            <span class="text-muted small">{{ currentDateLabel }}</span>
          </div>
        </div>
      </div>
    </section>

    <div class="row g-3 mb-4">
      <div class="col-md-3" v-for="stat in statsCards" :key="stat.label">
        <div class="stat-card" :class="stat.variant">
          <div class="stat-label">{{ stat.label }}</div>
          <div class="stat-number">{{ stat.value }}</div>
          <div class="text-muted small mt-1">{{ stat.helper }}</div>
        </div>
      </div>
    </div>

    <div class="admin-card mb-4">
      <div class="card-body">
        <ul class="nav nav-pills admin-pills gap-2 flex-wrap">
          <li class="nav-item" v-for="tab in tabs" :key="tab.key">
            <button class="nav-link" :class="{ active: activeTab === tab.key }" @click="activeTab = tab.key">
              {{ tab.label }}
            </button>
          </li>
        </ul>
      </div>
    </div>

    <div v-if="activeTab === 'overview'" class="row g-4">
      <div class="col-lg-7">
        <div class="admin-card h-100">
          <div class="card-header d-flex justify-content-between align-items-center">
            <span>Menú de la casa</span>
            <router-link to="/products" class="btn btn-sm btn-primary-outline">Ver menú público</router-link>
          </div>
          <div class="card-body">
            <div class="row g-3">
              <div v-for="product in adminStore.products.slice(0, 6)" :key="product.id" class="col-md-6">
                <div class="menu-mini-card">
                  <div class="d-flex justify-content-between align-items-start gap-3">
                    <div>
                      <div class="menu-mini-badge">{{ product.categoryName }}</div>
                      <h3 class="menu-mini-title mb-1">{{ product.name }}</h3>
                      <p class="menu-mini-text mb-2">{{ product.description }}</p>
                    </div>
                    <div class="text-end">
                      <div class="price-tag">{{ formatBs(product.price) }}</div>
                      <div class="small text-muted">Stock {{ product.stock }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-5">
        <div class="admin-card h-100">
          <div class="card-header">Atajos del mostrador</div>
          <div class="card-body d-grid gap-3">
            <button class="btn btn-primary" @click="switchTo('products')">Nuevo producto</button>
            <button class="btn btn-primary-outline" @click="switchTo('categories')">Nueva categoría</button>
            <button class="btn btn-primary-outline" @click="switchTo('users')">Gestionar personal</button>
            <div class="tip-box">
              <strong>Tip camba:</strong> si cambiás un producto acá, el menú público se refresca con el mismo estado.
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-else-if="activeTab === 'products'" class="admin-card">
      <div class="card-header d-flex flex-column flex-lg-row justify-content-between align-items-lg-center gap-3">
        <div>
          <div class="section-title mb-1">CRUD de Productos</div>
          <div class="text-muted small">Platillos y bebidas en Bs, con estado disponible o agotado.</div>
        </div>
        <button class="btn btn-primary" @click="openProductModal()">Nuevo Producto</button>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Categoría</th>
                <th>Precio</th>
                <th>Stock</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="product in adminStore.products" :key="product.id">
                <td>
                  <div class="fw-semibold">{{ product.name }}</div>
                  <div class="text-muted small">{{ product.description }}</div>
                </td>
                <td>{{ product.categoryName }}</td>
                <td class="fw-semibold">{{ formatBs(product.price) }}</td>
                <td>
                  <span :class="product.stock <= product.minStock ? 'text-danger fw-semibold' : 'fw-semibold'">{{ product.stock }}</span>
                  <div class="small text-muted">Mín. {{ product.minStock }}</div>
                </td>
                <td>
                  <span class="badge-modern" :class="product.isAvailable && product.stock > 0 ? 'badge-verde' : 'badge-rojo'">
                    {{ product.isAvailable && product.stock > 0 ? 'Disponible' : 'Agotado' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openProductModal(product)">Editar</button>
                  <button class="btn btn-danger btn-sm" @click="confirmDeleteProduct(product.id)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-else-if="activeTab === 'categories'" class="admin-card">
      <div class="card-header d-flex flex-column flex-lg-row justify-content-between align-items-lg-center gap-3">
        <div>
          <div class="section-title mb-1">CRUD de Categorías</div>
          <div class="text-muted small">Secciones de la cafetería: horneao, bebidas calientes, masacos y refrescos típicos.</div>
        </div>
        <button class="btn btn-primary" @click="openCategoryModal()">Nueva Categoría</button>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>Categoría</th>
                <th>Descripción</th>
                <th>Icono</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="category in adminStore.categories" :key="category.id">
                <td class="fw-semibold">{{ category.name }}</td>
                <td>{{ category.description }}</td>
                <td>{{ category.icon }}</td>
                <td>
                  <span class="badge-modern" :class="category.isActive ? 'badge-verde' : 'badge-rojo'">
                    {{ category.isActive ? 'Activa' : 'Inactiva' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openCategoryModal(category)">Editar</button>
                  <button class="btn btn-danger btn-sm" @click="confirmDeleteCategory(category.id)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-else-if="activeTab === 'users'" class="admin-card">
      <div class="card-header d-flex flex-column flex-lg-row justify-content-between align-items-lg-center gap-3">
        <div>
          <div class="section-title mb-1">CRUD de Usuarios</div>
          <div class="text-muted small">Gestioná admins, meseros y clientes sin romper el panel.</div>
        </div>
        <button class="btn btn-primary" @click="openUserModal()">Nuevo Usuario</button>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Correo</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Verificado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in adminStore.users" :key="user.id">
                <td>
                  <div class="fw-semibold">{{ user.fullName }}</div>
                  <div class="text-muted small">{{ user.phone || 'Sin teléfono' }}</div>
                </td>
                <td>{{ user.email }}</td>
                <td>
                  <span class="badge-modern" :class="user.role === 'admin' ? 'badge-cafe' : user.role === 'mesero' ? 'badge-verde' : 'badge-gris'">
                    {{ roleLabel(user.role) }}
                  </span>
                </td>
                <td>
                  <span class="badge-modern" :class="user.isActive ? 'badge-verde' : 'badge-rojo'">
                    {{ user.isActive ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td>
                  <span class="badge-modern" :class="user.isEmailVerified ? 'badge-verde' : 'badge-amarillo'">
                    {{ user.isEmailVerified ? 'Verificado' : 'Pendiente' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openUserModal(user)">Editar</button>
                  <button class="btn btn-warning btn-sm me-1" @click="toggleUser(user)">{{ user.isActive ? 'Desactivar' : 'Activar' }}</button>
                  <button class="btn btn-danger btn-sm" @click="confirmDeleteUser(user.id)">Eliminar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-else class="admin-card p-4 text-center text-muted">
      Este módulo se mantiene dentro del panel principal para evitar duplicar lógica.
    </div>

    <div class="modal fade modal-modern" id="productModal" tabindex="-1">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ productForm.id ? 'Editar Producto' : 'Nuevo Producto' }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form @submit.prevent="saveProduct">
            <div class="modal-body">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label">Nombre</label>
                  <input class="form-modern" v-model="productForm.name" required />
                </div>
                <div class="col-md-6">
                  <label class="form-label">Categoría</label>
                  <select class="form-modern-select" v-model="productForm.categoryId" required>
                    <option value="">Seleccionar categoría</option>
                    <option v-for="category in adminStore.categories" :key="category.id" :value="category.id">{{ category.name }}</option>
                      <option v-for="category in activeCategories" :key="category.id" :value="category.id">{{ category.name }}</option>
                  </select>
                </div>
                <div class="col-12">
                  <label class="form-label">Descripción</label>
                  <textarea class="form-modern" rows="3" v-model="productForm.description"></textarea>
                </div>
                <div class="col-md-4">
                  <label class="form-label">Precio Bs</label>
                  <input class="form-modern" type="number" min="0" step="0.5" v-model.number="productForm.price" required />
                </div>
                <div class="col-md-4">
                  <label class="form-label">Stock</label>
                  <input class="form-modern" type="number" min="0" step="1" v-model.number="productForm.stock" required />
                </div>
                <div class="col-md-4">
                  <label class="form-label">Stock mínimo</label>
                  <input class="form-modern" type="number" min="0" step="1" v-model.number="productForm.minStock" required />
                </div>
                <div class="col-md-6">
                  <label class="form-label">Imagen</label>
                  <input class="form-modern" v-model="productForm.imageUrl" placeholder="https://..." />
                </div>
                <div class="col-md-6">
                  <label class="form-label">Tiempo de preparación</label>
                  <input class="form-modern" v-model="productForm.preparationTime" placeholder="10 min" />
                </div>
                <div class="col-md-6">
                  <label class="form-label">Origen</label>
                  <input class="form-modern" v-model="productForm.origin" placeholder="Santa Cruz" />
                </div>
                <div class="col-md-6">
                  <label class="form-label">Notas de sabor</label>
                  <input class="form-modern" v-model="productForm.flavorNotes" placeholder="Quesito y horno casero" />
                </div>
                <div class="col-12 d-flex gap-4 flex-wrap">
                  <div class="form-check">
                    <input class="form-check-input" type="checkbox" v-model="productForm.isAvailable" id="productAvailable" />
                    <label class="form-check-label" for="productAvailable">Disponible</label>
                  </div>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
              <button type="submit" class="btn btn-primary" :disabled="savingProduct">{{ savingProduct ? 'Guardando...' : 'Guardar' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <div class="modal fade modal-modern" id="categoryModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ categoryForm.id ? 'Editar Categoría' : 'Nueva Categoría' }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form @submit.prevent="saveCategory">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Nombre</label>
                <input class="form-modern" v-model="categoryForm.name" required />
              </div>
              <div class="mb-3">
                <label class="form-label">Descripción</label>
                <textarea class="form-modern" rows="3" v-model="categoryForm.description"></textarea>
              </div>
              <div class="mb-3">
                <label class="form-label">Icono</label>
                <input class="form-modern" v-model="categoryForm.icon" placeholder="bread, cup-hot, egg-fried" />
              </div>
              <div class="form-check">
                <input class="form-check-input" type="checkbox" v-model="categoryForm.isActive" id="categoryActive" />
                <label class="form-check-label" for="categoryActive">Activa</label>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
              <button type="submit" class="btn btn-primary" :disabled="savingCategory">{{ savingCategory ? 'Guardando...' : 'Guardar' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <div class="modal fade modal-modern" id="userModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ userForm.id ? 'Editar Usuario' : 'Nuevo Usuario' }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form @submit.prevent="saveUser">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Nombre completo</label>
                <input class="form-modern" v-model="userForm.fullName" required />
              </div>
              <div class="mb-3">
                <label class="form-label">Correo</label>
                <input class="form-modern" type="email" v-model="userForm.email" required />
              </div>
                <div class="mb-3">
                  <label class="form-label">Contraseña temporal</label>
                  <input class="form-modern" type="password" v-model="userForm.password" :required="!userForm.id" :placeholder="userForm.id ? 'No cambiar contraseña' : 'Clave inicial'" />
                </div>
              <div class="mb-3">
                <label class="form-label">Teléfono</label>
                <input class="form-modern" v-model="userForm.phone" placeholder="70000000" />
              </div>
              <div class="mb-3">
                <label class="form-label">Rol</label>
                <select class="form-modern-select" v-model="userForm.role" required>
                  <option value="admin">Admin</option>
                  <option value="mesero">Mesero</option>
                  <option value="cliente">Cliente</option>
                </select>
              </div>
              <div class="d-flex gap-4 flex-wrap">
                <div class="form-check">
                  <input class="form-check-input" type="checkbox" v-model="userForm.isActive" id="userActive" />
                  <label class="form-check-label" for="userActive">Activo</label>
                </div>
                <div class="form-check">
                  <input class="form-check-input" type="checkbox" v-model="userForm.isEmailVerified" id="userVerified" />
                  <label class="form-check-label" for="userVerified">Verificado</label>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
              <button type="submit" class="btn btn-primary" :disabled="savingUser">{{ savingUser ? 'Guardando...' : 'Guardar' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAdminStore } from '../../stores/admin'
import { formatBolivianos as formatBs } from '../../stores/cafeteriaData'

const adminStore = useAdminStore()
const route = useRoute()
const activeTab = ref('overview')
const savingProduct = ref(false)
const savingCategory = ref(false)
const savingUser = ref(false)

const tabs = [
  { key: 'overview', label: 'Resumen' },
  { key: 'products', label: 'Productos' },
  { key: 'categories', label: 'Categorías' },
  { key: 'users', label: 'Usuarios' },
  { key: 'orders', label: 'Pedidos' },
  { key: 'inventory', label: 'Inventario' },
  { key: 'reports', label: 'Reportes' }
]

const currentDateLabel = computed(() => new Date().toLocaleDateString('es-ES', {
  weekday: 'long',
  year: 'numeric',
  month: 'long',
  day: 'numeric'
}))

const productForm = reactive({
  id: null,
  name: '',
  description: '',
  price: 0,
  categoryId: '',
  imageUrl: '',
  stock: 0,
  minStock: 0,
  isAvailable: true,
  preparationTime: '',
  origin: '',
  flavorNotes: ''
})

const categoryForm = reactive({
  id: null,
  name: '',
  description: '',
  icon: '',
  isActive: true
})

const userForm = reactive({
  id: null,
  fullName: '',
  email: '',
  phone: '',
  password: '',
  role: 'cliente',
  isActive: true,
  isEmailVerified: false
})

const statsCards = computed(() => [
  {
    label: 'Productos',
    value: adminStore.statistics.totalProducts,
    helper: 'Sabores listos para vender',
    variant: 'green'
  },
  {
    label: 'Usuarios',
    value: adminStore.statistics.totalUsers,
    helper: 'Personal y clientes registrados',
    variant: 'blue'
  },
  {
    label: 'Pedidos',
    value: adminStore.statistics.totalOrders,
    helper: 'Seguimiento del mostrador',
    variant: 'yellow'
  },
  {
    label: 'Ingresos',
    value: formatBs(adminStore.statistics.totalRevenue),
    helper: 'Acumulado en bolivianos',
    variant: 'red'
  }
])
const activeCategories = computed(() => adminStore.categories.filter((category) => category.isActive !== false))

const roleLabel = (role) => ({ admin: 'Admin', mesero: 'Mesero', cliente: 'Cliente' }[role] || role)

const switchTo = (tabKey) => {
  activeTab.value = tabKey
}

const resetProductForm = () => {
  Object.assign(productForm, {
    id: null,
    name: '',
    description: '',
    price: 0,
    categoryId: activeCategories.value[0]?.id || '',
    imageUrl: '',
    stock: 0,
    minStock: 0,
    isAvailable: true,
    preparationTime: '',
    origin: '',
    flavorNotes: ''
  })
}

const resetCategoryForm = () => {
  Object.assign(categoryForm, {
    id: null,
    name: '',
    description: '',
    icon: '',
    isActive: true
  })
}

const resetUserForm = () => {
  Object.assign(userForm, {
    id: null,
    fullName: '',
    email: '',
    phone: '',
    password: '',
    role: 'cliente',
    isActive: true,
    isEmailVerified: false
  })
}

const openModal = (id) => {
  const element = document.getElementById(id)
  if (!element || typeof bootstrap === 'undefined') {
    return null
  }

  const modal = bootstrap.Modal.getOrCreateInstance(element)
  modal.show()
  return modal
}

const closeModal = (id) => {
  const element = document.getElementById(id)
  if (!element || typeof bootstrap === 'undefined') {
    return
  }

  const modal = bootstrap.Modal.getInstance(element)
  modal?.hide()
}

const openProductModal = (product = null) => {
  if (product) {
    Object.assign(productForm, {
      id: product.id,
      name: product.name,
      description: product.description,
      price: product.price,
      categoryId: product.categoryId,
      imageUrl: product.imageUrl || '',
      stock: product.stock,
      minStock: product.minStock,
      isAvailable: product.isAvailable,
      preparationTime: product.preparationTime || '',
      origin: product.origin || '',
      flavorNotes: product.flavorNotes || ''
    })
  } else {
    resetProductForm()
  }

  openModal('productModal')
}

const openCategoryModal = (category = null) => {
  if (category) {
    Object.assign(categoryForm, {
      id: category.id,
      name: category.name,
      description: category.description || '',
      icon: category.icon || '',
      isActive: category.isActive !== false
    })
  } else {
    resetCategoryForm()
  }

  openModal('categoryModal')
}

const openUserModal = (user = null) => {
  if (user) {
    Object.assign(userForm, {
      id: user.id,
      fullName: user.fullName,
      email: user.email,
      phone: user.phone || '',
      role: user.role,
      isActive: user.isActive,
      isEmailVerified: user.isEmailVerified
    })
  } else {
    resetUserForm()
  }

  openModal('userModal')
}

const saveProduct = async () => {
  savingProduct.value = true
  const payload = {
    name: productForm.name.trim(),
    description: productForm.description.trim(),
    price: Number(productForm.price),
    categoryId: Number(productForm.categoryId),
    imageUrl: productForm.imageUrl.trim(),
    stock: Number(productForm.stock),
    minStock: Number(productForm.minStock),
    isAvailable: productForm.isAvailable,
    preparationTime: productForm.preparationTime.trim(),
    origin: productForm.origin.trim(),
    flavorNotes: productForm.flavorNotes.trim()
  }

  const result = productForm.id
    ? await adminStore.updateProduct(productForm.id, payload)
    : await adminStore.createProduct(payload)

  savingProduct.value = false
  if (result.success) {
    closeModal('productModal')
    resetProductForm()
  }
}

const saveCategory = async () => {
  savingCategory.value = true
  const payload = {
    name: categoryForm.name.trim(),
    description: categoryForm.description.trim(),
    icon: categoryForm.icon.trim(),
    isActive: categoryForm.isActive
  }

  const result = categoryForm.id
    ? await adminStore.updateCategory(categoryForm.id, payload)
    : await adminStore.createCategory(payload)

  savingCategory.value = false
  if (result.success) {
    closeModal('categoryModal')
    resetCategoryForm()
  }
}

const saveUser = async () => {
  savingUser.value = true
  const payload = {
    fullName: userForm.fullName.trim(),
    email: userForm.email.trim(),
    phone: userForm.phone.trim(),
    password: userForm.password.trim(),
    role: userForm.role,
    isActive: userForm.isActive,
    isEmailVerified: userForm.isEmailVerified
  }

  const result = userForm.id
    ? await adminStore.updateUser(userForm.id, payload)
    : await adminStore.createUser(payload)

  savingUser.value = false
  if (result.success) {
    closeModal('userModal')
    resetUserForm()
  }
}

const confirmDeleteProduct = async (id) => {
  if (confirm('¿Seguro que querés borrar este producto?')) {
    await adminStore.deleteProduct(id)
  }
}

const confirmDeleteCategory = async (id) => {
  if (confirm('¿Seguro que querés borrar esta categoría?')) {
    await adminStore.deleteCategory(id)
  }
}

const confirmDeleteUser = async (id) => {
  if (confirm('¿Seguro que querés borrar este usuario?')) {
    await adminStore.deleteUser(id)
  }
}

const toggleUser = async (user) => {
  await adminStore.toggleUserStatus(user.id)
}

onMounted(async () => {
  await adminStore.fetchDashboardData()

  const routeTab = route.query.tab
  if (typeof routeTab === 'string' && tabs.some((tab) => tab.key === routeTab)) {
    activeTab.value = routeTab
  }
})

watch(
  () => route.query.tab,
  (tab) => {
    if (typeof tab === 'string' && tabs.some((item) => item.key === tab)) {
      activeTab.value = tab
    }
  }
)
</script>
