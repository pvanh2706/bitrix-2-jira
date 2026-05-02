<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import AppLayout from '@/components/AppLayout.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import { configApi } from '@/api/config'
import { useToast } from '@/composables/useToast'
import type { ConfigData, SystemConfig } from '@/api/types'

const toast = useToast()
const allConfigs = ref<ConfigData[]>([])
const loading = ref(false)
const saving = ref(false)

const form = reactive({
  quetLaiSau: null as number | null,
  guiLaiEmailSau: null as number | null,
  soNgayQuet: null as number | null,
})

// ── SystemConfigs ──────────────────────────────────────────
const systemConfigs = ref<SystemConfig[]>([])
const systemLoading = ref(false)
const editingKey = ref<string | null>(null)
const editingValue = ref('')
const savingKey = ref<string | null>(null)

async function loadSystemConfigs() {
  systemLoading.value = true
  try {
    const res = await configApi.getSystemConfigs()
    systemConfigs.value = res.data.data ?? []
  } catch (err: any) {
    toast.error(err.message)
  } finally {
    systemLoading.value = false
  }
}

function startEdit(cfg: SystemConfig) {
  editingKey.value = cfg.configKey
  editingValue.value = cfg.configValue
}

function cancelEdit() {
  editingKey.value = null
  editingValue.value = ''
}

async function saveEdit(cfg: SystemConfig) {
  savingKey.value = cfg.configKey
  try {
    await configApi.updateSystemConfig(cfg.configKey, editingValue.value)
    cfg.configValue = editingValue.value
    editingKey.value = null
    toast.success(`Đã lưu "${cfg.configKey}"`)
  } catch (err: any) {
    toast.error(err.message)
  } finally {
    savingKey.value = null
  }
}
// ───────────────────────────────────────────────────────────

async function loadConfig() {
  loading.value = true
  try {
    const res = await configApi.get()
    allConfigs.value = res.data.data ?? []
    for (const c of allConfigs.value) {
      if (c.keyConfig === 'QuetLaiSau') form.quetLaiSau = Number(c.valueConfig) || null
      if (c.keyConfig === 'GuiLaiEmailSau') form.guiLaiEmailSau = Number(c.valueConfig) || null
      if (c.keyConfig === 'SoNgayQuet') form.soNgayQuet = Number(c.valueConfig) || null
    }
  } catch (err: any) {
    toast.error(err.message)
  } finally {
    loading.value = false
  }
}

async function saveConfig() {
  saving.value = true
  try {
    await configApi.save({
      quetLaiSau: form.quetLaiSau,
      guiLaiEmailSau: form.guiLaiEmailSau,
      soNgayQuet: form.soNgayQuet,
    })
    toast.success('Đã lưu cấu hình thành công')
    await loadConfig()
  } catch (err: any) {
    toast.error(err.message)
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadConfig()
  loadSystemConfigs()
})
</script>

<template>
  <AppLayout>
    <template #title>Cấu hình hệ thống</template>

    <div class="space-y-6">

      <!-- ── Row 1: Scan settings (3 cards ngang) ── -->
      <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div class="px-6 py-3 border-b border-gray-100 bg-gray-50">
          <h2 class="text-sm font-semibold text-gray-700">Cài đặt quét &amp; gửi mail</h2>
        </div>

        <div v-if="loading" class="flex justify-center py-10">
          <LoadingSpinner class="text-indigo-500" />
        </div>

        <form v-else @submit.prevent="saveConfig" class="px-6 py-5">
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">

            <div>
              <label class="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Quét lại sau
              </label>
              <div class="flex items-center gap-2">
                <input
                  v-model.number="form.quetLaiSau"
                  type="number" min="1" max="60"
                  class="w-24 rounded-md border border-gray-300 text-sm px-3 py-1.5 shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                />
                <span class="text-sm text-gray-500">phút</span>
              </div>
              <p class="mt-1 text-xs text-gray-400">Background service quét Bitrix (1–60 phút)</p>
            </div>

            <div>
              <label class="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Số ngày quét ngược
              </label>
              <div class="flex items-center gap-2">
                <input
                  v-model.number="form.soNgayQuet"
                  type="number" min="1" max="30"
                  class="w-24 rounded-md border border-gray-300 text-sm px-3 py-1.5 shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                />
                <span class="text-sm text-gray-500">ngày</span>
              </div>
              <p class="mt-1 text-xs text-gray-400">Quét ngược bao nhiêu ngày từ hiện tại</p>
            </div>

            <div>
              <label class="block text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Gửi lại email sau
              </label>
              <div class="flex items-center gap-2">
                <input
                  v-model.number="form.guiLaiEmailSau"
                  type="number" min="1"
                  class="w-24 rounded-md border border-gray-300 text-sm px-3 py-1.5 shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
                />
                <span class="text-sm text-gray-500">giờ</span>
              </div>
              <p class="mt-1 text-xs text-gray-400">Gửi lại email lỗi sau bao nhiêu giờ</p>
            </div>

          </div>

          <div class="mt-5 pt-4 border-t border-gray-100 flex justify-end">
            <button
              type="submit"
              :disabled="saving"
              class="flex items-center gap-2 bg-indigo-600 text-white text-sm font-medium px-6 py-2 rounded-md hover:bg-indigo-700 disabled:opacity-50"
            >
              <LoadingSpinner v-if="saving" size="sm" class="text-white" />
              Lưu cấu hình
            </button>
          </div>
        </form>
      </div>

      <!-- ── Row 2: System Config table full width ── -->
      <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div class="px-6 py-3 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-gray-700">System Config (DB)</h2>
          <span class="text-xs text-gray-400">Click vào giá trị để chỉnh sửa</span>
        </div>

        <div v-if="systemLoading" class="flex justify-center py-10">
          <LoadingSpinner class="text-indigo-500" />
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead class="bg-gray-50 text-xs text-gray-500 uppercase tracking-wide">
              <tr>
                <th class="px-6 py-3 text-left w-64">Key</th>
                <th class="px-6 py-3 text-left w-80">Giá trị</th>
                <th class="px-6 py-3 text-left">Mô tả</th>
                <th class="px-6 py-3 w-28 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              <tr
                v-for="cfg in systemConfigs"
                :key="cfg.configKey"
                class="hover:bg-gray-50 transition-colors"
              >
                <td class="px-6 py-3 font-mono text-xs text-indigo-700 align-middle whitespace-nowrap">
                  {{ cfg.configKey }}
                </td>

                <td class="px-6 py-3 align-middle">
                  <template v-if="editingKey === cfg.configKey">
                    <input
                      v-model="editingValue"
                      class="w-full rounded border border-indigo-400 text-sm px-2 py-1.5 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                      @keydown.enter="saveEdit(cfg)"
                      @keydown.escape="cancelEdit"
                      autofocus
                    />
                  </template>
                  <template v-else>
                    <span
                      class="text-gray-900 cursor-pointer hover:text-indigo-600 break-all"
                      @click="startEdit(cfg)"
                      :title="cfg.configValue"
                    >{{ cfg.configValue || '—' }}</span>
                  </template>
                </td>

                <td class="px-6 py-3 text-gray-500 text-xs align-middle">
                  {{ cfg.description || '—' }}
                </td>

                <td class="px-6 py-3 align-middle text-right whitespace-nowrap">
                  <template v-if="editingKey === cfg.configKey">
                    <div class="flex items-center justify-end gap-1.5">
                      <button
                        @click="saveEdit(cfg)"
                        :disabled="savingKey === cfg.configKey"
                        class="flex items-center gap-1 text-xs bg-indigo-600 text-white px-3 py-1.5 rounded hover:bg-indigo-700 disabled:opacity-50"
                      >
                        <LoadingSpinner v-if="savingKey === cfg.configKey" size="sm" class="text-white" />
                        Lưu
                      </button>
                      <button
                        @click="cancelEdit"
                        class="text-xs text-gray-600 border border-gray-300 px-3 py-1.5 rounded hover:bg-gray-100"
                      >
                        Hủy
                      </button>
                    </div>
                  </template>
                  <template v-else>
                    <button
                      @click="startEdit(cfg)"
                      class="text-xs text-indigo-600 border border-indigo-200 px-3 py-1.5 rounded hover:bg-indigo-50"
                    >
                      Sửa
                    </button>
                  </template>
                </td>
              </tr>

              <tr v-if="!systemConfigs.length">
                <td colspan="4" class="px-6 py-10 text-center text-gray-400">Không có dữ liệu</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </AppLayout>
</template>
