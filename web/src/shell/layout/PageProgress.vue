<script setup lang="ts">
defineProps<{ visible: boolean }>();
</script>

<template>
  <Transition name="progress">
    <div v-if="visible" class="page-progress" role="progressbar" aria-busy="true">
      <div class="bar" />
    </div>
  </Transition>
</template>

<style scoped lang="scss">
.page-progress {
  position: fixed;
  top: 0;
  left: 0;
  z-index: var(--kmic-z-progress);
  width: 100%;
  height: 2px;
  overflow: hidden;
  pointer-events: none;
}

.bar {
  width: 40%;
  height: 100%;
  background: var(--kmic-color-primary);
  animation: progress-run 1.2s ease-in-out infinite;
}

@keyframes progress-run {
  from {
    transform: translateX(-100%);
  }

  to {
    transform: translateX(250%);
  }
}

.progress-enter-active,
.progress-leave-active {
  transition: opacity var(--kmic-motion-medium);
}

.progress-enter-from,
.progress-leave-to {
  opacity: 0;
}
</style>
