import {
  useQuery as useTanstackQuery,
  type QueryKey,
  type UseQueryOptions,
  type UseQueryReturnType,
} from '@tanstack/vue-query';
import {
  computed,
  getCurrentScope,
  onScopeDispose,
  ref,
  shallowRef,
  toValue,
  unref,
  watchEffect,
  type ComputedRef,
  type MaybeRefOrGetter,
  type Ref,
} from 'vue';

export type LeaveStatus = 'free' | 'dirty' | 'saving' | 'unknown';

export interface PageInstance {
  readonly key: string;
  readonly epoch: number;
  readonly title: Readonly<Ref<string | null>>;
  readonly active: ComputedRef<boolean>;
  readonly leaveStatus: ComputedRef<LeaveStatus>;
  readonly signal: AbortSignal;
  setTitle(title: string | null): void;
  /** 动态绑定标签页标题；返回空值时回退至路由默认标题 */
  useTitle(title: () => string | null | undefined): void;
  /**
   * 页面级查询：非活动标签页暂停请求；缓存失效时仅当前页立即刷新，其余标签页切回时再后台刷新
   * @remarks `enabled` 仅支持布尔值、Ref 或无参 getter
   */
  useQuery<TQueryFnData, TError = Error, TData = TQueryFnData, TQueryKey extends QueryKey = QueryKey>(
    options: () => UseQueryOptions<TQueryFnData, TError, TData, TQueryFnData, TQueryKey>,
  ): UseQueryReturnType<TData, TError>;
  trackDirty(isDirty: () => boolean): void;
  trackSaving(isSaving: () => boolean): void;
  trackUnknown(hasUnknown: () => boolean): void;
  destroy(): void;
}

export function createPageInstance(options: { key: string; epoch: number; isActive?: () => boolean }): PageInstance {
  const controller = new AbortController();
  const title = ref<string | null>(null);
  const dirty = shallowRef<readonly (() => boolean)[]>([]);
  const saving = shallowRef<readonly (() => boolean)[]>([]);
  const unknown = shallowRef<readonly (() => boolean)[]>([]);
  const active = computed(options.isActive ?? (() => true));

  const leaveStatus = computed<LeaveStatus>(() => {
    if (unknown.value.some((check) => check())) {
      return 'unknown';
    }
    if (saving.value.some((check) => check())) {
      return 'saving';
    }
    return dirty.value.some((check) => check()) ? 'dirty' : 'free';
  });

  // 作用域销毁（组件卸载）时自动注销
  function track(list: Ref<readonly (() => boolean)[]>, check: () => boolean): void {
    list.value = [...list.value, check];
    if (getCurrentScope()) {
      onScopeDispose(() => {
        list.value = list.value.filter((item) => item !== check);
      });
    }
  }

  return {
    key: options.key,
    epoch: options.epoch,
    title,
    active,
    leaveStatus,
    signal: controller.signal,
    setTitle(value) {
      title.value = value;
    },
    useTitle(value) {
      watchEffect(() => {
        title.value = value() ?? null;
      });
    },
    useQuery(getOptions) {
      return useTanstackQuery(() => {
        const queryOptions = unref(getOptions());
        const enabled = queryOptions.enabled as MaybeRefOrGetter<boolean | undefined>;
        return { ...queryOptions, enabled: active.value && toValue(enabled) !== false };
      });
    },
    trackDirty(check) {
      track(dirty, check);
    },
    trackSaving(check) {
      track(saving, check);
    },
    trackUnknown(check) {
      track(unknown, check);
    },
    destroy() {
      controller.abort();
    },
  };
}
