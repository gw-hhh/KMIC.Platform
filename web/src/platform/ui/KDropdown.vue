<script setup lang="ts">
// 下拉菜单：细边框、紧凑行高、图标次要色，样式照 Vben Admin 的下拉菜单（MIT）
import { Dropdown, type DropdownProps } from 'antdv-next';

const {
  menu,
  trigger = ['click'],
  placement = 'bottomLeft',
} = defineProps<{
  menu: DropdownProps['menu'];
  trigger?: DropdownProps['trigger'];
  placement?: DropdownProps['placement'];
}>();
</script>

<template>
  <Dropdown :menu="menu" :trigger="trigger" :placement="placement" :classes="{ root: 'k-menu' }">
    <slot />
  </Dropdown>
</template>

<style lang="scss">
// 浮层挂在 body 下，只能用不带 scoped 的样式
.k-menu .ant-dropdown-menu {
  min-width: 128px;
  padding: 4px;
  border: 1px solid var(--kmic-color-border);
  border-radius: var(--kmic-radius-md);
  box-shadow:
    0 4px 6px -1px rgb(0 0 0 / 10%),
    0 2px 4px -2px rgb(0 0 0 / 10%);

  .ant-dropdown-menu-item {
    gap: 8px;
    padding: 6px 8px;
    border-radius: var(--kmic-radius-sm);

    .ant-dropdown-menu-item-icon {
      margin: 0;
      color: var(--kmic-color-muted-foreground);
    }

    &:hover:not(.ant-dropdown-menu-item-disabled) {
      color: var(--kmic-color-accent-foreground);
      background: var(--kmic-color-accent);
    }

    &.ant-dropdown-menu-item-disabled {
      opacity: 0.5;
    }
  }

  .ant-dropdown-menu-item-divider {
    margin: 4px -4px;
    background: var(--kmic-color-border);
  }
}
</style>
