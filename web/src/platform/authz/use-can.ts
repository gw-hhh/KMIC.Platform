import { computed, type ComputedRef } from 'vue';
import type { PermissionCode } from '@/generated/permissions';
import { useCurrentUserStore } from '@/platform/session/current-user';

/** 用于新建等不针对单条数据的按钮 */
export function useCan(permission: PermissionCode): ComputedRef<boolean> {
  const currentUser = useCurrentUserStore();
  return computed(() => currentUser.has(permission));
}
