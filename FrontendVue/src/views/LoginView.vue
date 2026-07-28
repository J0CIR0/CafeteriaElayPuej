<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6 col-lg-4">
        <div class="card shadow">
          <div class="card-body">
            <h2 class="text-center mb-3">Cafetería Elay Puej</h2>
            <h5 class="text-center text-muted mb-4">Iniciar Sesión</h5>
            
            <div v-if="needsVerification" class="alert alert-warning">
              <p>Tu correo no está verificado. <router-link to="/verify-email">Verificar ahora</router-link></p>
            </div>
            
            <form @submit.prevent="handleLogin">
              <div class="mb-3">
                <label class="form-label">Email</label>
                <input type="email" class="form-control" v-model="email" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Contraseña</label>
                <input type="password" class="form-control" v-model="password" required>
              </div>
              <div v-if="error" class="alert alert-danger">{{ error }}</div>
              <button type="submit" class="btn btn-primary w-100" :disabled="loading">
                {{ loading ? 'Cargando...' : 'Iniciar Sesión' }}
              </button>
            </form>
            <div class="text-center mt-3">
              <router-link to="/forgot-password" class="d-block">¿Olvidaste tu contraseña?</router-link>
              <router-link to="/register">¿No tienes cuenta? Regístrate</router-link>
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
const password = ref('')
const loading = ref(false)
const error = ref('')
const needsVerification = ref(false)

const handleLogin = async () => {
  loading.value = true
  error.value = ''
  needsVerification.value = false
  
  const result = await authStore.login(email.value, password.value)
  loading.value = false
  
  if (result.success) {
    router.push('/')
  } else {
    error.value = result.message
    needsVerification.value = result.message.toLowerCase().includes('verificar')
  }
}
</script>