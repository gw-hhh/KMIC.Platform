import { http, HttpResponse } from 'msw';
import type { components } from '@/generated/api';

// 本地 Mock 后台作业与站内消息，按账号隔离存储于 sessionStorage
// 首次访问时预置示例数据：运行中导出（40 秒后完成并推送站内消息）、失败导入与已完成作业

type Job = components['schemas']['BackgroundJobItem'];
type Message = components['schemas']['SiteMessageItem'];

interface Inbox {
  jobs: Job[];
  messages: Message[];
}

const runSeconds = 40;
const total = 200;

const keyOf = (account: string): string => `kmic.mock.inbox.${account}`;

function accountOf(request: Request): string | null {
  const match = /^Bearer mock\.(.+)$/.exec(request.headers.get('Authorization') ?? '');
  return match?.[1] ?? null;
}

const minutesAgo = (minutes: number): string => new Date(Date.now() - minutes * 60_000).toISOString();

function job(fields: Partial<Job> & Pick<Job, 'id' | 'jobName' | 'status' | 'submittedAt'>): Job {
  return {
    module: 'Sample',
    jobType: 'Sample.Demo',
    submittedBy: null,
    submittedByName: null,
    startedAt: fields.submittedAt,
    finishedAt: null,
    progressDone: 0,
    progressTotal: null,
    progressMessage: null,
    resultFileId: null,
    resultSummary: null,
    errorCode: null,
    errorMessage: null,
    attempts: 1,
    cancelRequestedAt: null,
    cancelRequestedBy: null,
    cancelRequestedByName: null,
    ...fields,
  };
}

function siteMessage(fields: Pick<Message, 'id' | 'title' | 'body' | 'sentAt'> & Partial<Message>): Message {
  return { link: null, category: 'Sample', isRead: false, readAt: null, ...fields };
}

function seed(): Inbox {
  return {
    jobs: [
      job({
        id: 'job-export',
        jobName: '导出示例单',
        status: 'Running',
        submittedAt: new Date().toISOString(),
        progressTotal: total,
      }),
      job({
        id: 'job-import',
        jobName: '导入示例单',
        status: 'Failed',
        submittedAt: minutesAgo(30),
        finishedAt: minutesAgo(29),
        errorCode: 'Sample.ImportInvalid',
        errorMessage: '第 12 行：客户编码 C-0999 不存在',
      }),
      job({
        id: 'job-report',
        jobName: '重算示例报表',
        status: 'Succeeded',
        submittedAt: minutesAgo(120),
        finishedAt: minutesAgo(118),
      }),
    ],
    messages: [
      siteMessage({
        id: 'msg-submitted',
        title: '示例单 SP-001 已提交',
        body: '李销售提交了示例单 SP-001，请审核。',
        sentAt: minutesAgo(5),
        link: '/demo/items/SP-001',
      }),
      siteMessage({
        id: 'msg-password',
        title: '密码将在 7 天后过期',
        body: '请在过期前及时修改，过期后需联系管理员重置。',
        sentAt: minutesAgo(60),
      }),
      siteMessage({
        id: 'msg-welcome',
        title: '欢迎使用 KMIC',
        body: '这是一条已读的示例消息。',
        sentAt: minutesAgo(60 * 24),
        isRead: true,
        readAt: minutesAgo(60 * 23),
      }),
    ],
  };
}

// 按已耗时推进运行中作业进度，完成后向提交人发送站内消息（与真实后端行为一致）
function advance(inbox: Inbox): void {
  for (const item of inbox.jobs) {
    if (item.status !== 'Running' || item.startedAt === null) {
      continue;
    }
    const elapsed = (Date.now() - Date.parse(item.startedAt)) / 1000;
    item.progressDone = Math.min(total, Math.floor((elapsed / runSeconds) * total));
    item.progressMessage = `正在导出第 ${String(item.progressDone)} 行`;
    if (elapsed >= runSeconds) {
      item.status = 'Succeeded';
      item.finishedAt = new Date().toISOString();
      item.progressMessage = null;
      inbox.messages.unshift(
        siteMessage({
          id: `msg-${item.id}`,
          title: '后台作业已完成',
          body: `${item.jobName}：已完成`,
          sentAt: item.finishedAt,
        }),
      );
    }
  }
}

function load(account: string): Inbox {
  const text = sessionStorage.getItem(keyOf(account));
  const inbox = text === null ? seed() : (JSON.parse(text) as Inbox);
  advance(inbox);
  save(account, inbox);
  return inbox;
}

function save(account: string, inbox: Inbox): void {
  sessionStorage.setItem(keyOf(account), JSON.stringify(inbox));
}

const unauthorized = (): Response =>
  HttpResponse.json(
    {
      type: 'urn:kmic:error:Common.Unauthenticated',
      title: '请先登录',
      status: 401,
      code: 'Common.Unauthenticated',
      traceId: 'mock-trace',
      retryable: false,
    },
    { status: 401, headers: { 'Content-Type': 'application/problem+json' } },
  );

const notFound = (): Response =>
  HttpResponse.json(
    {
      type: 'urn:kmic:error:Common.NotFound',
      title: '记录不存在',
      status: 404,
      code: 'Common.NotFound',
      traceId: 'mock-trace',
      retryable: false,
    },
    { status: 404, headers: { 'Content-Type': 'application/problem+json' } },
  );

// 未读优先，同状态按发送时间倒序
const ordered = (messages: Message[]): Message[] =>
  [...messages].sort((a, b) => Number(a.isRead) - Number(b.isRead) || b.sentAt.localeCompare(a.sentAt));

export const inboxHandlers = [
  http.get('/api/v1/operations/my-jobs', ({ request }) => {
    const account = accountOf(request);
    return account === null ? unauthorized() : HttpResponse.json({ items: load(account).jobs, nextCursor: null });
  }),

  http.post('/api/v1/operations/my-jobs/:module/:id/cancel', ({ request, params }) => {
    const account = accountOf(request);
    if (account === null) {
      return unauthorized();
    }
    const inbox = load(account);
    const target = inbox.jobs.find((item) => item.id === params['id']);
    if (target === undefined) {
      return notFound();
    }
    if (target.status === 'Running' || target.status === 'Queued') {
      target.status = 'Cancelled';
      target.finishedAt = new Date().toISOString();
      target.cancelRequestedAt = target.finishedAt;
      target.progressMessage = null;
      save(account, inbox);
    }
    return new HttpResponse(null, { status: 204 });
  }),

  http.get('/api/v1/notifications/site-messages', ({ request }) => {
    const account = accountOf(request);
    if (account === null) {
      return unauthorized();
    }
    const pageSize = Number(new URL(request.url).searchParams.get('PageSize') ?? '20');
    const messages = ordered(load(account).messages);
    return HttpResponse.json({
      items: messages.slice(0, pageSize),
      page: 1,
      pageSize,
      totalCount: messages.length,
      totalCountExact: true,
    });
  }),

  http.get('/api/v1/notifications/site-messages/unread-count', ({ request }) => {
    const account = accountOf(request);
    return account === null
      ? unauthorized()
      : HttpResponse.json({ count: load(account).messages.filter((item) => !item.isRead).length });
  }),

  http.post('/api/v1/notifications/site-messages/read-all', ({ request }) => {
    const account = accountOf(request);
    if (account === null) {
      return unauthorized();
    }
    const inbox = load(account);
    const unread = inbox.messages.filter((item) => !item.isRead);
    const now = new Date().toISOString();
    for (const item of unread) {
      item.isRead = true;
      item.readAt = now;
    }
    save(account, inbox);
    return HttpResponse.json({ count: unread.length });
  }),

  http.post('/api/v1/notifications/site-messages/:id/read', ({ request, params }) => {
    const account = accountOf(request);
    if (account === null) {
      return unauthorized();
    }
    const inbox = load(account);
    const target = inbox.messages.find((item) => item.id === params['id']);
    if (target === undefined) {
      return notFound();
    }
    if (!target.isRead) {
      target.isRead = true;
      target.readAt = new Date().toISOString();
      save(account, inbox);
    }
    return new HttpResponse(null, { status: 204 });
  }),
];
