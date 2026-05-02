<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuth } from '@/composables/useAuth'

const route = useRoute()
const router = useRouter()
const { isAdmin, logout } = useAuth()

const allNavItems = [
  { to: '/', label: 'Dashboard', icon: '⬛', adminOnly: true },
  { to: '/deals', label: 'Danh sách Deal', icon: '📋', adminOnly: true },
  { to: '/create-issue', label: 'Tạo Issue', icon: '➕', adminOnly: false },
  { to: '/config', label: 'Cấu hình', icon: '⚙️', adminOnly: true },
  { to: '/admin-users', label: 'Tài khoản Admin', icon: '👤', adminOnly: true },
]

const navItems = computed(() =>
  allNavItems.filter((item) => !item.adminOnly || isAdmin.value),
)

function handleLogout() {
  logout()
  router.push({ name: 'create-issue' })
}
</script>

<template>
  <div class="flex min-h-screen bg-gray-50">
    <!-- Sidebar -->
    <aside class="w-56 shrink-0 bg-white border-r border-gray-200 flex flex-col">
      <div class="h-14 flex items-center px-4 border-b border-gray-200">
        <span class="text-sm font-bold text-indigo-600 leading-tight">Bitrix → Jira<br><span class="font-normal text-gray-500 text-xs">Connector</span></span>
      </div>
      <nav class="flex-1 px-2 py-3 space-y-0.5">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          :class="[
            'flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors',
            route.path === item.to
              ? 'bg-indigo-50 text-indigo-700'
              : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900',
          ]"
        >
          <span class="text-base leading-none">{{ item.icon }}</span>
          {{ item.label }}
        </RouterLink>
      </nav>
      <div class="px-4 py-3 border-t border-gray-200 space-y-2">
        <!-- Admin: show logout button -->
        <template v-if="isAdmin">
          <p class="text-xs text-gray-500 truncate">👤 Admin</p>
          <button
            class="w-full text-left text-xs text-red-500 hover:text-red-700 transition"
            @click="handleLogout"
          >
            Đăng xuất
          </button>
        </template>
        <!-- Guest: show login link -->
        <template v-else>
          <RouterLink
            to="/login"
            class="block text-xs text-indigo-500 hover:text-indigo-700 transition"
          >
            🔑 Đăng nhập admin
          </RouterLink>
        </template>
        <p class="text-xs text-gray-400">EzCloud · v1.0</p>
      </div>
    </aside>

    <!-- Main content -->
    <main class="flex-1 flex flex-col overflow-hidden">
      <header class="h-14 bg-white border-b border-gray-200 flex items-center px-6">
        <h1 class="text-sm font-semibold text-gray-700">
          <slot name="title">BitrixJira Connector</slot>
        </h1>
      </header>
      <div class="flex-1 overflow-auto p-6">
        <slot />
      </div>
    </main>
  </div>
</template>

