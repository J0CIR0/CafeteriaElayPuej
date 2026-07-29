<template>
    <div class="admin-sidebar">
        <div class="list-group">

            <!-- MENÚ DEL ADMINISTRADOR: Solo se muestra si es Admin -->
            <template v-if="authStore.isAdmin">
                <router-link to="/admin" class="list-group-item list-group-item-action" active-class="active" exact>Dashboard</router-link>
                <router-link to="/admin/products" class="list-group-item list-group-item-action" active-class="active">Productos</router-link>
                <router-link to="/admin/categories" class="list-group-item list-group-item-action" active-class="active">Categorías</router-link>
                <router-link to="/admin/orders" class="list-group-item list-group-item-action" active-class="active">Pedidos</router-link>
                <router-link to="/admin/inventory" class="list-group-item list-group-item-action" active-class="active">Inventario</router-link>
                <router-link to="/admin/insumos" class="list-group-item list-group-item-action" active-class="active">Insumos y Recetas</router-link>
                <router-link to="/admin/finanzas" class="list-group-item list-group-item-action" active-class="active">Finanzas y Pérdidas</router-link>
                <router-link to="/admin/users" class="list-group-item list-group-item-action" active-class="active">Usuarios</router-link>
                <router-link to="/admin/reports" class="list-group-item list-group-item-action" active-class="active">Reportes</router-link>
            </template>

            <!-- MENÚ DEL TRABAJADOR: Solo se muestra si es trabajador y NO es admin -->
            <template v-else-if="authStore.isWorker">
                <router-link to="/trabajador/pedidos" class="list-group-item list-group-item-action" active-class="active">
                    Mis Pedidos
                </router-link>
                <router-link to="/trabajador/inventario" class="list-group-item list-group-item-action" active-class="active">
                    Inventario
                </router-link>
            </template>

        </div>
    </div>
</template>

<script>
    import { useAuthStore } from '@/stores/auth';

    export default {
        computed: {
            authStore() {
                return useAuthStore();
            }
        }
    }
</script>