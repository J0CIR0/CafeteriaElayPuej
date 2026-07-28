<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
      <router-link class="navbar-brand" to="/">Cafetería Elay Puej</router-link>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav ms-auto">
          <li class="nav-item">
            <router-link class="nav-link" to="/">Inicio</router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/products">Menú</router-link>
          </li>
          <li class="nav-item" v-if="authStore.isAuthenticated">
            <router-link class="nav-link" to="/orders">Mis Pedidos</router-link>
          </li>
          <li class="nav-item" v-if="authStore.isAdmin">
            <router-link class="nav-link" to="/admin">Admin</router-link>
          </li>
          <li class="nav-item" v-if="authStore.isWorker && !authStore.isAdmin">
            <router-link class="nav-link" to="/dashboard">Dashboard</router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/cart">
              Carrito
              <span v-if="cartStore.totalItems > 0" class="badge bg-danger">{{ cartStore.totalItems }}</span>
            </router-link>
          </li>
          <li class="nav-item" v-if="!authStore.isAuthenticated">
            <router-link class="nav-link" to="/login">Iniciar Sesión</router-link>
          </li>
          <li class="nav-item" v-if="authStore.isAuthenticated">
            <button class="btn btn-outline-light btn-sm ms-2" @click="logout">Cerrar Sesión</button>
          </li>
        </ul>
      </div>
    </div>
  </nav>
  
  <router-view />
</template>

<script setup>
import { useAuthStore } from './stores/auth'
import { useCartStore } from './stores/cart'

const authStore = useAuthStore()
const cartStore = useCartStore()

const logout = () => {
  authStore.logout()
  window.location.href = '/'
}
</script>

<style>
.navbar-brand {
  font-weight: bold;
}
</style>