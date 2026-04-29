<script setup lang="ts">
import { RouterView } from 'vue-router'
import ToastContainer from '@/components/ToastContainer.vue'
</script>

<template>
  <div id="app-root">
    <RouterView />
    <ToastContainer />
  </div>
</template>
