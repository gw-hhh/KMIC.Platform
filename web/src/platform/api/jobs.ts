import type { components } from '@/generated/api';
import { defineModuleApi } from './define-module-api';

export type BackgroundJob = components['schemas']['BackgroundJobItem'];
export type BackgroundJobStatus = components['schemas']['BackgroundJobStatus'];

/** 当前用户提交的后台作业接口，供外壳作业面板与页面作业跟踪共用 */
export const jobsApi = defineModuleApi('operations', ({ query, command }) => ({
  myJobs: query('/api/v1/operations/my-jobs'),
  myJob: query('/api/v1/operations/my-jobs/{module}/{id}'),
  cancelMyJob: command('POST', '/api/v1/operations/my-jobs/{module}/{id}/cancel'),
}));

const activeStatuses: ReadonlySet<BackgroundJobStatus> = new Set(['Queued', 'Running', 'Cancelling']);

export function isJobActive(job: Pick<BackgroundJob, 'status'>): boolean {
  return activeStatuses.has(job.status);
}

/** 校验命令返回 202 Accepted 时是否携带 jobId */
export function isJobAccepted(data: unknown): data is { jobId: string } {
  return typeof data === 'object' && data !== null && typeof (data as { jobId?: unknown }).jobId === 'string';
}
