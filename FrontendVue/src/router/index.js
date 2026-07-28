import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/HomeView.vue')
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/LoginView.vue')
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/RegisterView.vue')
  },
  {
    path: '/verify-email',
    name: 'VerifyEmail',
    component: () => import('../views/VerifyEmailView.vue')
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import('../views/ForgotPasswordView.vue')
  },
  {
    path: '/products',
    name: 'Products',
    component: () => import('../views/ProductsView.vue')
  },
  {
    path: '/cart',
    name: 'Cart',
    component: () => import('../views/CartView.vue')
  },
  {
    path: '/checkout',
    name: 'Checkout',
    component: () => import('../views/CheckoutView.vue')
  },
  {
    path: '/orders',
    name: 'Orders',
    component: () => import('../views/OrdersView.vue')
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('../views/ProfileView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: () => import('../views/admin/AdminDashboardView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/products',
    name: 'AdminProducts',
    component: () => import('../views/admin/AdminProductsView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/categories',
    name: 'AdminCategories',
    component: () => import('../views/admin/AdminCategoriesView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/orders',
    name: 'AdminOrders',
    component: () => import('../views/admin/AdminOrdersView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/inventory',
    name: 'AdminInventory',
    component: () => import('../views/admin/AdminInventoryView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/users',
    name: 'AdminUsers',
    component: () => import('../views/admin/AdminUsersView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/reports',
    name: 'AdminReports',
    component: () => import('../views/admin/AdminReportsView.vue'),
    meta: { requiresAdmin: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next('/')
  } else if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.path !== '/verify-email' && 
             to.path !== '/login' && 
             to.path !== '/register' && 
             to.path !== '/forgot-password' &&
             authStore.isAuthenticated && 
             !authStore.isEmailVerified) {
    next('/verify-email')
  } else {
    next()
  }
})

export default router