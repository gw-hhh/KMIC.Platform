<script setup lang="ts">
// 当前用户的后台作业列表：存在进行中作业时每 2 秒轮询，页面不可见时暂停
import { useQuery } from '@tanstack/vue-query';
import { Badge, Popconfirm, Popover, Progress, Spin, Tag, message } from 'antdv-next';
import { computed, ref, watch } from 'vue';
import { isJobActive, jobsApi, useCommand, type BackgroundJob, type BackgroundJobStatus } from '@/platform/api';
import { enumLabel, useFormat } from '@/platform/format';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';
import KTooltip from '@/platform/ui/KTooltip.vue';

const t = shellTexts.jobs;
const format = useFormat();
const open = ref(false);
const pollInterval = 2000;

const jobs = useQuery(
  computed(() => ({
    ...jobsApi.myJobs.options({ params: { query: { Limit: 10 } } }),
    refetchInterval: (query: { state: { data?: { items: readonly BackgroundJob[] } } }) =>
      query.state.data?.items.some(isJobActive) === true ? pollInterval : false,
  })),
);

const items = computed<readonly BackgroundJob[]>(() => jobs.data.value?.items ?? []);
const activeCount = computed(() => items.value.filter(isJobActive).length);

watch(open, (isOpen) => {
  if (isOpen) {
    void jobs.refetch();
  }
});

const tones: Record<BackgroundJobStatus, string> = {
  Queued: 'default',
  Running: 'processing',
  Succeeded: 'success',
  Failed: 'error',
  Cancelling: 'warning',
  Cancelled: 'default',
};

const cancellable = (job: BackgroundJob): boolean => job.status === 'Queued' || job.status === 'Running';

function percent(job: BackgroundJob): number | null {
  return job.progressTotal === null || job.progressTotal <= 0
    ? null
    : Math.min(100, Math.floor((job.progressDone / job.progressTotal) * 100));
}

const cancel = useCommand(jobsApi.cancelMyJob, { invalidates: [jobsApi.myJobs.all] });

async function cancelJob(job: BackgroundJob): Promise<void> {
  const result = await cancel.run({ params: { path: { module: job.module, id: job.id } } });
  if (result.ok) {
    message.success(t.cancelRequested);
  } else {
    // 即使结果未知也无需确认，刷新列表即可获取作业实际状态
    cancel.reset();
    message.error(result.error.message);
    void jobs.refetch();
  }
}
</script>

<template>
  <Popover v-model:open="open" trigger="click" placement="bottomRight" :arrow="false" :classes="{ root: 'kmic-jobs' }">
    <KTooltip :title="t.button" :disabled="open">
      <button class="jobs-button" :aria-label="t.button">
        <Badge :count="activeCount" size="small">
          <KIcon name="todo" :size="16" />
        </Badge>
      </button>
    </KTooltip>
    <template #content>
      <div class="jobs">
        <div class="jobs-header">{{ t.title }}</div>
        <ul v-if="items.length > 0" class="jobs-list">
          <li v-for="job in items" :key="`${job.module}/${job.id}`" class="job">
            <div class="job-head">
              <span class="job-name">{{ job.jobName }}</span>
              <Tag :color="tones[job.status]" :bordered="false">{{ enumLabel('BackgroundJobStatus', job.status) }}</Tag>
            </div>
            <template v-if="job.status === 'Running'">
              <Progress v-if="percent(job) !== null" :percent="percent(job) ?? 0" size="small" />
              <p v-else class="job-meta">{{ t.processed.replace('{done}', String(job.progressDone)) }}</p>
            </template>
            <p v-if="job.progressMessage && isJobActive(job)" class="job-meta">{{ job.progressMessage }}</p>
            <p v-if="job.status === 'Failed' && job.errorMessage" class="job-error">{{ job.errorMessage }}</p>
            <div class="job-foot">
              <span class="job-meta">{{ format.dateTime(job.submittedAt) }}</span>
              <Popconfirm v-if="cancellable(job)" :title="t.cancelConfirm" placement="left" @confirm="cancelJob(job)">
                <button class="job-cancel" :disabled="cancel.state.value === 'saving'">{{ t.cancel }}</button>
              </Popconfirm>
            </div>
          </li>
        </ul>
        <div v-else class="jobs-empty">
          <Spin v-if="jobs.isPending.value" size="small" />
          <template v-else>{{ t.empty }}</template>
        </div>
      </div>
    </template>
  </Popover>
</template>

<style scoped lang="scss">
.jobs-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--kmic-header-icon-color);
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: 50%;
  transition:
    background var(--kmic-motion-fast),
    color var(--kmic-motion-fast);

  &:hover {
    color: var(--kmic-header-foreground);
    background: var(--kmic-header-accent);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
  }

  :deep(.ant-badge) {
    color: inherit;
  }
}

.jobs {
  width: 360px;
  max-width: calc(100vw - 32px);
}

.jobs-header {
  padding: 12px 16px;
}

.jobs-list {
  max-height: 400px;
  padding: 0;
  margin: 0;
  overflow-y: auto;
  list-style: none;
}

.job {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 12px 16px;
  border-top: 1px solid var(--kmic-color-border);

  p {
    margin: 0;
  }

  :deep(.ant-progress) {
    margin: 0;
  }
}

.job-head,
.job-foot {
  display: flex;
  gap: 8px;
  align-items: center;
  justify-content: space-between;
}

.job-name {
  overflow: hidden;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.job-head :deep(.ant-tag) {
  margin: 0;
}

.job-meta {
  font-size: 12px;
  color: var(--kmic-color-muted-foreground);
}

.job-error {
  font-size: 12px;
  color: var(--kmic-color-destructive);
}

.job-cancel {
  padding: 0;
  font: inherit;
  font-size: 12px;
  color: var(--kmic-color-primary);
  cursor: pointer;
  background: none;
  border: 0;

  &:disabled {
    color: var(--kmic-color-muted-foreground);
    cursor: not-allowed;
  }
}

.jobs-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 150px;
  color: var(--kmic-color-muted-foreground);
  border-top: 1px solid var(--kmic-color-border);
}
</style>

<style lang="scss">
.kmic-jobs .ant-popover-container {
  padding: 0;
}
</style>
