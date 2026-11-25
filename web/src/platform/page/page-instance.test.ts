import { effectScope, ref } from 'vue';
import { describe, expect, it } from 'vitest';
import { createPageInstance } from './page-instance';

describe('页面实例', () => {
  it('没有登记任何状态时可以直接离开', () => {
    const page = createPageInstance({ key: 'a', epoch: 0 });

    expect(page.leaveStatus.value).toBe('free');
  });

  it('同时有多种状态时，按 结果未知 > 保存中 > 未保存 取一个', () => {
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

  it('登记它的作用域结束后自动撤销', () => {
    const page = createPageInstance({ key: 'a', epoch: 0 });
    const scope = effectScope();
    scope.run(() => {
      page.trackDirty(() => true);
    });
    expect(page.leaveStatus.value).toBe('dirty');

    scope.stop();

    expect(page.leaveStatus.value).toBe('free');
  });

  it('销毁时取消读请求', () => {
    const page = createPageInstance({ key: 'a', epoch: 3 });

    page.destroy();

    expect(page.signal.aborted).toBe(true);
    expect(page.epoch).toBe(3);
  });
});
