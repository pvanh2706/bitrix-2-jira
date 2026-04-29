<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import AppLayout from '@/components/AppLayout.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import { configApi } from '@/api/config'
import { useToast } from '@/composables/useToast'
import type { ConfigData } from '@/api/types'

const toast = useToast()
const allConfigs = ref<ConfigData[]>([])
const loading = ref(false)
const saving = ref(false)

const form = reactive({
  quetLaiSau: null as number | null,
  guiLaiEmailSau: null as number | null,
  soNgayQuet: null as number | null,
})

const configDescriptions: Record<string, string> = {
  QuetLaiSau: 'Quét lại sau (phút)',
  GuiLaiEmailSau: 'Gửi lại email sau (giờ)',
  SoNgayQuet: 'Số ngày quét ngược',
}

async function loadConfig() {
  loading.value = true
  try {
    const res = await configApi.get()
    allConfigs.value = res.data.data ?? []
    // fill form
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

onMounted(loadConfig)
</script>

<template>
  <AppLayout>
    <template #title>Cấu hình hệ thống</template>

    <div class="max-w-xl space-y-6">
      <!-- Scan config card -->
      <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div class="px-5 py-3 border-b border-gray-100 bg-gray-50">
          <h2 class="text-sm font-semibold text-gray-700">Cài đặt quét & gửi mail</h2>
        </div>

        <div v-if="loading" class="flex justify-center py-8">
          <LoadingSpinner class="text-indigo-500" />
        </div>

        <form v-else @submit.prevent="saveConfig" class="px-5 py-5 space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              {{ configDescriptions['QuetLaiSau'] }}
            </label>
            <div class="flex items-center gap-2">
              <input
                v-model.number="form.quetLaiSau"
                type="number"
                min="1"
                max="60"
                class="w-28 rounded-md border-gray-300 text-sm shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
              />
              <span class="text-sm text-gray-500">phút</span>
            </div>
            <p class="mt-1 text-xs text-gray-400">Khoảng thời gian background service quét Bitrix (1–60 phút)</p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              {{ configDescriptions['SoNgayQuet'] }}
            </label>
            <div class="flex items-center gap-2">
              <input
                v-model.number="form.soNgayQuet"
                type="number"
                min="1"
                max="30"
                class="w-28 rounded-md border-gray-300 text-sm shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
              />
              <span class="text-sm text-gray-500">ngày</span>
            </div>
            <p class="mt-1 text-xs text-gray-400">Quét ngược bao nhiêu ngày từ hiện tại</p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              {{ configDescriptions['GuiLaiEmailSau'] }}
            </label>
            <div class="flex items-center gap-2">
              <input
                v-model.number="form.guiLaiEmailSau"
                type="number"
                min="1"
                class="w-28 rounded-md border-gray-300 text-sm shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
              />
              <span class="text-sm text-gray-500">giờ</span>
            </div>
            <p class="mt-1 text-xs text-gray-400">Gửi lại email lỗi sau bao nhiêu giờ</p>
          </div>

          <div class="pt-2">
            <button
              type="submit"
              :disabled="saving"
              class="flex items-center gap-2 bg-indigo-600 text-white text-sm font-medium px-5 py-2 rounded-md hover:bg-indigo-700 disabled:opacity-50"
            >
              <LoadingSpinner v-if="saving" size="sm" class="text-white" />
              Lưu cấu hình
            </button>
          </div>
        </form>
      </div>

      <!-- All config rows (read-only table) -->
      <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div class="px-5 py-3 border-b border-gray-100 bg-gray-50">
          <h2 class="text-sm font-semibold text-gray-700">Tất cả config trong DB</h2>
        </div>
        <div v-if="loading" class="flex justify-center py-6">
          <LoadingSpinner class="text-indigo-500" />
        </div>
        <table v-else class="w-full text-sm">
          <thead class="bg-gray-50 text-xs text-gray-500 uppercase">
            <tr>
              <th class="px-5 py-2.5 text-left">Key</th>
              <th class="px-5 py-2.5 text-left">Value</th>
              <th class="px-5 py-2.5 text-left">Mô tả</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr v-for="c in allConfigs" :key="c.id" class="hover:bg-gray-50">
              <td class="px-5 py-2.5 font-mono text-xs text-gray-600">{{ c.keyConfig }}</td>
              <td class="px-5 py-2.5 font-semibold text-gray-900">{{ c.valueConfig }}</td>
              <td class="px-5 py-2.5 text-gray-500">{{ c.description || '—' }}</td>
            </tr>
            <tr v-if="!allConfigs.length">
              <td colspan="3" class="px-5 py-6 text-center text-gray-400">Không có dữ liệu</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </AppLayout>
</template>
