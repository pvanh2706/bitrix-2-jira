<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import dayjs from 'dayjs'
import AppLayout from '@/components/AppLayout.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import { dealsApi } from '@/api/deals'
import { useToast } from '@/composables/useToast'
import type { DealSummaryDto } from '@/api/types'

const toast = useToast()

const deals = ref<DealSummaryDto[]>([])
const loading = ref(false)
const processingIds = ref<Set<number>>(new Set())

// Detail modal
const selectedDeal = ref<DealSummaryDto | null>(null)
const showModal = ref(false)

const filters = reactive({
  dealId: '' as string,
  fromDate: dayjs().subtract(30, 'day').format('YYYY-MM-DD'),
  toDate: dayjs().format('YYYY-MM-DD'),
})

async function search() {
  loading.value = true
  try {
    const params = {
      dealId: filters.dealId ? Number(filters.dealId) : null,
      fromDate: filters.fromDate || null,
      toDate: filters.toDate || null,
    }
    const res = await dealsApi.search(params)
    deals.value = res.data.data ?? []
  } catch (err: any) {
    toast.error(err.message)
  } finally {
    loading.value = false
  }
}

async function processDeal(dealId: number) {
  if (processingIds.value.has(dealId)) return
  processingIds.value.add(dealId)
  try {
    const res = await dealsApi.process(dealId)
    const result = res.data.data
    if (result?.success) {
      toast.success(`Deal #${dealId} → Issue ${result.jiraKey ?? ''} tạo thành công`)
      await search()
    } else {
      toast.error(result?.message ?? 'Xử lý thất bại')
    }
  } catch (err: any) {
    toast.error(err.message)
  } finally {
    processingIds.value.delete(dealId)
  }
}

function openDetail(deal: DealSummaryDto) {
  selectedDeal.value = deal
  showModal.value = true
}

onMounted(search)
</script>

<template>
  <AppLayout>
    <template #title>Danh sách Deal</template>

    <!-- Filters -->
    <div class="bg-white rounded-lg border border-gray-200 px-5 py-4 mb-5 flex flex-wrap gap-3 items-end">
      <div>
        <label class="block text-xs text-gray-500 mb-1">Deal ID</label>
        <input
          v-model="filters.dealId"
          type="number"
          placeholder="Nhập Deal ID..."
          class="w-36 rounded-md border-gray-300 text-sm shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
          @keyup.enter="search"
        />
      </div>
      <div>
        <label class="block text-xs text-gray-500 mb-1">Từ ngày</label>
        <input
          v-model="filters.fromDate"
          type="date"
          class="rounded-md border-gray-300 text-sm shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
        />
      </div>
      <div>
        <label class="block text-xs text-gray-500 mb-1">Đến ngày</label>
        <input
          v-model="filters.toDate"
          type="date"
          class="rounded-md border-gray-300 text-sm shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
        />
      </div>
      <button
        @click="search"
        :disabled="loading"
        class="flex items-center gap-2 bg-indigo-600 text-white text-sm font-medium px-4 py-2 rounded-md hover:bg-indigo-700 disabled:opacity-50"
      >
        <LoadingSpinner v-if="loading" size="sm" class="text-white" />
        <span>Tìm kiếm</span>
      </button>
    </div>

    <!-- Table -->
    <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
      <div class="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
        <p class="text-sm text-gray-500">
          <span class="font-semibold text-gray-900">{{ deals.length }}</span> kết quả
        </p>
      </div>

      <div v-if="loading && !deals.length" class="flex justify-center py-12">
        <LoadingSpinner size="lg" class="text-indigo-500" />
      </div>

      <div v-else-if="!deals.length" class="py-12 text-center text-sm text-gray-400">
        Không có kết quả nào.
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-gray-50 text-xs text-gray-500 uppercase tracking-wide">
            <tr>
              <th class="px-5 py-3 text-left">Deal ID</th>
              <th class="px-5 py-3 text-left">Ngày tạo</th>
              <th class="px-5 py-3 text-left">Jira Issue</th>
              <th class="px-5 py-3 text-left">Trạng thái</th>
              <th class="px-5 py-3 text-left">Cập nhật cuối</th>
              <th class="px-5 py-3 text-left">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr
              v-for="deal in deals"
              :key="deal.bitrix_DealID"
              class="hover:bg-gray-50 cursor-pointer"
              @click="openDetail(deal)"
            >
              <td class="px-5 py-3">
                <a
                  :href="deal.bitrix_DealLink"
                  target="_blank"
                  class="font-mono text-indigo-600 hover:underline"
                  @click.stop
                >
                  #{{ deal.bitrix_DealID }}
                </a>
              </td>
              <td class="px-5 py-3 text-gray-500">{{ deal.dateTimeCreated }}</td>
              <td class="px-5 py-3">
                <a
                  v-if="deal.jira_Link"
                  :href="deal.jira_Link"
                  target="_blank"
                  class="text-indigo-600 hover:underline font-mono text-xs"
                  @click.stop
                >
                  {{ deal.jira_Link.split('/').pop() }}
                </a>
                <span v-else class="text-gray-400">—</span>
              </td>
              <td class="px-5 py-3">
                <StatusBadge :deal="deal" />
              </td>
              <td class="px-5 py-3 text-gray-500 text-xs">{{ deal.lastChangeData }}</td>
              <td class="px-5 py-3" @click.stop>
                <button
                  v-if="deal.isSendDataToJira !== 1"
                  @click="processDeal(deal.bitrix_DealID)"
                  :disabled="processingIds.has(deal.bitrix_DealID)"
                  class="flex items-center gap-1.5 text-xs font-medium text-white bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 rounded px-3 py-1.5"
                >
                  <LoadingSpinner
                    v-if="processingIds.has(deal.bitrix_DealID)"
                    size="sm"
                    class="text-white"
                  />
                  Tạo Issue
                </button>
                <span v-else class="text-xs text-gray-400">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Detail Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div
          v-if="showModal && selectedDeal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div class="absolute inset-0 bg-black/40" @click="showModal = false" />
          <div class="relative bg-white rounded-xl shadow-xl w-full max-w-lg p-6 z-10">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-base font-semibold text-gray-900">
                Chi tiết Deal #{{ selectedDeal.bitrix_DealID }}
              </h3>
              <button @click="showModal = false" class="text-gray-400 hover:text-gray-600 text-xl leading-none">✕</button>
            </div>
            <dl class="space-y-3 text-sm">
              <div class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Trạng thái</dt>
                <dd><StatusBadge :deal="selectedDeal" /></dd>
              </div>
              <div class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Link CRM</dt>
                <dd>
                  <a :href="selectedDeal.bitrix_DealLink" target="_blank" class="text-indigo-600 hover:underline break-all text-xs">
                    {{ selectedDeal.bitrix_DealLink || '—' }}
                  </a>
                </dd>
              </div>
              <div class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Jira Issue</dt>
                <dd>
                  <a v-if="selectedDeal.jira_Link" :href="selectedDeal.jira_Link" target="_blank" class="text-indigo-600 hover:underline">
                    {{ selectedDeal.jira_Link.split('/').pop() }}
                  </a>
                  <span v-else class="text-gray-400">Chưa tạo</span>
                </dd>
              </div>
              <div class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Ngày tìm kiếm</dt>
                <dd class="text-gray-700">{{ selectedDeal.bitrix_DateSearch }}</dd>
              </div>
              <div class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Ngày tạo bản ghi</dt>
                <dd class="text-gray-700">{{ selectedDeal.dateTimeCreated }}</dd>
              </div>
              <div class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Cập nhật cuối</dt>
                <dd class="text-gray-700">{{ selectedDeal.lastChangeData }}</dd>
              </div>
              <div v-if="selectedDeal.errorInfo" class="flex gap-2">
                <dt class="w-40 shrink-0 text-gray-500">Thông tin lỗi</dt>
                <dd class="text-red-600 text-xs bg-red-50 rounded p-2 flex-1" v-html="selectedDeal.errorInfo" />
              </div>
            </dl>
            <div class="mt-5 flex justify-end gap-2">
              <button
                v-if="selectedDeal.isSendDataToJira !== 1"
                @click="processDeal(selectedDeal.bitrix_DealID); showModal = false"
                :disabled="processingIds.has(selectedDeal.bitrix_DealID)"
                class="text-sm font-medium bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 disabled:opacity-50"
              >
                Tạo Issue thủ công
              </button>
              <button
                @click="showModal = false"
                class="text-sm font-medium bg-gray-100 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-200"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </AppLayout>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
