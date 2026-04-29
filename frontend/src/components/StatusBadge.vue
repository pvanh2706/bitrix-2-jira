<script setup lang="ts">
import { computed } from 'vue'
import type { DealSummaryDto } from '@/api/types'

const props = defineProps<{ deal: DealSummaryDto }>()

const badge = computed(() => {
  if (props.deal.isSendDataToJira === 1 && props.deal.haveError === 0)
    return { label: 'Thành công', cls: 'bg-green-100 text-green-800' }
  if (props.deal.haveError === 1)
    return { label: 'Lỗi', cls: 'bg-red-100 text-red-800' }
  return { label: 'Chờ xử lý', cls: 'bg-yellow-100 text-yellow-800' }
})
</script>

<template>
  <span :class="['inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium', badge.cls]">
    {{ badge.label }}
  </span>
</template>
