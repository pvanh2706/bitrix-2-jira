import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'

const STORAGE_KEY = 'bjc_role'

// Reactive state — persisted in sessionStorage (cleared when tab closes)
const role = ref<string | null>(sessionStorage.getItem(STORAGE_KEY))

export function useAuth() {
  const isAdmin = computed(() => role.value === 'admin')

  async function login(username: string, password: string): Promise<void> {
    const res = await authApi.login(username, password)
    role.value = res.data.role
    sessionStorage.setItem(STORAGE_KEY, res.data.role)
  }

  function logout() {
    role.value = null
    sessionStorage.removeItem(STORAGE_KEY)
  }

  return { isAdmin, login, logout }
}
