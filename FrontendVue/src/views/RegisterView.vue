<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6 col-lg-4">
        <div class="card shadow">
          <div class="card-body">
            <h2 class="text-center mb-3">Cafetería Elay Puej</h2>
            <h5 class="text-center text-muted mb-4">Registro de Usuario</h5>
            
            <form @submit.prevent="handleRegister">
              <div class="mb-3">
                <label class="form-label">Nombre Completo</label>
                <input type="text" class="form-control" v-model="fullName" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Email</label>
                <input type="email" class="form-control" v-model="email" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Teléfono</label>
                <input type="text" class="form-control" v-model="phone">
              </div>
              <div class="mb-3">
                <label class="form-label">Contraseña</label>
                <input type="password" class="form-control" v-model="password" required minlength="6">
              </div>
              <div v-if="error" class="alert alert-danger">{{ error }}</div>
              <button type="submit" class="btn btn-primary w-100" :disabled="loading">
                {{ loading ? 'Cargando...' : 'Registrarse' }}
              </button>
            </form>
            <p class="text-center mt-3">
              ¿Ya tienes cuenta? <router-link to="/login">Inicia Sesión</router-link>
            </p>
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
const fullName = ref('')
const email = ref('')
const phone = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

const handleRegister = async () => {
  loading.value = true
  error.value = ''
  
  const result = await authStore.register({
    fullName: fullName.value,
    email: email.value,
    phone: phone.value,
    password: password.value,
    role: 'customer'
  })
  
  loading.value = false
  
  if (result.success) {
    router.push({ path: '/verify-email', query: { email: email.value } })
  } else {
    error.value = result.message
  }
}
</script>