<script setup lang="ts">
import { useToast } from '@/composables/useToast'

const { toasts, dismiss } = useToast()

const icons = {
  success: '✓',
  error: '✕',
  info: 'ℹ',
}
const colors = {
  success: 'bg-green-50 border-green-400 text-green-800',
  error: 'bg-red-50 border-red-400 text-red-800',
  info: 'bg-blue-50 border-blue-400 text-blue-800',
}
</script>

<template>
  <div class="fixed bottom-4 right-4 z-50 flex flex-col gap-2 w-80">
    <TransitionGroup name="toast">
      <div
        v-for="t in toasts"
        :key="t.id"
        :class="['flex items-start gap-2 rounded-lg border px-4 py-3 shadow-md text-sm', colors[t.type]]"
      >
        <span class="font-bold mt-0.5">{{ icons[t.type] }}</span>
        <p class="flex-1 leading-snug">{{ t.message }}</p>
        <button @click="dismiss(t.id)" class="opacity-60 hover:opacity-100 ml-1">✕</button>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.25s ease;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(20px);
}
</style>
