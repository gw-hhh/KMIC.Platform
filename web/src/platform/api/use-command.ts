import { useQueryClient, type QueryKey } from '@tanstack/vue-query';
import { injectPage } from '@/platform/page';
import { createCommandRunner, type CommandHandle } from './command-runner';
import type { CommandDef } from './define-module-api';
import { isJobAccepted, jobsApi } from './jobs';
import { useApi } from './use-api';

export interface UseCommandOptions<Data> {
  /** 成功后失效的查询；可以按响应计算 */
  invalidates?: readonly QueryKey[] | ((data: Data) => readonly QueryKey[]);
}

/**
 * 页面里执行写操作。保存中、结果未知会登记到页面实例，关闭标签前会拦下（F03 第 6.2、7.5 节）
 */
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
      // 提交了后台作业，外壳的作业入口马上刷新并开始跟踪
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
