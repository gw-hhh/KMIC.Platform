import { inject, ref, watch, type InjectionKey, type Ref } from 'vue';

const showAfterMs = 200;
const showAtLeastMs = 500;

export interface NavigationState {
  /** 导航进度指示器可见状态（延迟 200ms 显示，展示后至少保持 500ms） */
  readonly visible: Readonly<Ref<boolean>>;
  start(): void;
  finish(): void;
}

export function createNavigationState(): NavigationState {
  const pending = ref(false);
  const visible = ref(false);
  let showTimer: ReturnType<typeof setTimeout> | undefined;
  let shownAt = 0;

  watch(pending, (isPending) => {
    if (isPending) {
      showTimer = setTimeout(() => {
        shownAt = Date.now();
        visible.value = true;
      }, showAfterMs);
      return;
    }
    clearTimeout(showTimer);
    if (visible.value) {
      const remaining = Math.max(0, showAtLeastMs - (Date.now() - shownAt));
      setTimeout(() => {
        if (!pending.value) {
          visible.value = false;
        }
      }, remaining);
    }
  });

  return {
    visible,
    start: () => {
      pending.value = true;
    },
    finish: () => {
      pending.value = false;
    },
  };
}

export const navigationKey: InjectionKey<NavigationState> = Symbol('kmic.navigation');

export function useNavigationState(): NavigationState {
  const state = inject(navigationKey, null);
  if (state === null) {
    throw new Error('navigationKey 未注入，请在 main.ts 中通过 app.provide 注册');
  }
  return state;
}
