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
    throw new Error('查询没有创建');
  }
  return { queryClient, active, page, fetcher, query, scope };
}

async function settle(): Promise<void> {
  await nextTick();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await nextTick();
}

describe('页面查询', () => {
  it('当前标签：写入后失效立即重新获取', async () => {
    const { queryClient, fetcher, query } = setup({ active: true });
    await settle();
    expect(fetcher).toHaveBeenCalledTimes(1);

    await queryClient.invalidateQueries({ queryKey: key });
    await settle();

    expect(fetcher).toHaveBeenCalledTimes(2);
    expect(query.data.value).toBe(2);
  });

  it('其他标签：失效时不请求，切回来才在后台刷新，期间旧数据保留', async () => {
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

  it('页面自己关掉的查询，当前标签也不请求', async () => {
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
  it('跟着数据变，没有数据时退回路由标题', async () => {
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
