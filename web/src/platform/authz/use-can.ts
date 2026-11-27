import { computed, type ComputedRef } from 'vue';
import type { PermissionCode } from '@/generated/permissions';
import { useCurrentUserStore } from '@/platform/session/current-user';

/** 判断当前用户是否具备指定权限（常用于新建等非单条数据操作按钮） */
export function useCan(permission: PermissionCode): ComputedRef<boolean> {
  const currentUser = useCurrentUserStore();
  return computed(() => currentUser.has(permission));
}
