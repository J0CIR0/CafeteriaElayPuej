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
    component: () => import('../views/CheckoutView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/orders',
    name: 'Orders',
    component: () => import('../views/OrdersView.vue'),
    meta: { requiresAuth: true }
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
    path: '/admin/insumos',
    name: 'AdminIngredients',
    component: () => import('../views/admin/AdminIngredientsView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/finanzas',
    name: 'AdminFinancial',
    component: () => import('../views/admin/AdminFinancialView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/admin/reports',
    name: 'AdminReports',
    component: () => import('../views/admin/AdminReportsView.vue'),
    meta: { requiresAdmin: true }
  },
  {
    path: '/dashboard',
    name: 'WorkerDashboard',
    component: () => import('../views/worker/WorkerDashboardView.vue'),
    meta: { requiresWorker: true }
  },
  {
    path: '/dashboard/orders',
    name: 'WorkerOrders',
    component: () => import('../views/worker/WorkerOrdersView.vue'),
    meta: { requiresWorker: true }
  },
  {
    path: '/dashboard/clients',
    name: 'WorkerClients',
    component: () => import('../views/worker/WorkerClientsView.vue'),
    meta: { requiresWorker: true }
  },
  {
    path: '/dashboard/products',
    name: 'WorkerProducts',
    component: () => import('../views/worker/WorkerProductsView.vue'),
    meta: { requiresWorker: true }
  },
  {
    path: '/dashboard/inventory',
    name: 'WorkerInventory',
    component: () => import('../views/worker/WorkerInventoryView.vue'),
    meta: { requiresWorker: true }
  },
  {
    path: '/dashboard/reports',
    name: 'WorkerReports',
    component: () => import('../views/worker/WorkerReportsView.vue'),
    meta: { requiresWorker: true }
  },
  {
    path: '/dashboard/profile',
    name: 'WorkerProfile',
    component: () => import('../views/worker/WorkerProfileView.vue'),
    meta: { requiresWorker: true }
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
  } else if (to.meta.requiresWorker && !authStore.isWorker) {
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