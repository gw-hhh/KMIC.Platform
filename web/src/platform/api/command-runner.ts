import { computed, readonly, ref, type ComputedRef, type Ref } from 'vue';
import type { ApiClient } from './client';
import type { CommandDef } from './define-module-api';
import { KmicError } from './kmicError';

export type CommandState = 'idle' | 'saving' | 'succeeded' | 'failed' | 'unknown';

export type CommandResult<Data> = { ok: true; data: Data } | { ok: false; error: KmicError };

export interface CommandHandle<Input, Data> {
  readonly state: Readonly<Ref<CommandState>>;
  readonly error: Readonly<Ref<KmicError | null>>;
  /** 结果未知，可以用原来的幂等键和内容再发一次确认 */
  readonly canConfirm: ComputedRef<boolean>;
  /** 发起一次提交；保存中重复调用返回同一次的结果 */
  run(input: Input): Promise<CommandResult<Data>>;
  /** 结果未知时用原键原内容重发，已生效的服务端会重放第一次的结果 */
  confirm(): Promise<CommandResult<Data>>;
  /** 用户核对过结果后放弃这次提交 */
  reset(): void;
}

export interface CommandRunnerOptions<Input, Data> {
  def: CommandDef<Input, Data>;
  api: ApiClient;
  /** 成功后调用，用来失效相关查询 */
  onSucceeded?: (data: Data) => void;
  newId?: () => string;
  sleep?: (ms: number) => Promise<void>;
}

/** 同一个键的请求还在处理时（409 Common.RequestInProgress）的重试间隔 */
export const inProgressDelays = [1000, 2000, 4000, 4000, 4000] as const;

const unconfirmed = (): KmicError =>
  new KmicError({ kind: 'client', message: '上次提交的结果还没确认，请先重试确认或核对后再提交' });

const nothingToConfirm = (): KmicError => new KmicError({ kind: 'client', message: '没有需要确认的提交' });

interface Intent<Input> {
  /** 首次发送时冻结，之后修改表单不影响重试的内容 */
  input: Input;
  idempotencyKey: string | undefined;
}

/**
 * 一次提交意图的状态：幂等键、冻结内容、结果未知后的确认
 */
export function createCommandRunner<Input, Data>(
  options: CommandRunnerOptions<Input, Data>,
): CommandHandle<Input, Data> {
  const { def, api } = options;
  const newId = options.newId ?? (() => crypto.randomUUID());
  const sleep = options.sleep ?? ((ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms)));

  const state = ref<CommandState>('idle');
  const error = ref<KmicError | null>(null);
  let intent: Intent<Input> | null = null;
  let inflight: Promise<CommandResult<Data>> | null = null;

  const canConfirm = computed(() => state.value === 'unknown' && intent?.idempotencyKey !== undefined);

  function run(input: Input): Promise<CommandResult<Data>> {
    if (inflight !== null) {
      return inflight;
    }
    if (state.value === 'unknown') {
      return Promise.resolve({ ok: false, error: unconfirmed() });
    }
    intent = { input: freeze(input), idempotencyKey: def.idempotent ? newId() : undefined };
    return execute(intent);
  }

  function confirm(): Promise<CommandResult<Data>> {
    if (inflight !== null) {
      return inflight;
    }
    if (!canConfirm.value || intent === null) {
      return Promise.resolve({ ok: false, error: nothingToConfirm() });
    }
    return execute(intent);
  }

  function reset(): void {
    if (inflight === null) {
      intent = null;
      state.value = 'idle';
      error.value = null;
    }
  }

  function execute(current: Intent<Input>): Promise<CommandResult<Data>> {
    state.value = 'saving';
    error.value = null;
    inflight = attempt(current).finally(() => {
      inflight = null;
    });
    return inflight;
  }

  async function attempt(current: Intent<Input>): Promise<CommandResult<Data>> {
    for (let retry = 0; ; retry++) {
      try {
        const sendOptions = current.idempotencyKey === undefined ? {} : { idempotencyKey: current.idempotencyKey };
        const data = await def.send(api, current.input, sendOptions);
        intent = null;
        state.value = 'succeeded';
        options.onSucceeded?.(data);
        return { ok: true, data };
      } catch (caught) {
        if (!(caught instanceof KmicError)) {
          intent = null;
          state.value = 'failed';
          throw caught;
        }
        const delay = inProgressDelays[retry];
        if (caught.code === 'Common.RequestInProgress' && delay !== undefined) {
          await sleep(delay);
          continue;
        }
        error.value = caught;
        if (caught.outcome === 'unknown') {
          // 没有幂等键的不能安全重发，只能等用户核对
          state.value = 'unknown';
        } else {
          intent = null;
          state.value = 'failed';
        }
        return { ok: false, error: caught };
      }
    }
  }

  return { state: readonly(state), error: readonly(error), canConfirm, run, confirm, reset };
}

// 请求体本来就要序列化成 JSON，按 JSON 复制一份，也去掉了 Vue 的响应式代理
function freeze<T>(input: T): T {
  return input === undefined ? input : (JSON.parse(JSON.stringify(input)) as T);
}
