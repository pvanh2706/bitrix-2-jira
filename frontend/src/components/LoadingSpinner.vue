<script setup lang="ts">
defineProps<{ size?: 'sm' | 'md' | 'lg' }>()
</script>

<template>
  <span
    :class="[
      'inline-block rounded-full border-2 border-current border-t-transparent animate-spin',
      { 'w-4 h-4': size === 'sm', 'w-6 h-6': !size || size === 'md', 'w-8 h-8': size === 'lg' },
    ]"
    role="status"
    aria-label="Loading"
  />
</template>
