import { describe, expect, it } from 'vitest';
import type { ApiClient } from './client';
import { createCommandRunner, inProgressDelays } from './command-runner';
import type { CommandDef } from './define-module-api';
import { KmicError } from './kmicError';

interface Sent {
  input: { name: string };
  key: string | undefined;
}

type Reply = { data: string } | { error: KmicError };

function fakeDef(idempotent: boolean, replies: Reply[]): CommandDef<{ name: string }, string> & { sent: Sent[] } {
  const sent: Sent[] = [];
  return {
    module: 'test',
    method: 'POST',
    path: '/api/v1/test/things',
    idempotent,
    sent,
    send(_api, input, options) {
      sent.push({ input: structuredClone(input), key: options?.idempotencyKey });
      const reply = replies.shift();
      if (reply === undefined) {
        throw new Error('没有准备回复');
      }
      return 'error' in reply ? Promise.reject(reply.error) : Promise.resolve(reply.data);
    },
  };
}

const api = {} as ApiClient;
const problem = (code: string, status = 409) =>
  new KmicError({ kind: 'problem', message: code, status, code, outcome: 'failed' });
const unknownOutcome = () => new KmicError({ kind: 'timeout', message: '请求超时', outcome: 'unknown' });

function setup(idempotent: boolean, replies: Reply[]) {
  const def = fakeDef(idempotent, replies);
  let id = 0;
  const waits: number[] = [];
  const succeeded: string[] = [];
  const handle = createCommandRunner({
    def,
    api,
    newId: () => `key-${String(++id)}`,
    sleep: (ms) => {
      waits.push(ms);
      return Promise.resolve();
    },
    onSucceeded: (data) => succeeded.push(data),
  });
  return { def, handle, waits, succeeded };
}

describe('写操作', () => {
  it('成功后状态为已成功，通知失效；每次提交用新的幂等键', async () => {
    const { def, handle, succeeded } = setup(true, [{ data: 'a' }, { data: 'b' }]);

    const first = await handle.run({ name: '甲' });
    await handle.run({ name: '乙' });

    expect(first).toEqual({ ok: true, data: 'a' });
    expect(handle.state.value).toBe('succeeded');
    expect(succeeded).toEqual(['a', 'b']);
    expect(def.sent.map((s) => s.key)).toEqual(['key-1', 'key-2']);
  });

  it('没标幂等的操作不带幂等键', async () => {
    const { def, handle } = setup(false, [{ data: 'a' }]);

    await handle.run({ name: '甲' });

    expect(def.sent[0]?.key).toBeUndefined();
  });

  it('保存中重复提交只发一次', async () => {
    const { def, handle } = setup(true, [{ data: 'a' }]);

    const [a, b] = await Promise.all([handle.run({ name: '甲' }), handle.run({ name: '乙' })]);

    expect(a).toBe(b);
    expect(def.sent).toHaveLength(1);
  });

  it('同一个键还在处理时按退避用原键重试', async () => {
    const busy = { error: problem('Common.RequestInProgress') };
    const { def, handle, waits } = setup(true, [busy, busy, { data: 'a' }]);

    const result = await handle.run({ name: '甲' });

    expect(result.ok).toBe(true);
    expect(waits).toEqual(inProgressDelays.slice(0, 2));
    expect(new Set(def.sent.map((s) => s.key))).toEqual(new Set(['key-1']));
  });

  it('一直处理中，重试次数用完后算失败', async () => {
    const busy = { error: problem('Common.RequestInProgress') };
    const { def, handle } = setup(
      true,
      Array.from({ length: inProgressDelays.length + 1 }, () => busy),
    );

    const result = await handle.run({ name: '甲' });

    expect(result.ok).toBe(false);
    expect(handle.state.value).toBe('failed');
    expect(def.sent).toHaveLength(inProgressDelays.length + 1);
  });

  it('确定失败时保留错误，下次提交是新的意图', async () => {
    const { def, handle } = setup(true, [{ error: problem('Common.VersionConflict') }, { data: 'a' }]);

    await handle.run({ name: '甲' });
    expect(handle.state.value).toBe('failed');
    expect(handle.error.value?.code).toBe('Common.VersionConflict');
    expect(handle.canConfirm.value).toBe(false);

    await handle.run({ name: '乙' });
    expect(def.sent.map((s) => s.key)).toEqual(['key-1', 'key-2']);
  });

  it('结果未知时用原键、首次的内容确认，期间不能发起新的提交', async () => {
    const { def, handle } = setup(true, [{ error: unknownOutcome() }, { data: 'a' }]);
    const input = { name: '甲' };

    await handle.run(input);
    input.name = '改过的';

    expect(handle.state.value).toBe('unknown');
    expect(handle.canConfirm.value).toBe(true);
    const blocked = await handle.run({ name: '乙' });
    expect(blocked.ok).toBe(false);
    expect(def.sent).toHaveLength(1);

    const confirmed = await handle.confirm();
    expect(confirmed).toEqual({ ok: true, data: 'a' });
    expect(def.sent[1]).toEqual({ input: { name: '甲' }, key: 'key-1' });
  });

  it('没标幂等的操作结果未知时不能确认，核对后重置才能再提交', async () => {
    const { def, handle } = setup(false, [{ error: unknownOutcome() }, { data: 'a' }]);

    await handle.run({ name: '甲' });
    expect(handle.state.value).toBe('unknown');
    expect(handle.canConfirm.value).toBe(false);
    expect((await handle.confirm()).ok).toBe(false);

    handle.reset();
    await handle.run({ name: '甲' });
    expect(handle.state.value).toBe('succeeded');
    expect(def.sent).toHaveLength(2);
  });

  it('不是 KmicError 的异常是程序缺陷，照常抛出', async () => {
    const def = fakeDef(true, []);
    const handle = createCommandRunner({ def, api });

    await expect(handle.run({ name: '甲' })).rejects.toThrow('没有准备回复');
    expect(handle.state.value).toBe('failed');
  });
});
