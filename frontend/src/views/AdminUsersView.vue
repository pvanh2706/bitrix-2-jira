<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AppLayout from '@/components/AppLayout.vue'
import { authApi } from '@/api/auth'
import { useToast } from '@/composables/useToast'
import type { AdminUser } from '@/api/types'

const { success, error } = useToast()

const users = ref<AdminUser[]>([])
const loading = ref(false)

// --- Create form ---
const newUsername = ref('')
const newPassword = ref('')
const creating = ref(false)

// --- Edit modal ---
const editTarget = ref<AdminUser | null>(null)
const editUsername = ref('')
const editPassword = ref('')
const saving = ref(false)

// --- Delete confirm ---
const deleteTarget = ref<AdminUser | null>(null)
const deleting = ref(false)

async function fetchUsers() {
  loading.value = true
  try {
    const res = await authApi.listUsers()
    users.value = res.data
  } catch {
    error('Không thể tải danh sách tài khoản.')
  } finally {
    loading.value = false
  }
}

onMounted(fetchUsers)

async function handleCreate() {
  if (!newUsername.value.trim() || !newPassword.value) return
  creating.value = true
  try {
    await authApi.createUser(newUsername.value.trim(), newPassword.value)
    success(`Đã tạo tài khoản "${newUsername.value.trim()}".`)
    newUsername.value = ''
    newPassword.value = ''
    await fetchUsers()
  } catch (e: unknown) {
    error(e instanceof Error ? e.message : 'Tạo tài khoản thất bại.')
  } finally {
    creating.value = false
  }
}

function openEdit(user: AdminUser) {
  editTarget.value = user
  editUsername.value = user.username
  editPassword.value = ''
}

function closeEdit() {
  editTarget.value = null
}

async function handleSave() {
  if (!editTarget.value) return
  const payload: { newUsername?: string; newPassword?: string } = {}
  if (editUsername.value.trim() && editUsername.value.trim() !== editTarget.value.username)
    payload.newUsername = editUsername.value.trim()
  if (editPassword.value)
    payload.newPassword = editPassword.value
  if (!payload.newUsername && !payload.newPassword) {
    closeEdit()
    return
  }
  saving.value = true
  try {
    await authApi.updateUser(editTarget.value.id, payload)
    success('Đã cập nhật tài khoản.')
    closeEdit()
    await fetchUsers()
  } catch (e: unknown) {
    error(e instanceof Error ? e.message : 'Cập nhật thất bại.')
  } finally {
    saving.value = false
  }
}

function confirmDelete(user: AdminUser) {
  deleteTarget.value = user
}

function cancelDelete() {
  deleteTarget.value = null
}

async function handleDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await authApi.deleteUser(deleteTarget.value.id)
    success(`Đã xóa tài khoản "${deleteTarget.value.username}".`)
    deleteTarget.value = null
    await fetchUsers()
  } catch (e: unknown) {
    error(e instanceof Error ? e.message : 'Xóa thất bại.')
  } finally {
    deleting.value = false
  }
}
</script>

<template>
  <AppLayout>
    <template #title>Quản lý tài khoản Admin</template>

    <div class="max-w-2xl space-y-8">
      <!-- Create new account -->
      <section class="bg-white rounded-xl border border-gray-200 p-6">
        <h2 class="text-sm font-semibold text-gray-700 mb-4">Thêm tài khoản mới</h2>
        <form class="flex gap-3 items-end" @submit.prevent="handleCreate">
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-600 mb-1">Username</label>
            <input
              v-model="newUsername"
              type="text"
              required
              placeholder="vd: manager01"
              class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-400 transition"
            />
          </div>
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-600 mb-1">Password</label>
            <input
              v-model="newPassword"
              type="password"
              required
              placeholder="••••••••"
              class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-400 transition"
            />
          </div>
          <button
            type="submit"
            :disabled="creating"
            class="rounded-md bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50 transition whitespace-nowrap"
          >
            {{ creating ? 'Đang tạo…' : 'Tạo tài khoản' }}
          </button>
        </form>
      </section>

      <!-- User list -->
      <section class="bg-white rounded-xl border border-gray-200">
        <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-gray-700">Danh sách tài khoản</h2>
          <span class="text-xs text-gray-400">{{ users.length }} tài khoản</span>
        </div>

        <div v-if="loading" class="px-6 py-8 text-center text-sm text-gray-400">Đang tải…</div>

        <ul v-else class="divide-y divide-gray-100">
          <li
            v-for="user in users"
            :key="user.id"
            class="flex items-center justify-between px-6 py-3"
          >
            <div class="flex items-center gap-3">
              <span class="w-8 h-8 rounded-full bg-indigo-100 text-indigo-600 flex items-center justify-center text-xs font-bold uppercase">
                {{ user.username.charAt(0) }}
              </span>
              <span class="text-sm font-medium text-gray-800">{{ user.username }}</span>
            </div>
            <div class="flex gap-2">
              <button
                class="text-xs px-3 py-1 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 transition"
                @click="openEdit(user)"
              >
                Sửa
              </button>
              <button
                class="text-xs px-3 py-1 rounded border border-red-200 text-red-500 hover:bg-red-50 transition"
                @click="confirmDelete(user)"
              >
                Xóa
              </button>
            </div>
          </li>
        </ul>
      </section>
    </div>

    <!-- Edit modal -->
    <Teleport to="body">
      <div
        v-if="editTarget"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
        @click.self="closeEdit"
      >
        <div class="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
          <h3 class="text-sm font-semibold text-gray-700 mb-4">
            Sửa tài khoản: <span class="text-indigo-600">{{ editTarget.username }}</span>
          </h3>
          <form class="space-y-4" @submit.prevent="handleSave">
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">Username mới</label>
              <input
                v-model="editUsername"
                type="text"
                class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-400 transition"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-600 mb-1">
                Password mới <span class="text-gray-400">(để trống = giữ nguyên)</span>
              </label>
              <input
                v-model="editPassword"
                type="password"
                placeholder="••••••••"
                class="w-full rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-400 transition"
              />
            </div>
            <div class="flex justify-end gap-2 pt-2">
              <button
                type="button"
                class="text-sm px-4 py-2 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 transition"
                @click="closeEdit"
              >
                Hủy
              </button>
              <button
                type="submit"
                :disabled="saving"
                class="text-sm px-4 py-2 rounded bg-indigo-600 text-white font-semibold hover:bg-indigo-700 disabled:opacity-50 transition"
              >
                {{ saving ? 'Đang lưu…' : 'Lưu thay đổi' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

    <!-- Delete confirm modal -->
    <Teleport to="body">
      <div
        v-if="deleteTarget"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
        @click.self="cancelDelete"
      >
        <div class="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
          <h3 class="text-sm font-semibold text-gray-700 mb-2">Xác nhận xóa</h3>
          <p class="text-sm text-gray-600 mb-6">
            Bạn chắc chắn muốn xóa tài khoản
            <strong class="text-gray-900">{{ deleteTarget.username }}</strong>?
          </p>
          <div class="flex justify-end gap-2">
            <button
              class="text-sm px-4 py-2 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 transition"
              @click="cancelDelete"
            >
              Hủy
            </button>
            <button
              :disabled="deleting"
              class="text-sm px-4 py-2 rounded bg-red-500 text-white font-semibold hover:bg-red-600 disabled:opacity-50 transition"
              @click="handleDelete"
            >
              {{ deleting ? 'Đang xóa…' : 'Xóa' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </AppLayout>
</template>
