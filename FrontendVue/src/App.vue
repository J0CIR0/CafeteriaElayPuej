<template>
  <div class="admin-wrapper" v-if="authStore.isAuthenticated">
    <nav class="admin-navbar navbar navbar-expand-lg navbar-dark">
      <div class="container">
        <router-link class="navbar-brand" to="/">
          Cafeteria Elay Puej
        </router-link>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin">Dashboard</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/products">Productos</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/categories">Categorias</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/orders">Pedidos</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/inventory">Inventario</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/insumos">Insumos y Recetas</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/finanzas">Finanzas y Pérdidas</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/users">Usuarios</router-link>
            </li>
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/admin/reports">Reportes</router-link>
            </li>
          </ul>
          <ul class="navbar-nav">
            <li class="nav-item" v-if="authStore.isAdmin">
              <router-link class="nav-link" to="/profile">Mi Perfil</router-link>
            </li>
            <li class="nav-item">
              <button class="btn btn-outline-light btn-sm ms-2" @click="logout">Cerrar Sesion</button>
            </li>
          </ul>
        </div>
      </div>
    </nav>
    <main class="app-shell-main py-4">
      <router-view />
    </main>
  </div>
  <div v-else>
    <router-view />
  </div>
</template>

<script setup>
import { useAuthStore } from './stores/auth'

const authStore = useAuthStore()

const logout = () => {
  authStore.logout()
  window.location.href = '/login'
}
</script>