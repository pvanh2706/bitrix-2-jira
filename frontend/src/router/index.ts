import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '@/views/DashboardView.vue'
import { useAuth } from '@/composables/useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/deals',
      name: 'deals',
      component: () => import('@/views/DealsView.vue'),
      meta: { requiresAdmin: true },
    },
    {
      path: '/config',
      name: 'config',
      component: () => import('@/views/ConfigView.vue'),
      meta: { requiresAdmin: true },
    },
    {
      path: '/admin-users',
      name: 'admin-users',
      component: () => import('@/views/AdminUsersView.vue'),
      meta: { requiresAdmin: true },
    },
    {
      path: '/create-issue',
      name: 'create-issue',
      component: () => import('@/views/CreateIssueView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const { isAdmin } = useAuth()

  // Redirect logged-in admin away from login page
  if (to.name === 'login' && isAdmin.value) {
    return { name: 'dashboard' }
  }

  // Protect admin-only routes
  if (to.meta.requiresAdmin && !isAdmin.value) {
    return { name: 'create-issue' }
  }
})

export default router
