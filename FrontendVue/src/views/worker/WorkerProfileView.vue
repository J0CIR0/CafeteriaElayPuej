<template>
  <div class="container-fluid px-4 py-2">
    <!-- Header -->
    <div class="mb-4">
      <h2 class="fw-bold m-0" style="color: #ffffff;">
        <span style="border-left: 4px solid var(--color-verde-selva); padding-left: 12px;">
          Mi Perfil Personal
        </span>
      </h2>
      <p class="text-muted small m-0 mt-1 ms-3">
        Gestión de datos del trabajador y actualización de contraseña de acceso
      </p>
    </div>

    <!-- Feedback Alerts -->
    <div v-if="alertMessage" class="alert alert-dismissible fade show" :class="alertSuccess ? 'alert-success' : 'alert-danger'" role="alert">
      {{ alertMessage }}
      <button type="button" class="btn-close" @click="alertMessage = ''"></button>
    </div>

    <div class="row g-4">
      <!-- Profile Information Card -->
      <div class="col-md-6">
        <div class="admin-card h-100">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Información Personal
          </div>
          <div class="card-body">
            <form @submit.prevent="updateProfile">
              <div class="mb-3">
                <label class="form-label fw-bold">Nombre Completo</label>
                <input type="text" class="form-control form-modern" v-model="profile.fullName" required>
              </div>
              <div class="mb-3">
                <label class="form-label fw-bold">Correo Electrónico</label>
                <input type="email" class="form-control form-modern" v-model="profile.email" disabled>
                <small class="text-muted">El correo electrónico no puede ser modificado.</small>
              </div>
              <div class="mb-3">
                <label class="form-label fw-bold">Teléfono de Contacto</label>
                <input type="text" class="form-control form-modern" v-model="profile.phone" placeholder="Ej: +591 70000000">
              </div>
              <div class="mb-3">
                <label class="form-label fw-bold">Rol Asignado</label>
                <input type="text" class="form-control form-modern text-capitalize" value="Trabajador (Worker)" disabled>
              </div>
              <button type="submit" class="btn btn-primary w-100" :disabled="loading">
                <span v-if="loading" class="spinner-border spinner-border-sm me-1" role="status"></span>
                <span>Guardar Cambios de Perfil</span>
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- Change Password Card -->
      <div class="col-md-6">
        <div class="admin-card h-100">
          <div class="card-header fw-bold" style="color: var(--color-cafe-tostado);">
            Cambiar Contraseña
          </div>
          <div class="card-body">
            <form @submit.prevent="changePassword">
              <div class="mb-3">
                <label class="form-label fw-bold">Contraseña Actual</label>
                <input type="password" class="form-control form-modern" v-model="passwordData.currentPassword" required>
              </div>
              <div class="mb-3">
                <label class="form-label fw-bold">Nueva Contraseña</label>
                <input type="password" class="form-control form-modern" v-model="passwordData.newPassword" required minlength="6">
              </div>
              <div class="mb-3">
                <label class="form-label fw-bold">Confirmar Nueva Contraseña</label>
                <input type="password" class="form-control form-modern" v-model="passwordData.confirmPassword" required minlength="6">
              </div>
              <button type="submit" class="btn btn-primary w-100" :disabled="loadingPass">
                <span v-if="loadingPass" class="spinner-border spinner-border-sm me-1" role="status"></span>
                <span>Actualizar Contraseña</span>
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { useAuthStore } from '../../stores/auth'
import api from '../../services/api'

const authStore = useAuthStore()
const loading = ref(false)
const loadingPass = ref(false)
const alertMessage = ref('')
const alertSuccess = ref(true)

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

const showAlert = (msg, isSuccess = true) => {
  alertMessage.value = msg
  alertSuccess.value = isSuccess
}

const updateProfile = async () => {
  loading.value = true
  alertMessage.value = ''
  try {
    await api.put(`/Users/${authStore.user.id}`, {
      id: authStore.user.id,
      fullName: profile.fullName,
      email: profile.email,
      phone: profile.phone,
      role: authStore.user.role
    })
    authStore.user.fullName = profile.fullName
    authStore.user.phone = profile.phone
    localStorage.setItem('user', JSON.stringify(authStore.user))
    showAlert('Perfil actualizado exitosamente', true)
  } catch (err) {
    const msg = err.response?.data?.message || 'Error al actualizar el perfil'
    showAlert(msg, false)
  } finally {
    loading.value = false
  }
}

const changePassword = async () => {
  if (passwordData.newPassword !== passwordData.confirmPassword) {
    showAlert('Las contraseñas no coinciden', false)
    return
  }
  if (passwordData.newPassword.length < 6) {
    showAlert('La nueva contraseña debe tener al menos 6 caracteres', false)
    return
  }
  loadingPass.value = true
  alertMessage.value = ''
  try {
    await api.post('/Auth/change-password', {
      currentPassword: passwordData.currentPassword,
      newPassword: passwordData.newPassword
    })
    showAlert('Contraseña actualizada exitosamente', true)
    passwordData.currentPassword = ''
    passwordData.newPassword = ''
    passwordData.confirmPassword = ''
  } catch (err) {
    const msg = err.response?.data?.message || 'Error al cambiar la contraseña'
    showAlert(msg, false)
  } finally {
    loadingPass.value = false
  }
}
</script>

<style scoped>
</style>
