import { effectScope, ref } from 'vue';
import { describe, expect, it } from 'vitest';
import { createPageInstance } from './page-instance';

describe('页面实例', () => {
  it('未注册任何阻止状态时允许直接离开', () => {
    const page = createPageInstance({ key: 'a', epoch: 0 });

    expect(page.leaveStatus.value).toBe('free');
  });

  it('同时存在多种状态时按优先级（结果未知 > 保存中 > 未保存）返回离开状态', () => {
    const page = createPageInstance({ key: 'a', epoch: 0 });
    const dirty = ref(false);
    const saving = ref(false);
    const unknown = ref(false);
    page.trackDirty(() => dirty.value);
    page.trackSaving(() => saving.value);
    page.trackUnknown(() => unknown.value);

    dirty.value = true;
    expect(page.leaveStatus.value).toBe('dirty');
    saving.value = true;
    expect(page.leaveStatus.value).toBe('saving');
    unknown.value = true;
    expect(page.leaveStatus.value).toBe('unknown');
  });

  it('注册状态的响应式作用域销毁后自动注销该状态', () => {
    const page = createPageInstance({ key: 'a', epoch: 0 });
    const scope = effectScope();
    scope.run(() => {
      page.trackDirty(() => true);
    });
    expect(page.leaveStatus.value).toBe('dirty');

    scope.stop();

    expect(page.leaveStatus.value).toBe('free');
  });

  it('实例销毁时中止关联的读取请求信号', () => {
    const page = createPageInstance({ key: 'a', epoch: 3 });

    page.destroy();

    expect(page.signal.aborted).toBe(true);
    expect(page.epoch).toBe(3);
  });
});
