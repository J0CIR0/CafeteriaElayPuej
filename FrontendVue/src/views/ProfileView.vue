<template>
  <div>
    <h2 class="fw-bold mb-4" style="color: var(--color-cafe);">
      <span style="border-left: 4px solid var(--color-cafe); padding-left: 12px;">Mi Perfil</span>
    </h2>

    <div class="row g-4">
      <div class="col-md-6">
        <div class="admin-card">
          <div class="card-header">Informacion Personal</div>
          <div class="card-body">
            <form @submit.prevent="updateProfile">
              <div class="mb-3">
                <label class="form-label">Nombre Completo</label>
                <input type="text" class="form-control form-control-cafe" v-model="profile.fullName" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Email</label>
                <input type="email" class="form-control form-control-cafe" v-model="profile.email" disabled>
              </div>
              <div class="mb-3">
                <label class="form-label">Telefono</label>
                <input type="text" class="form-control form-control-cafe" v-model="profile.phone">
              </div>
              <div class="mb-3">
                <label class="form-label">Rol</label>
                <input type="text" class="form-control form-control-cafe" :value="profile.role" disabled>
              </div>
              <button type="submit" class="btn btn-cafe" :disabled="loading">Actualizar Perfil</button>
            </form>
          </div>
        </div>
      </div>

      <div class="col-md-6">
        <div class="admin-card">
          <div class="card-header">Cambiar Contraseña</div>
          <div class="card-body">
            <form @submit.prevent="changePassword">
              <div class="mb-3">
                <label class="form-label">Contraseña Actual</label>
                <input type="password" class="form-control form-control-cafe" v-model="passwordData.currentPassword" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Nueva Contraseña</label>
                <input type="password" class="form-control form-control-cafe" v-model="passwordData.newPassword" required minlength="6">
              </div>
              <div class="mb-3">
                <label class="form-label">Confirmar Contraseña</label>
                <input type="password" class="form-control form-control-cafe" v-model="passwordData.confirmPassword" required minlength="6">
              </div>
              <button type="submit" class="btn btn-cafe" :disabled="loading">Cambiar Contraseña</button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import api from '../services/api'

const authStore = useAuthStore()
const loading = ref(false)
const profile = reactive({
  fullName: '',
  email: '',
  phone: '',
  role: ''
})

const passwordData = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

onMounted(() => {
  if (authStore.user) {
    profile.fullName = authStore.user.fullName || ''
    profile.email = authStore.user.email || ''
    profile.phone = authStore.user.phone || ''
    profile.role = authStore.user.role || ''
  }
})

const updateProfile = async () => {
  loading.value = true
  try {
    await api.put(`/Users/${authStore.user.id}`, {
      fullName: profile.fullName,
      email: profile.email,
      phone: profile.phone,
      role: profile.role
    })
    authStore.user.fullName = profile.fullName
    authStore.user.phone = profile.phone
    localStorage.setItem('user', JSON.stringify(authStore.user))
    alert('Perfil actualizado exitosamente')
  } catch (error) {
    alert(error.response?.data?.message || 'Error al actualizar perfil')
  } finally {
    loading.value = false
  }
}

const changePassword = async () => {
  if (passwordData.newPassword !== passwordData.confirmPassword) {
    alert('Las contraseñas no coinciden')
    return
  }
  if (passwordData.newPassword.length < 6) {
    alert('La contraseña debe tener al menos 6 caracteres')
    return
  }
  loading.value = true
  try {
    await api.post('/Auth/change-password', {
      currentPassword: passwordData.currentPassword,
      newPassword: passwordData.newPassword
    })
    alert('Contraseña actualizada exitosamente')
    passwordData.currentPassword = ''
    passwordData.newPassword = ''
    passwordData.confirmPassword = ''
  } catch (error) {
    alert(error.response?.data?.message || 'Error al cambiar contraseña')
  } finally {
    loading.value = false
  }
}
</script>