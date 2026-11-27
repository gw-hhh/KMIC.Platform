import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { createApp, effectScope, nextTick, ref } from 'vue';
import { describe, expect, it, vi } from 'vitest';
import { createPageInstance, type PageInstance } from './page-instance';

const key = ['kmic', 'sample', '/api/v1/sample/samples', {}] as const;

function setup(options: { active: boolean; enabled?: () => boolean }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { staleTime: 0, retry: false } } });
  const app = createApp({});
  app.use(VueQueryPlugin, { queryClient });
  const active = ref(options.active);
  const page = createPageInstance({ key: 'a', epoch: 0, isActive: () => active.value });
  const fetcher = vi.fn(() => Promise.resolve(fetcher.mock.calls.length));
  const scope = effectScope();
  const query = app.runWithContext(() =>
    scope.run(() =>
      page.useQuery(() => ({
        queryKey: key,
        queryFn: fetcher,
        ...(options.enabled ? { enabled: options.enabled } : {}),
      })),
    ),
  );
  if (query === undefined) {
    throw new Error('页面查询实例未创建');
  }
  return { queryClient, active, page, fetcher, query, scope };
}

async function settle(): Promise<void> {
  await nextTick();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await nextTick();
}

describe('页面查询', () => {
  it('当前激活标签页在查询失效后立即重新获取数据', async () => {
    const { queryClient, fetcher, query } = setup({ active: true });
    await settle();
    expect(fetcher).toHaveBeenCalledTimes(1);

    await queryClient.invalidateQueries({ queryKey: key });
    await settle();

    expect(fetcher).toHaveBeenCalledTimes(2);
    expect(query.data.value).toBe(2);
  });

  it('非激活标签页在查询失效时不发起请求，切回激活后在后台刷新并保留旧数据', async () => {
    const { queryClient, active, fetcher, query } = setup({ active: true });
    await settle();
    active.value = false;
    await settle();

    await queryClient.invalidateQueries({ queryKey: key });
    await settle();
    expect(fetcher).toHaveBeenCalledTimes(1);
    expect(query.data.value).toBe(1);

    active.value = true;
    await settle();

    expect(fetcher).toHaveBeenCalledTimes(2);
    expect(query.data.value).toBe(2);
  });

  it('页面主动禁用查询时，即使处于激活状态也不发起请求', async () => {
    const enabled = ref(false);
    const { fetcher } = setup({ active: true, enabled: () => enabled.value });
    await settle();
    expect(fetcher).not.toHaveBeenCalled();

    enabled.value = true;
    await settle();

    expect(fetcher).toHaveBeenCalledTimes(1);
  });
});

describe('页面标题', () => {
  it('随数据响应式更新标题，无数据时回退为默认路由标题', async () => {
    const page: PageInstance = createPageInstance({ key: 'a', epoch: 0 });
    const number = ref<string | null>(null);
    const scope = effectScope();
    scope.run(() => {
      page.useTitle(() => (number.value === null ? undefined : `示例 ${number.value}`));
    });
    expect(page.title.value).toBeNull();

    number.value = 'SP-001';
    await nextTick();

    expect(page.title.value).toBe('示例 SP-001');
    scope.stop();
  });
});
