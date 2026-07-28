<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6 col-lg-5">
        <div class="card shadow">
          <div class="card-body">
            <h2 class="text-center mb-3">Cafetería Elay Puej</h2>
            <h5 class="text-center text-muted mb-4">Verificación de Correo</h5>
            
            <div v-if="!codeSent" class="text-center">
              <p>Se enviará un código de verificación a tu correo:</p>
              <p class="fw-bold">{{ email }}</p>
              <button class="btn btn-primary w-100 mt-3" @click="sendCode" :disabled="loading">
                {{ loading ? 'Enviando...' : 'Enviar Código de Verificación' }}
              </button>
              <div v-if="error" class="alert alert-danger mt-3">{{ error }}</div>
              <div v-if="success" class="alert alert-success mt-3">
                {{ success }}
              </div>
            </div>
            
            <div v-else>
              <p class="text-center text-muted">Ingresa el código de 6 dígitos que recibiste por correo</p>
              <form @submit.prevent="verifyCode">
                <div class="mb-3">
                  <label class="form-label">Código de Verificación</label>
                  <input 
                    type="text" 
                    class="form-control text-center" 
                    v-model="code" 
                    maxlength="6"
                    placeholder="Ingresa el código de 6 dígitos"
                    required
                  >
                </div>
                <div v-if="error" class="alert alert-danger">{{ error }}</div>
                <button type="submit" class="btn btn-success w-100" :disabled="loading">
                  {{ loading ? 'Verificando...' : 'Verificar Correo' }}
                </button>
                <div class="text-center mt-3">
                  <button type="button" class="btn btn-link" @click="resendCode">Reenviar código</button>
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
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const email = ref('')
const code = ref('')
const codeSent = ref(false)
const loading = ref(false)
const error = ref('')
const success = ref('')

onMounted(() => {
  email.value = (route.query.email || authStore.pendingVerificationEmail || authStore.user?.email || '').toString()

  if (!email.value) {
    router.push('/login')
    return
  }

  if (route.query.email || authStore.pendingVerificationEmail) {
    codeSent.value = true
    success.value = 'Ingresa el código que fue enviado a tu correo electrónico'
  }
})

const sendCode = async () => {
  loading.value = true
  error.value = ''
  success.value = ''
  
  const result = await authStore.sendVerificationCode(email.value)
  loading.value = false
  
  if (result.success) {
    codeSent.value = true
    success.value = 'Código enviado a tu correo electrónico'
  } else {
    error.value = result.message
  }
}

const verifyCode = async () => {
  if (code.value.length !== 6) {
    error.value = 'El código debe tener 6 dígitos'
    return
  }
  
  loading.value = true
  error.value = ''
  
  const result = await authStore.verifyEmail(email.value, code.value)
  loading.value = false
  
  if (result.success) {
    router.push('/login')
  } else {
    error.value = result.message
  }
}

const resendCode = async () => {
  codeSent.value = false
  success.value = ''
  error.value = ''
  await sendCode()
}
</script>