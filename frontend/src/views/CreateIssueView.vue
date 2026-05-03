<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import AppLayout from '@/components/AppLayout.vue'
import { dealsApi } from '@/api/deals'
import { useToast } from '@/composables/useToast'
import type { ProcessDealResult } from '@/api/types'

const { success, error, info } = useToast()

const input = ref('')
const loading = ref(false)
const result = ref<ProcessDealResult | null>(null)
const isBackgroundProcessing = ref(false)

// Countdown state for waiting-for-files case
const countdown = ref(0)
let countdownTimer: ReturnType<typeof setInterval> | null = null

function stopCountdown() {
  if (countdownTimer !== null) {
    clearInterval(countdownTimer)
    countdownTimer = null
  }
  countdown.value = 0
}

function startCountdown(seconds: number, onExpire: () => void) {
  stopCountdown()
  countdown.value = seconds
  countdownTimer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      stopCountdown()
      onExpire()
    }
  }, 1000)
}

/** Extract dealId from a plain number or a Bitrix deal URL */
const parsedDealId = computed<number | null>(() => {
  const raw = input.value.trim()
  if (!raw) return null

  // Plain numeric ID
  if (/^\d+$/.test(raw)) return Number(raw)

  // URL pattern: .../deal/details/{id}/
  const match = raw.match(/\/deal\/details\/(\d+)\/?/)
  if (match) return Number(match[1])

  return null
})

const inputError = computed(() => {
  if (!input.value.trim()) return null
  if (parsedDealId.value === null) return 'Không nhận dạng được Deal ID từ giá trị đã nhập.'
  return null
})

// Poll is-processing khi có dealId hợp lệ và chưa loading
let pollTimer: ReturnType<typeof setInterval> | null = null

function stopPoll() {
  if (pollTimer !== null) {
    clearInterval(pollTimer)
    pollTimer = null
  }
}

async function checkIsProcessing() {
  if (!parsedDealId.value || loading.value) return
  try {
    const res = await dealsApi.isProcessing(parsedDealId.value)
    isBackgroundProcessing.value = res.data.data ?? false
  } catch {
    isBackgroundProcessing.value = false
  }
}

watch(parsedDealId, (id) => {
  stopPoll()
  isBackgroundProcessing.value = false
  if (id !== null) {
    checkIsProcessing()
    pollTimer = setInterval(checkIsProcessing, 2000)
  }
})

onUnmounted(() => {
  stopPoll()
  stopCountdown()
})

async function handleCreate() {
  if (!parsedDealId.value) return
  stopPoll()
  stopCountdown()
  loading.value = true
  result.value = null
  try {
    const res = await dealsApi.process(parsedDealId.value)
    const processResult = res.data.data

    // Deal có tài liệu đang upload — hiển thị countdown và tự động retry
    if (res.data.success && processResult?.isWaitingForFiles) {
      result.value = processResult
      const retryAfter = processResult.retryAfterSeconds > 0 ? processResult.retryAfterSeconds : 30
      startCountdown(retryAfter, () => {
        info('Đang thử lại...')
        handleCreate()
      })
      return
    }

    // Nếu deal đã được xử lý (bởi background service) — jiraKey sẽ null
    // Fallback: lấy thông tin Jira link từ DB
    if (res.data.success && processResult?.success && !processResult.jiraKey) {
      try {
        const dealRes = await dealsApi.getById(parsedDealId.value)
        const dealInfo = dealRes.data.data
        if (dealInfo?.jira_Link) {
          processResult.jiraUrl = dealInfo.jira_Link
          // Extract key from URL (last path segment)
          const key = dealInfo.jira_Link.split('/').filter(Boolean).pop() ?? null
          processResult.jiraKey = key
        }
      } catch {
        // ignore — hiển thị kết quả gốc
      }
    }

    result.value = processResult
    if (res.data.success && result.value?.success) {
      if (result.value.jiraKey) {
        success(`Tạo issue thành công: ${result.value.jiraKey}`)
      } else {
        info(result.value.message ?? 'Deal đã được xử lý')
      }
    } else {
      error(result.value?.message ?? res.data.message ?? 'Có lỗi xảy ra')
    }
  } catch (err: unknown) {
    const msg =
      (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
      'Không thể kết nối tới API server.'
    error(msg)
    result.value = null
  } finally {
    loading.value = false
    isBackgroundProcessing.value = false
  }
}
</script>

<template>
  <AppLayout>
    <template #title>Tạo Issue Jira</template>

    <div class="max-w-lg mx-auto">
      <div class="bg-white rounded-lg border border-gray-200 shadow-sm p-6 space-y-5">
        <div>
          <h2 class="text-sm font-semibold text-gray-700 mb-1">Deal ID hoặc URL Bitrix</h2>
          <p class="text-xs text-gray-500">
            Nhập số Deal ID (VD: <code class="font-mono bg-gray-100 px-1 rounded">49821</code>)
            hoặc đường dẫn Bitrix (VD:
            <code class="font-mono bg-gray-100 px-1 rounded"
              >https://work.ezcloudhotel.com/crm/deal/details/49821/</code
            >).
          </p>
        </div>

        <div>
          <label for="deal-input" class="block text-xs font-medium text-gray-600 mb-1">
            Deal ID / URL
          </label>
          <input
            id="deal-input"
            v-model="input"
            type="text"
            placeholder="49821 hoặc https://work.ezcloudhotel.com/crm/deal/details/49821/"
            :class="[
              'w-full rounded-md border px-3 py-2 text-sm outline-none transition',
              inputError
                ? 'border-red-400 focus:ring-2 focus:ring-red-300'
                : 'border-gray-300 focus:border-indigo-400 focus:ring-2 focus:ring-indigo-200',
            ]"
            @keydown.enter="handleCreate"
          />
          <p v-if="inputError" class="mt-1 text-xs text-red-500">{{ inputError }}</p>
          <p v-else-if="parsedDealId !== null" class="mt-1 text-xs text-gray-400">
            Deal ID được nhận dạng: <span class="font-semibold text-indigo-600">{{ parsedDealId }}</span>
          </p>
        </div>

        <button
          :disabled="loading || !parsedDealId"
          class="w-full flex items-center justify-center gap-2 rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed"
          @click="handleCreate"
        >
          <span v-if="loading" class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
          <span>{{ loading ? 'Đang chờ background service xử lý xong...' : 'Tạo Issue' }}</span>
        </button>

        <!-- Waiting for files upload banner -->
        <transition name="fade">
          <div
            v-if="result?.isWaitingForFiles && countdown > 0"
            class="flex items-center gap-2 rounded-md border border-blue-300 bg-blue-50 px-4 py-2.5 text-sm text-blue-800"
          >
            <span class="inline-block h-3.5 w-3.5 animate-spin rounded-full border-2 border-blue-500 border-t-transparent shrink-0" />
            <span>
              Deal có tài liệu đang được tải lên Bitrix. Tự động thử lại sau
              <strong>{{ countdown }}s</strong>.
              <button
                class="ml-2 underline font-medium hover:opacity-70"
                @click="() => { stopCountdown(); handleCreate() }"
              >Thử ngay</button>
            </span>
          </div>
        </transition>

        <!-- Background processing banner -->
        <transition name="fade">
          <div
            v-if="isBackgroundProcessing && !loading"
            class="flex items-center gap-2 rounded-md border border-yellow-300 bg-yellow-50 px-4 py-2.5 text-sm text-yellow-800"
          >
            <span class="inline-block h-3.5 w-3.5 animate-spin rounded-full border-2 border-yellow-500 border-t-transparent shrink-0" />
            <span>Background service đang xử lý deal này. Nhấn <strong>Tạo Issue</strong> sẽ chờ background xong rồi trả kết quả.</span>
          </div>
        </transition>

        <!-- Result panel -->
        <transition name="fade">
          <div v-if="result && !result.isWaitingForFiles" :class="[
            'rounded-md border px-4 py-3 text-sm space-y-2',
            result.success
              ? 'bg-green-50 border-green-300 text-green-800'
              : 'bg-red-50 border-red-300 text-red-800',
          ]">
            <p class="font-semibold">{{ result.success ? '✅ Thành công' : '❌ Thất bại' }}</p>
            <p>{{ result.message }}</p>
            <p v-if="result.jiraKey && result.jiraUrl">
              Jira Issue:
              <a
                :href="result.jiraUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="font-mono underline hover:opacity-75"
              >{{ result.jiraKey }}</a>
            </p>
          </div>
        </transition>
      </div>
    </div>
  </AppLayout>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
