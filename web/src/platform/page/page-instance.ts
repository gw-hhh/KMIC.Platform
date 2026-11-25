import { computed, getCurrentScope, onScopeDispose, ref, shallowRef, type ComputedRef, type Ref } from 'vue';

export type LeaveStatus = 'free' | 'dirty' | 'saving' | 'unknown';

export interface PageInstance {
  readonly key: string;
  readonly epoch: number;
  readonly title: Readonly<Ref<string | null>>;
  readonly leaveStatus: ComputedRef<LeaveStatus>;
  readonly signal: AbortSignal;
  setTitle(title: string | null): void;
  trackDirty(isDirty: () => boolean): void;
  trackSaving(isSaving: () => boolean): void;
  trackUnknown(hasUnknown: () => boolean): void;
  destroy(): void;
}

export function createPageInstance(options: { key: string; epoch: number }): PageInstance {
  const controller = new AbortController();
  const title = ref<string | null>(null);
  const dirty = shallowRef<readonly (() => boolean)[]>([]);
  const saving = shallowRef<readonly (() => boolean)[]>([]);
  const unknown = shallowRef<readonly (() => boolean)[]>([]);

  const leaveStatus = computed<LeaveStatus>(() => {
    if (unknown.value.some((check) => check())) {
      return 'unknown';
    }
    if (saving.value.some((check) => check())) {
      return 'saving';
    }
    return dirty.value.some((check) => check()) ? 'dirty' : 'free';
  });

  // 组件卸载时自动撤销
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
    leaveStatus,
    signal: controller.signal,
    setTitle(value) {
      title.value = value;
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
