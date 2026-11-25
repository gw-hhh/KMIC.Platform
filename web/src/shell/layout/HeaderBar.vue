<script setup lang="ts">
import { Dropdown, Popover, Tag } from 'antdv-next';
import type { MenuProps } from 'antdv-next';
import { computed, ref } from 'vue';
import type { Me } from '@/platform/session/current-user';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';
import KTooltip from '@/platform/ui/KTooltip.vue';
import JobsButton from '../jobs/JobsButton.vue';
import MessagesButton from '../messages/MessagesButton.vue';
import type { CrumbView } from './menu/menu-tree';
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

const openCrumbIndex = ref<number | null>(null);

function openPage(routeName: string): void {
  openCrumbIndex.value = null;
  emit('openCrumb', routeName);
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
          <Popover
            v-if="crumb.pages.length > 0"
            :open="openCrumbIndex === index"
            trigger="click"
            placement="bottomLeft"
            :arrow="false"
            :classes="{ root: 'crumb-pages' }"
            @update:open="(value: boolean) => (openCrumbIndex = value ? index : null)"
          >
            <button class="crumb is-link is-dropdown" :class="{ 'is-open': openCrumbIndex === index }">
              <KIcon v-if="showBreadcrumbIcon && crumb.icon" :name="crumb.icon" :size="20" />
              {{ crumb.title }}
              <KIcon name="chevron-down" :size="16" />
            </button>
            <template #content>
              <button
                v-for="page in crumb.pages"
                :key="page.routeName"
                class="crumb-page"
                @click="openPage(page.routeName)"
              >
                {{ page.title }}
              </button>
            </template>
          </Popover>
          <button
            v-else-if="crumb.routeName !== undefined && index < crumbs.length - 1"
            class="crumb is-link"
            @click="emit('openCrumb', crumb.routeName)"
          >
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
      <button class="icon-button" :aria-label="dark ? t.lightMode : t.darkMode" @click="onToggleTheme">
        <KIcon :name="dark ? 'sun' : 'moon'" :size="16" />
      </button>
    </KTooltip>
    <KTooltip :title="t.settings">
      <button class="icon-button" :aria-label="t.settings" @click="emit('openSettings')">
        <KIcon name="settings" :size="16" />
      </button>
    </KTooltip>

    <Dropdown v-if="me.roles.length > 1" :menu="roleMenu" :trigger="['click']">
      <Tag class="role" :bordered="false" :title="t.switchRole">{{ me.currentRole.name }}</Tag>
    </Dropdown>
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

    &:hover,
    &.is-open {
      color: var(--kmic-header-foreground);
    }
  }

  &.is-current {
    color: var(--kmic-header-foreground);
  }
}

.crumb-page {
  display: flex;
  gap: 8px;
  align-items: center;
  width: 100%;
  padding: 6px 8px;
  font: inherit;
  color: var(--kmic-color-foreground);
  text-align: left;
  white-space: nowrap;
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: var(--kmic-radius-sm);

  &:hover,
  &:focus-visible {
    color: var(--kmic-color-accent-foreground);
    background: var(--kmic-color-accent);
    outline: none;
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
</style>

<style lang="scss">
// 面包屑下拉：紧凑的菜单样式，细边框、缩放淡入，照 Vben Admin 的下拉菜单
.crumb-pages .ant-popover-container {
  min-width: 128px;
  padding: 4px;
  border: 1px solid var(--kmic-color-border);
  border-radius: var(--kmic-radius-md);
  box-shadow:
    0 4px 6px -1px rgb(0 0 0 / 10%),
    0 2px 4px -2px rgb(0 0 0 / 10%);
  animation: crumb-pages-in var(--kmic-motion-fast) ease-out;
}

@keyframes crumb-pages-in {
  from {
    opacity: 0;
    transform: translateY(-8px) scale(0.95);
  }
}
</style>
