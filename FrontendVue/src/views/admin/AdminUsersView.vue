<template>
  <div class="container-fluid mt-4">
    <div class="row">
      <div class="col-md-3 col-lg-2">
        <AdminSidebar />
      </div>
      <div class="col-md-9 col-lg-10">
        <h2 class="mb-4">Gestión de Usuarios</h2>
        
        <div class="table-responsive">
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Email</th>
                <th>Teléfono</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Verificado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in adminStore.users" :key="user.id">
                <td>{{ user.id }}</td>
                <td>{{ user.fullName }}</td>
                <td>{{ user.email }}</td>
                <td>{{ user.phone || 'N/A' }}</td>
                <td>
                  <span class="badge" :class="user.role === 'admin' ? 'bg-danger' : user.role === 'worker' ? 'bg-info' : 'bg-secondary'">
                    {{ user.role }}
                  </span>
                </td>
                <td>
                  <span class="badge" :class="user.isActive ? 'bg-success' : 'bg-danger'">
                    {{ user.isActive ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td>
                  <span class="badge" :class="user.isEmailVerified ? 'bg-success' : 'bg-warning'">
                    {{ user.isEmailVerified ? 'Verificado' : 'Pendiente' }}
                  </span>
                </td>
                <td>
                  <button v-if="user.role !== 'admin'" class="btn btn-sm btn-warning me-1" @click="toggleUser(user.id, user.isActive)">
                    {{ user.isActive ? 'Desactivar' : 'Activar' }}
                  </button>
                  <button v-if="user.role !== 'admin'" class="btn btn-sm btn-danger" @click="confirmDelete(user.id)">
                    Eliminar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useAdminStore } from '../../stores/admin'
import AdminSidebar from '../../components/admin/AdminSidebar.vue'

const adminStore = useAdminStore()

onMounted(async () => {
  await adminStore.fetchUsers()
})

const toggleUser = async (userId, isActive) => {
  const action = isActive ? 'desactivar' : 'activar'
  if (confirm(`¿Estás seguro de ${action} este usuario?`)) {
    const result = await adminStore.toggleUserStatus(userId)
    if (result.success) {
      await adminStore.fetchUsers()
    }
  }
}

const confirmDelete = async (userId) => {
  if (confirm('¿Estás seguro de eliminar este usuario?')) {
    const result = await adminStore.deleteUser(userId)
    if (result.success) {
      await adminStore.fetchUsers()
    }
  }
}
</script>