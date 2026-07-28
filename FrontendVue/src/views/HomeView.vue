<template>
  <div class="container mt-4">
    <div class="jumbotron bg-light p-5 rounded">
      <h1 class="display-4">Cafetería Elay Puej</h1>
      <p class="lead">Café que cuenta la historia del origen.</p>
      <hr class="my-4">
      <p>Granos de origen único, tostados en pequeños lotes. Cada extracción es el resultado de años de relación directa con productores.</p>
      <router-link to="/products" class="btn btn-primary btn-lg">Ver Menú</router-link>
    </div>
    
    
    <div v-if="!authStore.isAuthenticated" class="row mt-4">
      <div class="col text-center">
        <router-link to="/login" class="btn btn-primary me-2">Iniciar Sesión</router-link>
        <router-link to="/register" class="btn btn-outline-primary">Registrarse</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
</script>