<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '@/composables/useAuth'
import { useToast } from '@/composables/useToast'

const router = useRouter()
const { login } = useAuth()
const { error } = useToast()

const username = ref('')
const password = ref('')
const loading = ref(false)

async function handleSubmit() {
  if (!username.value.trim() || !password.value) return
  loading.value = true
  try {
    await login(username.value.trim(), password.value)
    router.replace({ name: 'dashboard' })
  } catch {
    error('Sai username hoặc password.')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 flex items-center justify-center">
    <div class="w-full max-w-sm bg-white rounded-xl shadow border border-gray-200 p-8">
      <!-- Logo / title -->
      <div class="mb-8 text-center">
        <p class="text-xl font-bold text-indigo-600">Bitrix → Jira</p>
        <p class="text-xs text-gray-500 mt-0.5">Connector — Đăng nhập quản trị</p>
      </div>

      <form class="space-y-4" @submit.prevent="handleSubmit">
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1" for="username">
            Username
          </label>
          <input
            id="username"
            v-model="username"
            type="text"
            autocomplete="username"
            required
            class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-400 transition"
            placeholder="admin"
          />
        </div>

        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1" for="password">
            Password
          </label>
          <input
            id="password"
            v-model="password"
            type="password"
            autocomplete="current-password"
            required
            class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-400 transition"
            placeholder="••••••••"
          />
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full rounded-md bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50 transition"
        >
          {{ loading ? 'Đang đăng nhập…' : 'Đăng nhập' }}
        </button>
      </form>
    </div>
  </div>
</template>
