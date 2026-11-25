import { Modal } from 'antdv-next';
import { h } from 'vue';
import { shellTexts } from '@/platform/texts/shell';
import type { LeaveReport } from '@/shell/workspace/workspace';

const t = shellTexts;

function ask(options: {
  title: string;
  content: ReturnType<typeof h> | string;
  ok: string;
  cancel?: string;
}): Promise<boolean> {
  return new Promise((resolve) => {
    Modal.confirm({
      title: options.title,
      content: options.content,
      okText: options.ok,
      okCancel: options.cancel !== undefined,
      cancelText: options.cancel,
      onOk: () => {
        resolve(true);
      },
      onCancel: () => {
        resolve(false);
      },
    });
  });
}

export function confirmLeave(reports: readonly LeaveReport[]): Promise<boolean> {
  const list = h(
    'ul',
    reports.map((report) => h('li', `${report.title}：${t.leave[report.status]}`)),
  );
  // 保存中不能离开，只提示
  if (reports.some((report) => report.status === 'saving')) {
    return ask({ title: t.leave.title, content: list, ok: t.leave.understood }).then(() => false);
  }
  return ask({ title: t.leave.title, content: list, ok: t.leave.discard, cancel: t.leave.cancel });
}

export function confirmReplace(candidate: { key: string; title: string } | null): Promise<boolean> {
  if (candidate === null) {
    return ask({ title: t.replace.title, content: t.replace.limit, ok: t.leave.understood }).then(() => false);
  }
  return ask({
    title: t.replace.limit,
    content: t.replace.ask.replace('{title}', candidate.title),
    ok: t.replace.close,
    cancel: t.leave.cancel,
  });
}
