import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '@/views/DashboardView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
    },
    {
      path: '/deals',
      name: 'deals',
      component: () => import('@/views/DealsView.vue'),
    },
    {
      path: '/config',
      name: 'config',
      component: () => import('@/views/ConfigView.vue'),
    },
    {
      path: '/create-issue',
      name: 'create-issue',
      component: () => import('@/views/CreateIssueView.vue'),
    },
  ],
})

export default router
