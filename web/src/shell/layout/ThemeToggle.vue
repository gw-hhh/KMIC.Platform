<script setup lang="ts">
// 深浅色切换按钮：太阳与月亮互相变形，移植自 Vben Admin 的 theme-button（MIT）
const { dark } = defineProps<{ dark: boolean }>();
</script>

<template>
  <!-- 和 Vben 一样显示要切换到的模式：深色时显示太阳，浅色时显示月亮 -->
  <span class="theme-toggle" :class="dark ? 'is-light' : 'is-dark'">
    <svg aria-hidden="true" height="18" viewBox="0 0 24 24" width="18">
      <mask id="kmic-theme-toggle-moon" class="moon">
        <rect fill="white" height="100%" width="100%" x="0" y="0" />
        <circle cx="40" cy="8" fill="black" r="11" />
      </mask>
      <circle class="sun" cx="12" cy="12" fill="currentColor" mask="url(#kmic-theme-toggle-moon)" r="11" />
      <g class="sun-beams" stroke="currentColor" stroke-width="2">
        <line x1="12" x2="12" y1="1" y2="3" />
        <line x1="12" x2="12" y1="21" y2="23" />
        <line x1="4.22" x2="5.64" y1="4.22" y2="5.64" />
        <line x1="18.36" x2="19.78" y1="18.36" y2="19.78" />
        <line x1="1" x2="3" y1="12" y2="12" />
        <line x1="21" x2="23" y1="12" y2="12" />
        <line x1="4.22" x2="5.64" y1="19.78" y2="18.36" />
        <line x1="18.36" x2="19.78" y1="5.64" y2="4.22" />
      </g>
    </svg>
  </span>
</template>

<style scoped lang="scss">
// is-light 显示小太阳和光芒；is-dark 月亮的缺口移进来、光芒淡出
.theme-toggle {
  display: flex;
}

.moon > circle {
  transition: transform calc(var(--kmic-motion-slow) * 1.7) cubic-bezier(0, 0, 0.3, 1);
}

.sun {
  stroke: none;
  transform-origin: center center;
  transition: transform calc(var(--kmic-motion-slow) * 5.3) cubic-bezier(0.25, 0, 0.2, 1);
}

.sun-beams {
  transform-origin: center center;
  transition:
    transform calc(var(--kmic-motion-slow) * 5.3) cubic-bezier(0.5, 1.5, 0.75, 1.25),
    opacity calc(var(--kmic-motion-slow) * 2) cubic-bezier(0.25, 0, 0.3, 1);
}

.is-light {
  .sun {
    transform: scale(0.5);
  }

  .sun-beams {
    transform: rotateZ(0.25turn);
  }
}

.is-dark {
  .moon > circle {
    transform: translateX(-20px);
  }

  .sun-beams {
    opacity: 0;
  }
}
</style>
