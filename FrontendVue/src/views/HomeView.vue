<template>
  <div class="container mt-4">
    <div class="jumbotron bg-light p-5 rounded">
      <h1 class="display-4">Cafetería Elay Puej</h1>
      <p class="lead">Café que cuenta la historia del origen.</p>
      <hr class="my-4">
      <p>Granos de origen único, tostados en pequeños lotes. Cada extracción es el resultado de años de relación directa con productores.</p>
      <router-link to="/products" class="btn btn-primary btn-lg">Ver Menú</router-link>
    </div>
    
    <div class="row mt-4" v-if="authStore.isAuthenticated">
      <div class="col-md-4" v-if="authStore.isAdmin">
        <div class="card text-center">
          <div class="card-body">
            <h5 class="card-title">Administración</h5>
            <p class="card-text">Gestiona productos, inventario y pedidos</p>
            <router-link to="/admin" class="btn btn-dark">Ir al Panel</router-link>
          </div>
        </div>
      </div>
      <div class="col-md-4" v-if="authStore.isWorker">
        <div class="card text-center">
          <div class="card-body">
            <h5 class="card-title">Dashboard</h5>
            <p class="card-text">Visualiza pedidos y estadísticas</p>
            <router-link to="/dashboard" class="btn btn-info">Ir al Dashboard</router-link>
          </div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="card text-center">
          <div class="card-body">
            <h5 class="card-title">Mis Pedidos</h5>
            <p class="card-text">Consulta el estado de tus pedidos</p>
            <router-link to="/orders" class="btn btn-success">Ver Pedidos</router-link>
          </div>
        </div>
      </div>
    </div>
    
    <div v-else class="row mt-4">
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