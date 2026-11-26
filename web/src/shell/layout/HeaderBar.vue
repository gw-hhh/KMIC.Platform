<script setup lang="ts">
import { Tag } from 'antdv-next';
import type { MenuProps } from 'antdv-next';
import { computed } from 'vue';
import type { Me } from '@/platform/session/current-user';
import { shellTexts } from '@/platform/texts/shell';
import KDropdown from '@/platform/ui/KDropdown.vue';
import KIcon from '@/platform/ui/KIcon.vue';
import KTooltip from '@/platform/ui/KTooltip.vue';
import JobsButton from '../jobs/JobsButton.vue';
import MessagesButton from '../messages/MessagesButton.vue';
import type { CrumbView } from './menu/menu-tree';
import ThemeToggle from './ThemeToggle.vue';
import UserMenu from './UserMenu.vue';

const { me, crumbs, showBreadcrumb, showBreadcrumbIcon, dark, sidebarHidden } = defineProps<{
  me: Me;
  crumbs: readonly CrumbView[];
  showBreadcrumb: boolean;
  showBreadcrumbIcon: boolean;
  dark: boolean;
  sidebarHidden: boolean;
}>();

const emit = defineEmits<{
  toggleSidebar: [];
  openSearch: [];
  toggleTheme: [origin: { x: number; y: number }];
  openSettings: [];
  openCrumb: [routeName: string];
  openLink: [path: string];
  switchRole: [roleId: string];
  changePassword: [];
  logout: [];
}>();

const t = shellTexts.header;
const shortcut = /mac/i.test(navigator.userAgent) ? '⌘' : 'Ctrl';

const roleMenu = computed<MenuProps>(() => ({
  items: me.roles.map((role) => ({ key: role.id, label: role.name, disabled: role.id === me.currentRole.id })),
  onClick: ({ key }) => {
    emit('switchRole', key);
  },
}));

// 和 Vben 一样，点上级跳到它下面的第一个页面
function targetOf(crumb: CrumbView): string | undefined {
  return crumb.routeName ?? crumb.pages[0]?.routeName;
}

function openCrumb(crumb: CrumbView): void {
  const target = targetOf(crumb);
  if (target !== undefined) {
    emit('openCrumb', target);
  }
}

function onToggleTheme(event: MouseEvent): void {
  emit('toggleTheme', { x: event.clientX, y: event.clientY });
}
</script>

<template>
  <header class="header-bar">
    <slot name="logo" />
    <button
      class="icon-button is-square"
      :aria-label="sidebarHidden ? t.showSidebar : t.hideSidebar"
      @click="emit('toggleSidebar')"
    >
      <KIcon :name="sidebarHidden ? 'indent-increase' : 'indent-decrease'" :size="18" />
    </button>

    <slot v-if="$slots.nav" name="nav" />
    <nav v-else-if="showBreadcrumb" aria-label="breadcrumb">
      <TransitionGroup tag="ol" name="breadcrumb" class="breadcrumb">
        <li v-for="(crumb, index) in crumbs" :key="`${crumb.title}-${index}`" class="crumb-item">
          <KIcon v-if="index > 0" class="separator" name="chevron-right" :size="14" />
          <button v-if="index < crumbs.length - 1 && targetOf(crumb)" class="crumb is-link" @click="openCrumb(crumb)">
            <KIcon v-if="showBreadcrumbIcon && crumb.icon" :name="crumb.icon" :size="16" />
            {{ crumb.title }}
          </button>
          <span v-else class="crumb" :class="{ 'is-current': index === crumbs.length - 1 }">
            <KIcon v-if="showBreadcrumbIcon && crumb.icon" :name="crumb.icon" :size="16" />
            {{ crumb.title }}
          </span>
        </li>
      </TransitionGroup>
    </nav>

    <div class="spacer" />

    <button class="search-button" @click="emit('openSearch')">
      <KIcon name="search" :size="16" />
      <span class="search-text">{{ shellTexts.search.button }}</span>
      <kbd class="search-shortcut">{{ shortcut }} K</kbd>
    </button>

    <JobsButton />
    <MessagesButton :key="me.userId" :user-id="me.userId" @open-link="(path) => emit('openLink', path)" />

    <KTooltip :title="dark ? t.lightMode : t.darkMode">
      <button class="icon-button theme-button" :aria-label="dark ? t.lightMode : t.darkMode" @click="onToggleTheme">
        <ThemeToggle :dark="dark" />
      </button>
    </KTooltip>
    <KTooltip :title="t.settings">
      <button class="icon-button" :aria-label="t.settings" @click="emit('openSettings')">
        <KIcon name="settings" :size="16" />
      </button>
    </KTooltip>

    <KDropdown v-if="me.roles.length > 1" :menu="roleMenu" placement="bottomRight">
      <Tag class="role" :bordered="false" :title="t.switchRole">{{ me.currentRole.name }}</Tag>
    </KDropdown>
    <Tag v-else class="role" :bordered="false">{{ me.currentRole.name }}</Tag>

    <UserMenu :me="me" @change-password="emit('changePassword')" @logout="emit('logout')" />
  </header>
</template>

<style scoped lang="scss">
.header-bar {
  display: flex;
  gap: 4px;
  align-items: center;
  height: var(--kmic-header-height);
  padding: 0 0 0 8px;
  color: var(--kmic-header-foreground);
  background: var(--kmic-header-background);
  border-bottom: 1px solid var(--kmic-header-border);
}

.icon-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--kmic-header-icon-color);
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: 50%;
  transition:
    background var(--kmic-motion-fast),
    color var(--kmic-motion-fast);

  &:hover {
    color: var(--kmic-header-foreground);
    background: var(--kmic-header-accent);
  }

  &.theme-button:hover :deep(.theme-toggle) {
    animation: shrink var(--kmic-motion-slow) ease-in-out;
  }

  // 侧栏开关是圆角方块，其余图标按钮是圆形
  &.is-square {
    margin-right: 4px;
    border-radius: var(--kmic-radius-md);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
  }
}

.breadcrumb {
  display: flex;
  align-items: center;
  padding: 0;
  margin: 0 0 0 8px;
  color: var(--kmic-color-muted-foreground);
  list-style: none;
}

.crumb-item {
  display: flex;
  align-items: center;
}

// 前一项与箭头隔 6px，箭头与后一项隔 10px
.separator {
  margin: 0 10px 0 6px;
}

.crumb {
  display: flex;
  gap: 4px;
  align-items: center;
  padding: 0;
  font: inherit;
  color: inherit;
  background: none;
  border: 0;

  &.is-link {
    cursor: pointer;
    transition: color var(--kmic-motion-fast);

    &:hover {
      color: var(--kmic-header-foreground);
    }
  }

  &.is-current {
    color: var(--kmic-header-foreground);
  }
}

// 切换页面时新的面包屑斜着滑入，旧的直接消失
.breadcrumb-enter-active {
  transition:
    transform 0.4s cubic-bezier(0.76, 0, 0.24, 1),
    opacity 0.4s cubic-bezier(0.76, 0, 0.24, 1);
}

.breadcrumb-leave-active {
  display: none;
}

.breadcrumb-enter-from {
  opacity: 0;
  transform: translateX(30px) skewX(-30deg);
}

@media (prefers-reduced-motion: reduce) {
  .breadcrumb-enter-active {
    transition: none;
  }
}

.spacer {
  flex: 1;
}

.search-button {
  display: flex;
  gap: 12px;
  align-items: center;
  height: 32px;
  padding: 2px 8px;
  margin-right: 8px;
  font: inherit;
  color: var(--kmic-color-muted-foreground);
  cursor: pointer;
  background: var(--kmic-header-accent);
  border: 0;
  border-radius: var(--kmic-radius-pill);
  transition: color var(--kmic-motion-fast);

  &:hover {
    color: var(--kmic-header-foreground);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
  }
}

.search-text {
  font-size: 12px;
}

.search-shortcut {
  padding: 4px 6px;
  font: inherit;
  font-size: 12px;
  line-height: 1;
  background: var(--kmic-header-background);
  border-radius: var(--kmic-radius-pill);
}

// 窄屏只留图标
@media (width < 768px) {
  .search-button {
    width: 32px;
    padding: 0;
    margin-right: 0;
    background: none;
    justify-content: center;
  }

  .search-text,
  .search-shortcut {
    display: none;
  }
}

.role {
  margin: 0 4px 0 8px;
  cursor: pointer;
}

@keyframes shrink {
  50% {
    transform: scale(0.8);
  }
}
</style>
