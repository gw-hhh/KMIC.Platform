<script setup lang="ts">
// 登录、修改密码与角色选择等独立认证页布局：左侧品牌展示区，右侧表单区，参考 Vben Admin 的 authentication 布局（MIT）
import { ConfigProvider } from 'antdv-next';
import { computed } from 'vue';
import { shellTexts } from '@/platform/texts/shell';
import KTooltip from '@/platform/ui/KTooltip.vue';
import ThemeToggle from '../layout/ThemeToggle.vue';
import { useShellSettingsStore } from '../settings/shell-settings-store';

defineProps<{ title: string; description?: string }>();

const t = shellTexts;
const shellSettings = useShellSettingsStore();
const dark = computed(() => shellSettings.resolvedMode === 'dark');
const toggleLabel = computed(() => (dark.value ? t.header.lightMode : t.header.darkMode));

function toggleTheme(event: MouseEvent): void {
  void shellSettings.toggleMode({ x: event.clientX, y: event.clientY });
}
</script>

<template>
  <div class="auth-page">
    <div class="brand">
      <span class="brand-mark">K</span>
      <span class="brand-name">{{ t.appName }}</span>
    </div>

    <div class="toolbar">
      <KTooltip :title="toggleLabel">
        <button type="button" class="toolbar-button" :aria-label="toggleLabel" @click="toggleTheme">
          <ThemeToggle :dark="dark" />
        </button>
      </KTooltip>
    </div>

    <!-- 品牌展示区：后续可替换为企业定制插画 -->
    <div class="intro">
      <div class="intro-glow" />
      <div class="intro-content enter-x">
        <div>
          <span class="intro-mark">K</span>
        </div>
        <div class="intro-title">{{ t.appName }}</div>
      </div>
    </div>

    <div class="form-panel">
      <div class="form-content enter-x">
        <h1 class="title">{{ title }}</h1>
        <p v-if="description" class="description">{{ description }}</p>
        <ConfigProvider component-size="large">
          <div class="body">
            <slot />
          </div>
        </ConfigProvider>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.auth-page {
  position: relative;
  display: flex;
  min-height: 100vh;
  overflow-x: hidden;
  color: var(--kmic-color-foreground);
  background: var(--kmic-color-background);
}

.brand {
  position: absolute;
  top: 16px;
  left: 16px;
  z-index: 2;
  display: flex;
  gap: 8px;
  align-items: center;
}

.brand-mark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 42px;
  height: 42px;
  font-size: 20px;
  font-weight: 700;
  color: var(--kmic-color-primary-foreground);
  background: var(--kmic-color-primary);
  border-radius: var(--kmic-radius);
}

.brand-name {
  font-size: 20px;
  font-weight: 500;
}

.toolbar {
  position: absolute;
  top: 16px;
  right: 8px;
  z-index: 2;
}

.toolbar-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--kmic-color-foreground);
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: 50%;
  transition: background var(--kmic-motion-fast);

  &:hover {
    background: var(--kmic-color-accent);
  }
}

// 展示区仅在宽屏显示，底色较表单区深一阶，中央叠加斜向主色光晕
.intro {
  position: relative;
  display: none;
  flex: 1;
  width: 0;
  overflow: hidden;
  background: var(--kmic-color-background-deep);

  @media (width >= 1024px) {
    display: block;
  }
}

.intro-glow {
  --glow: 30%;

  position: absolute;
  inset: 0;
  background: linear-gradient(
    154deg,
    transparent 30%,
    color-mix(in srgb, var(--kmic-color-primary) var(--glow), transparent) 48%,
    transparent 64%
  );
  filter: blur(100px);

  :global(html[data-theme='dark']) & {
    --glow: 20%;
  }
}

.intro-content {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  margin-right: 80px;
}

.intro-mark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 160px;
  height: 160px;
  font-size: 80px;
  font-weight: 700;
  color: var(--kmic-color-primary-foreground);
  background: var(--kmic-color-primary);
  border-radius: var(--kmic-radius);
  animation: float 5s linear infinite;
}

.intro-title {
  margin-top: 24px;
  font-size: 24px;
}

@keyframes float {
  0%,
  100% {
    transform: translateY(0);
  }

  50% {
    transform: translateY(-20px);
  }
}

@media (prefers-reduced-motion: reduce) {
  .intro-mark {
    animation: none;
  }
}

.form-panel {
  position: relative;
  display: flex;
  flex: 1;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 24px;
  background: var(--kmic-color-background);

  :global(html[data-theme='dark']) & {
    background: var(--kmic-color-background-deep);
  }

  @media (width >= 1024px) {
    flex: none;
    width: 40%;
    padding: 40px 32px;
  }
}

.form-content {
  width: 100%;
  max-width: 448px;
  margin-top: 24px;
}

.title {
  margin: 0 0 12px;
  font-size: 30px;
  font-weight: 700;
  line-height: 36px;
  letter-spacing: -0.025em;

  @media (width >= 1024px) {
    font-size: 36px;
    line-height: 40px;
  }
}

.description {
  margin: 0;
  font-size: 14px;
  color: var(--kmic-color-muted-foreground);
}

.body {
  margin-top: 28px;
}

// 进场动效：自右侧 50px 错峰淡入
.enter-x > * {
  opacity: 0;
  transform: translateX(50px);
  animation: enter-x var(--kmic-motion-slow) ease-in-out forwards;

  @for $i from 1 through 4 {
    &:nth-child(#{$i}) {
      animation-delay: calc(var(--kmic-motion-slow) / 3 * #{$i});
    }
  }
}

.intro .enter-x > * {
  transform: translateX(-50px);
}

@keyframes enter-x {
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>
