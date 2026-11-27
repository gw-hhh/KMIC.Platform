import { useQueryClient, type QueryKey } from '@tanstack/vue-query';
import { injectPage } from '@/platform/page';
import { createCommandRunner, type CommandHandle } from './command-runner';
import type { CommandDef } from './define-module-api';
import { isJobAccepted, jobsApi } from './jobs';
import { useApi } from './use-api';

export interface UseCommandOptions<Data> {
  /** 成功后需失效的查询键列表，支持根据响应结果动态计算 */
  invalidates?: readonly QueryKey[] | ((data: Data) => readonly QueryKey[]);
}

/** 在页面内执行写操作，自动将保存中与结果未知状态同步至当前页面实例，关闭或离开标签页前予以拦截确认 */
export function useCommand<Input, Data>(
  def: CommandDef<Input, Data>,
  options: UseCommandOptions<Data> = {},
): CommandHandle<Input, Data> {
  const queryClient = useQueryClient();
  const handle = createCommandRunner({
    def,
    api: useApi(),
    onSucceeded(data) {
      const keys = typeof options.invalidates === 'function' ? options.invalidates(data) : (options.invalidates ?? []);
      // 若提交了后台作业，立即失效作业列表缓存以触发外壳作业面板刷新与状态跟踪
      const all = isJobAccepted(data) ? [...keys, jobsApi.myJobs.all] : keys;
      for (const queryKey of all) {
        void queryClient.invalidateQueries({ queryKey });
      }
    },
  });

  const page = injectPage();
  page?.trackSaving(() => handle.state.value === 'saving');
  page?.trackUnknown(() => handle.state.value === 'unknown');
  return handle;
}
