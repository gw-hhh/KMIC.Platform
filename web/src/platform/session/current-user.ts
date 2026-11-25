import { defineStore } from 'pinia';
import { computed, ref, shallowRef } from 'vue';
import type { components } from '@/generated/api';
import type { PermissionCode } from '@/generated/permissions';
import type { KmicError } from '@/platform/api';

export type Me = components['schemas']['MeResponse'];

/**
 * starting 恢复登录中；anonymous 未登录；ready 已登录；password-change-required 要先改密码；
 * role-unavailable 当前角色失效，要重新选；failed 启动出错
 */
export type SessionStatus =
  'starting' | 'anonymous' | 'ready' | 'password-change-required' | 'role-unavailable' | 'failed';

export const useCurrentUserStore = defineStore('currentUser', () => {
  const status = ref<SessionStatus>('starting');
  const me = shallowRef<Me | null>(null);
  const failure = shallowRef<KmicError | null>(null);

  const permissions = computed<ReadonlySet<string>>(() => new Set(me.value?.permissions ?? []));

  function has(permission: PermissionCode): boolean {
    return permissions.value.has(permission);
  }

  function setMe(value: Me): void {
    me.value = value;
    failure.value = null;
    status.value = value.roles.some((role) => role.id === value.currentRole.id) ? 'ready' : 'role-unavailable';
  }

  function setStatus(
    value: Exclude<SessionStatus, 'ready' | 'role-unavailable'>,
    error: KmicError | null = null,
  ): void {
    status.value = value;
    failure.value = error;
    if (value === 'anonymous') {
      me.value = null;
    }
  }

  return { status, me, failure, permissions, has, setMe, setStatus };
});
