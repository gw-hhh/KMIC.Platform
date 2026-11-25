import type { components } from '@/generated/api';
import { defineModuleApi } from './define-module-api';

export type BackgroundJob = components['schemas']['BackgroundJobItem'];
export type BackgroundJobStatus = components['schemas']['BackgroundJobStatus'];

/** 本人提交的后台作业，外壳的作业入口和页面跟踪作业都用它 */
export const jobsApi = defineModuleApi('operations', ({ query, command }) => ({
  myJobs: query('/api/v1/operations/my-jobs'),
  myJob: query('/api/v1/operations/my-jobs/{module}/{id}'),
  cancelMyJob: command('POST', '/api/v1/operations/my-jobs/{module}/{id}/cancel'),
}));

const activeStatuses: ReadonlySet<BackgroundJobStatus> = new Set(['Queued', 'Running', 'Cancelling']);

/** 还没结束 */
export function isJobActive(job: Pick<BackgroundJob, 'status'>): boolean {
  return activeStatuses.has(job.status);
}

/** 命令返回 202 时的响应带 jobId */
export function isJobAccepted(data: unknown): data is { jobId: string } {
  return typeof data === 'object' && data !== null && typeof (data as { jobId?: unknown }).jobId === 'string';
}
