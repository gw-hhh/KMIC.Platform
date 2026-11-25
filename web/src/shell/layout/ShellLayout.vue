<script setup lang="ts">
import { useEventListener, useMediaQuery } from '@vueuse/core';
import { message, Modal } from 'antdv-next';
import { computed, ref, watch } from 'vue';
import { RouterView, useRouter } from 'vue-router';
import type { MenuNode } from '@/platform/routing';
import { useCatalog } from '@/platform/routing/use-catalog';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { useAuth } from '@/platform/session/use-auth';
import { shellTexts } from '@/platform/texts/shell';
import { motionDurations, shellSizes, switchWithRipple } from '@/platform/theme';
import KIcon from '@/platform/ui/KIcon.vue';
import { useNavigationState } from '../navigation-state';
import SearchDialog from '../search/SearchDialog.vue';
import ScrollShadow from './ScrollShadow.vue';
import { useShellSettingsStore } from '../settings/shell-settings-store';
import { useWorkspace } from '../workspace/use-workspace';
import HeaderBar from './HeaderBar.vue';
import HeaderMenu from './menu/HeaderMenu.vue';
import { rootOf, toCrumbViews } from './menu/menu-tree';
import MixedColumn from './menu/MixedColumn.vue';
import SidebarMenu from './menu/SidebarMenu.vue';
import PageProgress from './PageProgress.vue';
import SettingsDrawer from './SettingsDrawer.vue';
import TabBar from './TabBar.vue';
import TabContent from './TabContent.vue';

const { homeRouteName } = defineProps<{ homeRouteName: string }>();

const t = shellTexts;
const router = useRouter();
const catalog = useCatalog();
const workspace = useWorkspace();
const auth = useAuth();
const navigation = useNavigationState();
const currentUser = useCurrentUserStore();
const shellSettings = useShellSettingsStore();

const settings = computed(() => shellSettings.settings);
const me = computed(() => currentUser.me);

// 小于 1280 自动折叠成图标，小于 768 改为抽屉
const isCompact = useMediaQuery('(max-width: 1279px)');
const isMobile = useMediaQuery('(max-width: 767px)');
const mobileOpen = ref(false);
const hovering = ref(false);
// 拖动侧栏边缘时，侧栏先不动，只有一条线跟着鼠标走，松手后才改宽度
const drag = ref<{ offset: number; out: boolean } | null>(null);
const maximized = ref(false);
const settingsOpen = ref(false);
const searchOpen = ref(false);

// 窄屏一律按垂直布局显示
const layout = computed(() => (isMobile.value ? 'vertical' : settings.value.layout));

const nodes = computed(() => catalog.menu((permission) => currentUser.has(permission)));

// 没有菜单项的页面，高亮上级菜单
const activeRouteName = computed(
  () => workspace.tabs.value.find((tab) => tab.key === workspace.activeKey.value)?.routeName ?? null,
);
const activeMenuKey = computed(() => {
  for (let entry = catalog.find(activeRouteName.value ?? ''); entry; entry = catalog.find(entry.parent ?? '')) {
    if (entry.menu) {
      return entry.name;
    }
  }
  return null;
});
const crumbs = computed(() =>
  activeRouteName.value === null ? [] : toCrumbViews(catalog.breadcrumb(activeRouteName.value), nodes.value),
);

// 双列和混合垂直时，侧栏只显示选中的一级目录下的菜单
const selectedRootKey = ref<string | null>(null);
const previewRootKey = ref<string | null>(null);
watch(
  [activeMenuKey, nodes],
  () => {
    selectedRootKey.value = rootOf(nodes.value, activeMenuKey.value)?.key ?? selectedRootKey.value;
  },
  { immediate: true },
);
const shownRoot = computed(() =>
  nodes.value.find((node) => node.key === (previewRootKey.value ?? selectedRootKey.value)),
);
const sideNodes = computed(() => (layout.value === 'vertical' ? nodes.value : (shownRoot.value?.children ?? [])));

const collapsed = computed(() => settings.value.sidebar.collapsed || (isCompact.value && !isMobile.value));
const expandedWidth = computed(() => settings.value.sidebar.width);
// 双列布局下不隐藏，只折叠
const hidden = computed(() => settings.value.sidebar.hidden && !isMobile.value && layout.value !== 'mixed-column');
const hasSideMenu = computed(() => sideNodes.value.length > 0);

// 双列：第一列固定宽度；第二列折叠时隐藏，鼠标移到侧栏上时浮出来
const columnWidth = shellSizes.mixedColumnWidth;
const panelPinned = computed(() => !collapsed.value && hasSideMenu.value);
const panelVisible = computed(() => hasSideMenu.value && (!collapsed.value || hovering.value));

const hoverExpanded = computed(
  () =>
    layout.value !== 'mixed-column' &&
    collapsed.value &&
    settings.value.sidebar.hoverExpandAll &&
    hovering.value &&
    !isMobile.value,
);
const menuCollapsed = computed(
  () => layout.value !== 'mixed-column' && collapsed.value && !hoverExpanded.value && !isMobile.value,
);

// 悬停展开的侧栏浮在内容上，不改变内容宽度
const layoutWidth = computed(() => {
  if (isMobile.value || maximized.value || hidden.value) {
    return 0;
  }
  if (layout.value === 'mixed-column') {
    return columnWidth + (panelPinned.value ? expandedWidth.value : 0);
  }
  if (layout.value === 'mixed-vertical' && !hasSideMenu.value) {
    return 0;
  }
  return collapsed.value ? shellSizes.sidebarMinWidth : expandedWidth.value;
});
const sidebarWidth = computed(() => {
  if (layout.value === 'mixed-column') {
    return columnWidth + (panelVisible.value ? expandedWidth.value : 0);
  }
  return hoverExpanded.value || isMobile.value ? shellSizes.sidebarWidth : layoutWidth.value;
});
const sidebarFloating = computed(
  () => hoverExpanded.value || (layout.value === 'mixed-column' && panelVisible.value && !panelPinned.value),
);
const showSidebar = computed(
  () => !maximized.value && !hidden.value && (layout.value !== 'mixed-vertical' || hasSideMenu.value),
);
const dragBarVisible = computed(
  () =>
    showSidebar.value && !isMobile.value && !collapsed.value && (layout.value !== 'mixed-column' || panelPinned.value),
);

// 顶栏按钮：隐藏或显示整个侧栏；窄屏时开关抽屉
function toggleSidebar(): void {
  if (isMobile.value) {
    mobileOpen.value = !mobileOpen.value;
  } else if (layout.value === 'mixed-column') {
    toggleCollapsed();
  } else {
    shellSettings.update((draft) => {
      draft.sidebar.hidden = !draft.sidebar.hidden;
    });
  }
}

// 侧栏底部按钮：折叠成图标或展开
function toggleCollapsed(): void {
  shellSettings.update((draft) => {
    draft.sidebar.collapsed = !collapsed.value;
  });
}

async function openRoute(routeName: string): Promise<void> {
  mobileOpen.value = false;
  await workspace.open(routeName);
}

// 站内消息的链接是站内路径，按路由目录换成页面和参数再开标签
async function openLink(path: string): Promise<void> {
  const target = router.resolve(path);
  const name = typeof target.name === 'string' ? target.name : null;
  if (name === null || catalog.find(name) === undefined) {
    message.warning(t.messages.linkUnavailable);
    return;
  }
  const params = Object.fromEntries(
    Object.entries(target.params).map(([key, value]) => [key, Array.isArray(value) ? value.join('/') : value]),
  );
  await workspace.open(name, params);
}

async function selectRoot(node: MenuNode): Promise<void> {
  if (node.children.length > 0) {
    selectedRootKey.value = node.key;
    previewRootKey.value = null;
  } else if (node.routeName !== undefined) {
    await openRoute(node.routeName);
  }
}

function previewRoot(node: MenuNode): void {
  if (collapsed.value) {
    previewRootKey.value = node.key;
  }
}

function leaveSidebar(): void {
  hovering.value = false;
  previewRootKey.value = null;
}

function startDrag(event: MouseEvent): void {
  event.preventDefault();
  const startX = event.clientX;
  const startWidth = expandedWidth.value;
  const min = shellSizes.sidebarMinWidth;
  const max = shellSizes.sidebarMaxWidth;
  const widthAt = (clientX: number): number => startWidth + clientX - startX;
  const clamp = (width: number): number => Math.min(max, Math.max(min, width));

  const stopMove = useEventListener(window, 'mousemove', (move: MouseEvent) => {
    const width = widthAt(move.clientX);
    drag.value = { offset: clamp(width) - startWidth, out: width < min || width > max };
  });
  const stopUp = useEventListener(window, 'mouseup', (up: MouseEvent) => {
    stopMove();
    stopUp();
    // 只是点了一下（比如双击恢复宽度），不算拖动
    if (drag.value === null) {
      return;
    }
    drag.value = null;
    const width = clamp(widthAt(up.clientX));
    // 拖到最窄就折叠成图标
    shellSettings.update((draft) => {
      if (width <= min) {
        draft.sidebar.collapsed = true;
      } else {
        draft.sidebar.width = width;
      }
    });
  });
}

// 取消固定：侧栏收起，鼠标还在上面所以先保持展开，移开后收回；固定：一直展开
function togglePinned(): void {
  const unpin = !hoverExpanded.value;
  shellSettings.update((draft) => {
    draft.sidebar.collapsed = unpin;
    draft.sidebar.hoverExpandAll = unpin;
  });
  hovering.value = unpin;
}

function resetWidth(): void {
  shellSettings.update((draft) => {
    draft.sidebar.width = shellSizes.sidebarWidth;
  });
}

async function toggleTheme(origin: { x: number; y: number }): Promise<void> {
  const next = shellSettings.resolvedMode === 'dark' ? 'light' : 'dark';
  await switchWithRipple(
    origin,
    () => {
      shellSettings.update((draft) => {
        draft.appearance.mode = next;
      });
    },
    { reducedMotion: shellSettings.reducedMotion, durationMs: motionDurations.theme },
  );
}

function confirmLogout(): Promise<boolean> {
  return new Promise((resolve) => {
    Modal.confirm({
      title: t.leave.title,
      content: t.header.logoutConfirm,
      onOk: () => {
        resolve(true);
      },
      onCancel: () => {
        resolve(false);
      },
    });
  });
}

async function logout(): Promise<void> {
  if (!(await confirmLogout()) || !(await workspace.canLeaveAll())) {
    return;
  }
  await auth.logout();
  workspace.reset();
  await router.replace({ name: 'auth.login' });
}

async function switchRole(roleId: string): Promise<void> {
  if (!(await workspace.canLeaveAll())) {
    return;
  }
  await auth.switchRole(roleId);
  workspace.onEpochChange();
  message.success(t.header.roleSwitched.replace('{role}', me.value?.currentRole.name ?? ''));
}

async function changePassword(): Promise<void> {
  await workspace.open('shell.change-password');
}

async function resetSettings(): Promise<void> {
  await shellSettings.reset();
}
</script>

<template>
  <div v-if="me" class="shell" :class="`layout-${layout}`">
    <PageProgress :visible="settings.transition.progress && navigation.visible.value" />

    <div v-if="isMobile && mobileOpen" class="mask" @click="mobileOpen = false" />

    <HeaderBar
      v-if="layout === 'mixed-vertical' && !maximized"
      class="top-header"
      :me="me"
      :crumbs="crumbs"
      :show-breadcrumb="false"
      :show-breadcrumb-icon="false"
      :dark="shellSettings.resolvedMode === 'dark'"
      :sidebar-hidden="hidden"
      @toggle-sidebar="toggleSidebar"
      @open-search="searchOpen = true"
      @open-link="openLink"
      @toggle-theme="toggleTheme"
      @open-settings="settingsOpen = true"
      @switch-role="switchRole"
      @change-password="changePassword"
      @logout="logout"
    >
      <template #logo>
        <a class="logo header-logo" @click.prevent="openRoute(homeRouteName)">
          <span class="logo-mark">K</span>
          <span class="logo-text">{{ t.appName }}</span>
        </a>
      </template>
      <template #nav>
        <HeaderMenu :nodes="nodes" :active-key="selectedRootKey" @select="selectRoot" />
      </template>
    </HeaderBar>

    <aside
      v-show="showSidebar"
      class="sidebar"
      :class="{ 'is-hidden': isMobile && !mobileOpen, 'is-floating': sidebarFloating }"
      :style="{ width: `${sidebarWidth}px` }"
      @mouseenter="hovering = true"
      @mouseleave="leaveSidebar"
    >
      <template v-if="layout === 'mixed-column'">
        <div class="column">
          <a class="logo" @click.prevent="openRoute(homeRouteName)">
            <span class="logo-mark">K</span>
          </a>
          <ScrollShadow class="menu-scroll">
            <MixedColumn
              :nodes="nodes"
              :active-key="previewRootKey ?? selectedRootKey"
              @select="selectRoot"
              @enter="previewRoot"
            />
          </ScrollShadow>
          <button class="collapse-button" :aria-label="t.header.toggleSidebar" @click="toggleCollapsed">
            <KIcon :name="collapsed ? 'chevrons-right' : 'chevrons-left'" :size="16" />
          </button>
        </div>
        <div v-if="panelVisible" class="panel" :style="{ width: `${expandedWidth}px` }">
          <div class="panel-title">{{ shownRoot?.title }}</div>
          <ScrollShadow class="menu-scroll">
            <SidebarMenu
              :nodes="sideNodes"
              :active-key="activeMenuKey"
              :collapsed="false"
              :accordion="settings.sidebar.accordion"
              :show-collapsed-title="false"
              @select="openRoute"
            />
          </ScrollShadow>
        </div>
      </template>

      <template v-else>
        <a
          v-if="layout === 'vertical'"
          class="logo"
          :class="{ 'is-full': !menuCollapsed }"
          @click.prevent="openRoute(homeRouteName)"
        >
          <span class="logo-mark">K</span>
          <span v-if="!menuCollapsed" class="logo-text">{{ t.appName }}</span>
        </a>
        <ScrollShadow class="menu-scroll">
          <SidebarMenu
            :nodes="sideNodes"
            :active-key="activeMenuKey"
            :collapsed="menuCollapsed"
            :accordion="settings.sidebar.accordion"
            :show-collapsed-title="settings.sidebar.showCollapsedTitle"
            @select="openRoute"
          />
        </ScrollShadow>
        <button
          v-if="!isMobile && !hoverExpanded"
          class="collapse-button"
          :aria-label="t.header.toggleSidebar"
          @click="toggleCollapsed"
        >
          <KIcon :name="collapsed ? 'chevrons-right' : 'chevrons-left'" :size="16" />
        </button>
        <button
          v-if="!isMobile && (hoverExpanded || !collapsed)"
          class="pin-button"
          :aria-label="hoverExpanded ? t.header.pinSidebar : t.header.unpinSidebar"
          @click="togglePinned"
        >
          <KIcon :name="hoverExpanded ? 'pin-off' : 'pin'" :size="14" />
        </button>
      </template>
    </aside>

    <div
      v-if="dragBarVisible"
      class="drag-bar"
      :class="{ 'is-dragging': drag, 'is-out': drag?.out }"
      :style="{ left: `${layoutWidth - 1 + (drag?.offset ?? 0)}px` }"
      @mousedown="startDrag"
      @dblclick="resetWidth"
    />
    <div v-if="drag" class="drag-overlay" :class="{ 'is-out': drag.out }" />

    <div class="main" :style="{ marginLeft: `${layoutWidth}px` }">
      <HeaderBar
        v-if="layout !== 'mixed-vertical' && !maximized"
        :me="me"
        :crumbs="crumbs"
        :show-breadcrumb="settings.breadcrumb.show"
        :show-breadcrumb-icon="settings.breadcrumb.showIcon"
        :dark="shellSettings.resolvedMode === 'dark'"
        :sidebar-hidden="hidden"
        @toggle-sidebar="toggleSidebar"
        @open-search="searchOpen = true"
        @toggle-theme="toggleTheme"
        @open-settings="settingsOpen = true"
        @open-crumb="openRoute"
        @open-link="openLink"
        @switch-role="switchRole"
        @change-password="changePassword"
        @logout="logout"
      />
      <TabBar
        v-if="settings.tabs.show"
        v-model:maximized="maximized"
        :workspace="workspace"
        :catalog="catalog"
        :home-route-name="homeRouteName"
        :tab-style="settings.tabs.style"
        :show-icon="settings.tabs.showIcon"
      />
      <TabContent class="content" :workspace="workspace" :catalog="catalog" :settings="settings">
        <RouterView />
      </TabContent>
    </div>

    <SearchDialog v-model:open="searchOpen" :nodes="nodes" :user-id="me.userId" @select="openRoute" />

    <SettingsDrawer
      v-model:open="settingsOpen"
      :settings="settings"
      :save-failed="shellSettings.saveFailed"
      @change="shellSettings.update"
      @reset="resetSettings"
    />
  </div>
</template>

<style scoped lang="scss">
.shell {
  display: flex;
  min-height: 100vh;
  color: var(--kmic-color-foreground);
  background: var(--kmic-color-background-deep);
}

.mask {
  position: fixed;
  inset: 0;
  z-index: var(--kmic-z-mask);
  background: rgb(0 0 0 / 45%);
}

.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  z-index: var(--kmic-z-sidebar);
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow: hidden;
  color: var(--kmic-sidebar-foreground);
  background: var(--kmic-sidebar-background);
  border-right: 1px solid var(--kmic-sidebar-border);

  // 内容区宽度一次到位，避免表格逐帧重算
  transition:
    width var(--kmic-motion-slow) var(--kmic-easing-drawer),
    transform var(--kmic-motion-slow) var(--kmic-easing-drawer);

  &.is-hidden {
    transform: translateX(-100%);
  }

  &.is-floating {
    box-shadow: 4px 0 12px rgb(0 0 0 / 12%);
  }
}

.layout-mixed-column .sidebar {
  flex-direction: row;
}

.layout-mixed-vertical {
  .top-header {
    position: fixed;
    top: 0;
    right: 0;
    left: 0;
    z-index: var(--kmic-z-header);
  }

  .sidebar {
    top: var(--kmic-header-height);
    height: calc(100vh - var(--kmic-header-height));
  }

  .main {
    height: calc(100vh - var(--kmic-header-height));
    margin-top: var(--kmic-header-height);
  }
}

.column {
  position: relative;
  display: flex;
  flex: none;
  flex-direction: column;
  width: 80px;
  border-right: 1px solid var(--kmic-sidebar-border);
}

.panel {
  position: relative;
  display: flex;
  flex: none;
  flex-direction: column;
}

.panel-title {
  display: flex;
  flex: none;
  align-items: center;
  height: var(--kmic-header-height);
  padding: 0 16px;
  font-weight: 600;
}

.logo {
  display: flex;
  flex: none;
  gap: 8px;
  align-items: center;
  justify-content: center;

  &.is-full {
    justify-content: flex-start;
    padding: 0 12px;
  }

  height: var(--kmic-header-height);
  overflow: hidden;
  color: inherit;
  cursor: pointer;
}

.header-logo {
  padding: 0 12px 0 4px;
}

.logo-mark {
  display: flex;
  flex: none;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  font-weight: 700;
  color: var(--kmic-color-primary-foreground);
  background: var(--kmic-color-primary);
  border-radius: var(--kmic-radius);
}

.logo-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 18px;
  font-weight: 600;
}

// 底部 42px 留给折叠、固定按钮，菜单不会被挡住
.menu-scroll {
  flex: 1;
  margin-bottom: 42px;
}

.collapse-button,
.pin-button {
  position: absolute;
  bottom: 8px;
  z-index: 2;
  display: flex;
  padding: 4px;
  color: var(--kmic-sidebar-muted-color);
  cursor: pointer;
  background: var(--kmic-sidebar-accent);
  border: 0;
  border-radius: var(--kmic-radius-sm);
  transition:
    background var(--kmic-motion-fast),
    color var(--kmic-motion-fast);

  &:hover {
    color: var(--kmic-sidebar-foreground);
    background: var(--kmic-sidebar-accent-hover);
  }
}

.collapse-button {
  left: 12px;
}

.pin-button {
  right: 12px;
  padding: 5px;
}

.column .collapse-button {
  left: 50%;
  transform: translateX(-50%);
}

.drag-bar {
  position: fixed;
  top: 0;
  bottom: 0;
  z-index: var(--kmic-z-header);
  width: 2px;
  cursor: col-resize;
  transition: background var(--kmic-motion-fast);

  &:hover,
  &.is-dragging {
    background: var(--kmic-color-primary);
  }

  &.is-out {
    opacity: 0.3;
  }
}

.layout-mixed-vertical .drag-bar {
  top: var(--kmic-header-height);
}

// 拖动时盖一层，鼠标移到哪都保持拖动的光标，也不会选中文字
.drag-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--kmic-z-progress);
  cursor: col-resize;
  user-select: none;

  &.is-out {
    cursor: not-allowed;
  }
}

.main {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
  height: 100vh;
}

.content {
  flex: 1;
  padding: 16px;
  overflow: auto;
}
</style>
