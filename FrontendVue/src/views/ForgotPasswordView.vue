<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6 col-lg-5">
        <div class="card shadow">
          <div class="card-body">
            <h2 class="text-center mb-3">Cafetería Elay Puej</h2>
            <h5 class="text-center text-muted mb-4">Recuperar Contraseña</h5>
            
            <div v-if="!codeSent">
              <p class="text-center">Ingresa tu correo para recibir un código de recuperación</p>
              <form @submit.prevent="sendRecoveryCode">
                <div class="mb-3">
                  <label class="form-label">Email</label>
                  <input type="email" class="form-control" v-model="email" required>
                </div>
                <div v-if="error" class="alert alert-danger">{{ error }}</div>
                <div v-if="success" class="alert alert-success">{{ success }}</div>
                <button type="submit" class="btn btn-primary w-100" :disabled="loading">
                  {{ loading ? 'Enviando...' : 'Enviar Código de Recuperación' }}
                </button>
              </form>
              <p class="text-center mt-3">
                <router-link to="/login">Volver al inicio de sesión</router-link>
              </p>
            </div>
            
            <div v-else>
              <p class="text-center text-muted">Ingresa el código de 6 dígitos y tu nueva contraseña</p>
              <form @submit.prevent="resetPassword">
                <div class="mb-3">
                  <label class="form-label">Código de Recuperación</label>
                  <input 
                    type="text" 
                    class="form-control text-center" 
                    v-model="code" 
                    maxlength="6"
                    placeholder="Ingresa el código de 6 dígitos"
                    required
                  >
                </div>
                <div class="mb-3">
                  <label class="form-label">Nueva Contraseña</label>
                  <input type="password" class="form-control" v-model="newPassword" minlength="6" required>
                </div>
                <div class="mb-3">
                  <label class="form-label">Confirmar Contraseña</label>
                  <input type="password" class="form-control" v-model="confirmPassword" minlength="6" required>
                </div>
                <div v-if="error" class="alert alert-danger">{{ error }}</div>
                <button type="submit" class="btn btn-success w-100" :disabled="loading">
                  {{ loading ? 'Restableciendo...' : 'Restablecer Contraseña' }}
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const email = ref('')
const code = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const codeSent = ref(false)
const loading = ref(false)
const error = ref('')
const success = ref('')

const sendRecoveryCode = async () => {
  loading.value = true
  error.value = ''
  success.value = ''
  
  const result = await authStore.forgotPassword(email.value)
  loading.value = false
  
  if (result.success) {
    codeSent.value = true
    success.value = 'Código enviado a tu correo electrónico'
  } else {
    error.value = result.message
  }
}

const resetPassword = async () => {
  if (code.value.length !== 6) {
    error.value = 'El código debe tener 6 dígitos'
    return
  }
  
  if (newPassword.value !== confirmPassword.value) {
    error.value = 'Las contraseñas no coinciden'
    return
  }
  
  if (newPassword.value.length < 6) {
    error.value = 'La contraseña debe tener al menos 6 caracteres'
    return
  }
  
  loading.value = true
  error.value = ''
  
  const result = await authStore.resetPassword(email.value, code.value, newPassword.value)
  loading.value = false
  
  if (result.success) {
    router.push('/login')
  } else {
    error.value = result.message
  }
}
</script>