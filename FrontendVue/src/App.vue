<template>
  <div class="admin-wrapper">
    <nav class="admin-navbar navbar navbar-expand-lg navbar-dark shadow-sm">
      <div class="container">
        <router-link class="navbar-brand d-flex align-items-center gap-2" to="/">
          <i class="bi bi-cup-hot-fill text-warning fs-4"></i>
          <span class="fw-bold fs-5">Cafetería Elay Puej</span>
        </router-link>
        <button class="navbar-toggler border-0" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <template v-if="authStore.isAuthenticated && authStore.isAdmin">
              <li class="nav-item">
                <router-link class="nav-link" to="/admin"><i class="bi bi-speedometer2 me-1"></i>Dashboard</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/products"><i class="bi bi-box-seam me-1"></i>Productos</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/categories"><i class="bi bi-tags me-1"></i>Categorías</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/orders"><i class="bi bi-receipt me-1"></i>Pedidos</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/inventory"><i class="bi bi-boxes me-1"></i>Inventario</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/insumos"><i class="bi bi-journal-text me-1"></i>Insumos & Recetas</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/finanzas"><i class="bi bi-cash-coin me-1"></i>Finanzas & Pérdidas</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/users"><i class="bi bi-people me-1"></i>Usuarios</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/admin/reports"><i class="bi bi-graph-up-arrow me-1"></i>Reportes</router-link>
              </li>
            </template>

            <template v-else-if="authStore.isAuthenticated && authStore.user?.role === 'worker'">
              <li class="nav-item">
                <router-link class="nav-link" to="/dashboard"><i class="bi bi-speedometer2 me-1"></i>Dashboard</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/dashboard/orders"><i class="bi bi-receipt me-1"></i>Pedidos</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/dashboard/clients"><i class="bi bi-people me-1"></i>Clientes</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/dashboard/products"><i class="bi bi-cup-straw me-1"></i>Productos</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/dashboard/inventory"><i class="bi bi-boxes me-1"></i>Inventario</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/dashboard/reports"><i class="bi bi-file-earmark-bar-graph me-1"></i>Reportes</router-link>
              </li>
            </template>

            <template v-else>
              <li class="nav-item">
                <router-link class="nav-link" to="/"><i class="bi bi-house me-1"></i>Inicio</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link" to="/products"><i class="bi bi-cup-hot me-1"></i>Menú</router-link>
              </li>
              <li class="nav-item" v-if="authStore.isAuthenticated">
                <router-link class="nav-link" to="/orders"><i class="bi bi-bag-check me-1"></i>Mis Pedidos</router-link>
              </li>
              <li class="nav-item">
                <router-link class="nav-link d-flex align-items-center gap-1" to="/cart">
                  <i class="bi bi-cart3 me-1"></i>
                  <span>Carrito</span>
                  <span v-if="cartStore.totalItems > 0" class="badge bg-terracota rounded-pill px-2 py-1">
                    {{ cartStore.totalItems }}
                  </span>
                </router-link>
              </li>
            </template>
          </ul>

          <ul class="navbar-nav align-items-center">
            <template v-if="authStore.isAuthenticated">
              <li class="nav-item" v-if="authStore.isAdmin">
                <router-link class="nav-link" to="/profile"><i class="bi bi-person-circle me-1"></i>Mi Perfil</router-link>
              </li>
              <li class="nav-item" v-else-if="authStore.user?.role === 'worker'">
                <router-link class="nav-link" to="/dashboard/profile"><i class="bi bi-person-circle me-1"></i>Mi Perfil</router-link>
              </li>
              <li class="nav-item" v-else>
                <router-link class="nav-link" to="/profile"><i class="bi bi-person-circle me-1"></i>Mi Perfil</router-link>
              </li>
              <li class="nav-item">
                <button class="btn btn-outline-light btn-sm ms-2" @click="logout"><i class="bi bi-box-arrow-right me-1"></i>Cerrar Sesión</button>
              </li>
            </template>
            <template v-else>
              <li class="nav-item me-2">
                <router-link class="btn btn-outline-light btn-sm" to="/login"><i class="bi bi-box-arrow-in-right me-1"></i>Iniciar Sesión</router-link>
              </li>
              <li class="nav-item">
                <router-link class="btn btn-warning btn-sm" to="/register"><i class="bi bi-person-plus me-1"></i>Registrarse</router-link>
              </li>
            </template>
          </ul>
        </div>
      </div>
    </nav>

    <main class="app-shell-main py-4">
      <router-view />
    </main>

    <div v-if="cartStore.toastMessage" class="toast-container-floating">
      <div class="toast-minimalist">
        <i class="bi bi-check-circle-fill text-success fs-5"></i>
        <span>{{ cartStore.toastMessage }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from './stores/auth'
import { useCartStore } from './stores/cart'

const authStore = useAuthStore()
const cartStore = useCartStore()

const logout = () => {
  if (confirm('¿Desea cerrar la sesión actual?')) {
    authStore.logout()
    window.location.href = '/login'
  }
}
</script>

<style scoped>
.bg-terracota {
  background-color: var(--color-terracota, #c85a32) !important;
  color: #fff;
}
</style>