<script setup lang="ts">
import { shellTexts } from '@/platform/texts/shell';

defineProps<{ title: string; description?: string }>();
</script>

<template>
  <div class="auth-page">
    <div class="card">
      <div class="brand">
        <span class="mark">K</span>
        <span class="name">{{ shellTexts.appName }}</span>
      </div>
      <h1 class="title">{{ title }}</h1>
      <p v-if="description" class="description">{{ description }}</p>
      <slot />
    </div>
  </div>
</template>

<style scoped lang="scss">
.auth-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 16px;
  color: var(--kmic-color-foreground);
  background: var(--kmic-color-background-deep);
}

.card {
  width: 100%;
  max-width: 400px;
  padding: 32px;
  background: var(--kmic-color-background);
  border: 1px solid var(--kmic-color-border);
  border-radius: var(--kmic-radius);
}

.brand {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 24px;
}

.mark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  font-weight: 700;
  color: var(--kmic-color-primary-foreground);
  background: var(--kmic-color-primary);
  border-radius: var(--kmic-radius);
}

.name {
  font-size: 20px;
  font-weight: 600;
}

.title {
  margin: 0 0 8px;
  font-size: 20px;
}

.description {
  margin: 0 0 16px;
  color: var(--kmic-color-muted-foreground);
}
</style>
