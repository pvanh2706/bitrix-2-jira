<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import dayjs from 'dayjs'
import AppLayout from '@/components/AppLayout.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import { healthApi } from '@/api/health'
import { dealsApi } from '@/api/deals'
import type { HealthStatus, DealSummaryDto } from '@/api/types'

const health = ref<HealthStatus | null>(null)
const healthError = ref(false)
const recentDeals = ref<DealSummaryDto[]>([])
const loadingDeals = ref(true)

async function fetchHealth() {
  try {
    const res = await healthApi.get()
    health.value = res.data.data
    healthError.value = false
  } catch {
    healthError.value = true
  }
}

async function fetchRecentDeals() {
  loadingDeals.value = true
  try {
    const from = dayjs().subtract(7, 'day').format('YYYY-MM-DD')
    const to = dayjs().format('YYYY-MM-DD')
    const res = await dealsApi.search({ fromDate: from, toDate: to })
    recentDeals.value = (res.data.data ?? []).slice(0, 10)
  } finally {
    loadingDeals.value = false
  }
}

const stats = computed(() => {
  const total = recentDeals.value.length
  const success = recentDeals.value.filter(
    (d) => d.isSendDataToJira === 1 && d.haveError === 0,
  ).length
  const errors = recentDeals.value.filter((d) => d.haveError === 1).length
  const pending = total - success - errors
  return { total, success, errors, pending }
})

let pollTimer: ReturnType<typeof setInterval>
onMounted(() => {
  fetchHealth()
  fetchRecentDeals()
  pollTimer = setInterval(fetchHealth, 30_000)
})
onUnmounted(() => clearInterval(pollTimer))
</script>

<template>
  <AppLayout>
    <template #title>Dashboard</template>

    <!-- Health banner -->
    <div
      :class="[
        'mb-6 flex items-center gap-3 rounded-lg px-4 py-3 text-sm font-medium border',
        healthError
          ? 'bg-red-50 border-red-300 text-red-700'
          : health?.applicationStopping
            ? 'bg-yellow-50 border-yellow-300 text-yellow-700'
            : health
              ? 'bg-green-50 border-green-300 text-green-700'
              : 'bg-gray-50 border-gray-200 text-gray-500',
      ]"
    >
      <span class="text-lg">
        {{ healthError ? '🔴' : health?.applicationStopping ? '🟡' : health ? '🟢' : '⚫' }}
      </span>
      <span v-if="!health && !healthError">Đang kết nối tới API...</span>
      <span v-else-if="healthError">Không thể kết nối tới API server</span>
      <span v-else-if="health?.applicationStopping">Dịch vụ đang dừng lại</span>
      <span v-else>Dịch vụ đang hoạt động bình thường</span>
      <span v-if="health" class="ml-auto text-xs opacity-70">
        Khởi động: {{ dayjs(health.startedAt).format('DD/MM/YYYY HH:mm') }}
      </span>
    </div>

    <!-- Stats cards -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-lg border border-gray-200 px-5 py-4">
        <p class="text-xs text-gray-500 mb-1">Tổng Deal (7 ngày)</p>
        <p class="text-2xl font-bold text-gray-900">{{ stats.total }}</p>
      </div>
      <div class="bg-white rounded-lg border border-gray-200 px-5 py-4">
        <p class="text-xs text-gray-500 mb-1">Tạo Issue thành công</p>
        <p class="text-2xl font-bold text-green-600">{{ stats.success }}</p>
      </div>
      <div class="bg-white rounded-lg border border-gray-200 px-5 py-4">
        <p class="text-xs text-gray-500 mb-1">Có lỗi</p>
        <p class="text-2xl font-bold text-red-600">{{ stats.errors }}</p>
      </div>
      <div class="bg-white rounded-lg border border-gray-200 px-5 py-4">
        <p class="text-xs text-gray-500 mb-1">Đang chờ / Bỏ qua</p>
        <p class="text-2xl font-bold text-yellow-600">{{ stats.pending }}</p>
      </div>
    </div>

    <!-- Recent deals table -->
    <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
      <div class="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
        <h2 class="text-sm font-semibold text-gray-700">Deal gần đây (7 ngày)</h2>
        <RouterLink to="/deals" class="text-xs text-indigo-600 hover:underline">Xem tất cả →</RouterLink>
      </div>
      <div v-if="loadingDeals" class="flex justify-center py-10">
        <LoadingSpinner />
      </div>
      <div v-else-if="!recentDeals.length" class="py-10 text-center text-sm text-gray-400">
        Không có deal nào trong 7 ngày qua.
      </div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 text-xs text-gray-500 uppercase">
          <tr>
            <th class="px-5 py-2.5 text-left">Deal ID</th>
            <th class="px-5 py-2.5 text-left">Ngày</th>
            <th class="px-5 py-2.5 text-left">Jira Issue</th>
            <th class="px-5 py-2.5 text-left">Trạng thái</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="deal in recentDeals" :key="deal.bitrix_DealID" class="hover:bg-gray-50">
            <td class="px-5 py-3 font-mono">
              <a :href="deal.bitrix_DealLink" target="_blank" class="text-indigo-600 hover:underline">
                #{{ deal.bitrix_DealID }}
              </a>
            </td>
            <td class="px-5 py-3 text-gray-500">{{ deal.bitrix_DateSearch }}</td>
            <td class="px-5 py-3">
              <a
                v-if="deal.jira_Link"
                :href="deal.jira_Link"
                target="_blank"
                class="text-indigo-600 hover:underline text-xs"
              >
                {{ deal.jira_Link.split('/').pop() }}
              </a>
              <span v-else class="text-gray-400 text-xs">—</span>
            </td>
            <td class="px-5 py-3">
              <StatusBadge :deal="deal" />
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </AppLayout>
</template>
