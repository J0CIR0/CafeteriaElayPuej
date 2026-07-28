<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="section-title" style="margin-bottom:0;">Gestion de Usuarios</h5>
      <button class="btn btn-primary" @click="openCreateModal">Nuevo Usuario</button>
    </div>

    <!-- Alert Notification Banner -->
    <div v-if="alert.show" :class="['alert', alert.type === 'success' ? 'alert-success' : 'alert-danger', 'alert-dismissible fade show mb-4']" role="alert">
      <strong>{{ alert.type === 'success' ? '¡Éxito!' : '¡Error!' }}</strong> {{ alert.message }}
      <button type="button" class="btn-close" @click="alert.show = false"></button>
    </div>

    <div class="admin-card">
      <div class="card-body p-0">
        <div v-if="adminStore.loading" class="text-center py-4">
          <div class="spinner-border" style="color:var(--color-verde-medio);width:2rem;height:2rem;" role="status">
            <span class="visually-hidden">Cargando...</span>
          </div>
        </div>
        <div v-else-if="adminStore.users.length === 0" class="text-center py-4 text-muted">
          No hay usuarios registrados
        </div>
        <div v-else class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Email</th>
                <th>Telefono</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Verificado</th>
                <th style="width:200px;">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in adminStore.users" :key="user.id">
                <td>#{{ user.id }}</td>
                <td style="font-weight:500;">{{ user.fullName }}</td>
                <td>{{ user.email }}</td>
                <td>{{ user.phone || '-' }}</td>
                <td>
                  <span class="badge-modern" :class="user.role === 'admin' ? 'badge-azul' : user.role === 'worker' ? 'badge-verde' : 'badge-gris'">
                    {{ user.role }}
                  </span>
                </td>
                <td>
                  <span class="badge-modern" :class="user.isActive ? 'badge-verde' : 'badge-rojo'">
                    {{ user.isActive ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td>
                  <span class="badge-modern" :class="user.isEmailVerified ? 'badge-verde' : 'badge-rojo'">
                    {{ user.isEmailVerified ? 'Verificado' : 'Pendiente' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-primary-outline btn-sm me-1" @click="openEditModal(user)">Editar</button>
                  <button v-if="user.role !== 'admin'" class="btn btn-warning btn-sm me-1" @click="toggleUser(user)">
                    {{ user.isActive ? 'Desactivar' : 'Activar' }}
                  </button>
                  <button v-if="user.role !== 'admin'" class="btn btn-danger btn-sm" @click="confirmDelete(user)">
                    Eliminar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div class="modal fade modal-modern" id="userModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editing ? 'Editar Usuario' : 'Nuevo Usuario' }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form @submit.prevent="saveUser">
            <div class="modal-body">
              <div v-if="modalError" class="alert alert-danger alert-dismissible fade show mb-3" role="alert">
                {{ modalError }}
                <button type="button" class="btn-close" @click="modalError = ''"></button>
              </div>
              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Nombre Completo</label>
                <input type="text" class="form-modern" v-model="editUser.fullName" required>
              </div>
              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Email</label>
                <input type="email" class="form-modern" v-model="editUser.email" required>
              </div>
              <div class="mb-3" v-if="!editing">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Contraseña temporal</label>
                <input type="password" class="form-modern" v-model="editUser.password" required>
              </div>
              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Telefono</label>
                <input type="text" class="form-modern" v-model="editUser.phone">
              </div>
              <div class="mb-3">
                <label class="form-label" style="font-size:0.85rem;font-weight:500;">Rol</label>
                <select class="form-modern-select" v-model="editUser.role" required>
                  <option value="admin">Administrador</option>
                  <option value="worker">Trabajador</option>
                  <option value="customer">Cliente</option>
                </select>
              </div>
              <div class="form-check mb-3">
                <input type="checkbox" class="form-check-input" id="userActive" v-model="editUser.isActive">
                <label class="form-check-label" for="userActive" style="font-size:0.85rem;">Activo</label>
              </div>
              <div class="form-check">
                <input type="checkbox" class="form-check-input" id="userVerified" v-model="editUser.isEmailVerified">
                <label class="form-check-label" for="userVerified" style="font-size:0.85rem;">Email Verificado</label>
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
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { Modal } from 'bootstrap'
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()
const loading = ref(false)
const editing = ref(false)
const modalError = ref('')

const alert = reactive({
  show: false,
  message: '',
  type: 'success'
})

const showAlert = (message, type = 'success') => {
  alert.message = message
  alert.type = type
  alert.show = true
  setTimeout(() => {
    alert.show = false
  }, 4000)
}

const editUser = reactive({
  id: null,
  fullName: '',
  email: '',
  phone: '',
  password: '',
  role: 'customer',
  isActive: true,
  isEmailVerified: false
})

onMounted(async () => {
  await adminStore.fetchUsers()
})

const openEditModal = (user) => {
  editing.value = true
  modalError.value = ''
  editUser.id = user.id
  editUser.fullName = user.fullName
  editUser.email = user.email
  editUser.phone = user.phone || ''
  editUser.password = ''
  editUser.role = user.role
  editUser.isActive = user.isActive
  editUser.isEmailVerified = user.isEmailVerified || false
  const el = document.getElementById('userModal')
  if (el) {
    const modal = Modal.getOrCreateInstance(el)
    modal.show()
  }
}

const openCreateModal = () => {
  editing.value = false
  modalError.value = ''
  editUser.id = null
  editUser.fullName = ''
  editUser.email = ''
  editUser.phone = ''
  editUser.password = ''
  editUser.role = 'customer'
  editUser.isActive = true
  editUser.isEmailVerified = false
  const el = document.getElementById('userModal')
  if (el) {
    const modal = Modal.getOrCreateInstance(el)
    modal.show()
  }
}

const saveUser = async () => {
  loading.value = true
  modalError.value = ''
  try {
    const payload = {
      fullName: editUser.fullName,
      email: editUser.email,
      phone: editUser.phone,
      password: editUser.password,
      role: editUser.role,
      isActive: editUser.isActive,
      isEmailVerified: editUser.isEmailVerified
    }

    let result
    if (editing.value) {
      result = await adminStore.updateUser(editUser.id, payload)
    } else {
      result = await adminStore.createUser(payload)
    }

    if (result.success) {
      const modalEl = document.getElementById('userModal')
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
      const actionText = editing.value ? 'editado' : 'creado'
      showAlert(`Usuario "${editUser.fullName}" ${actionText} exitosamente.`, 'success')
      await adminStore.fetchUsers()
    } else {
      modalError.value = result.message || 'Error al guardar usuario'
    }
  } catch (error) {
    modalError.value = error.response?.data?.message || 'Error al guardar usuario'
  } finally {
    loading.value = false
  }
}

const toggleUser = async (user) => {
  const action = user.isActive ? 'desactivar' : 'activar'
  if (confirm(`¿Estás seguro de ${action} al usuario "${user.fullName}"?`)) {
    const result = await adminStore.toggleUserStatus(user.id)
    if (result.success) {
      showAlert(`Estado del usuario "${user.fullName}" actualizado exitosamente.`, 'success')
      await adminStore.fetchUsers()
    } else {
      showAlert(result.message || 'Error al cambiar estado del usuario', 'danger')
    }
  }
}

const confirmDelete = async (user) => {
  if (confirm(`¿Estás seguro de eliminar al usuario "${user.fullName}"?`)) {
    const result = await adminStore.deleteUser(user.id)
    if (result.success) {
      showAlert(`Usuario "${user.fullName}" eliminado exitosamente.`, 'success')
      await adminStore.fetchUsers()
    } else {
      showAlert(result.message || 'Error al eliminar el usuario.', 'danger')
    }
  }
}
</script>