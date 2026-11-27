import { computed, readonly, ref, type ComputedRef, type Ref } from 'vue';
import type { ApiClient } from './client';
import type { CommandDef } from './define-module-api';
import { KmicError } from './kmicError';

export type CommandState = 'idle' | 'saving' | 'succeeded' | 'failed' | 'unknown';

export type CommandResult<Data> = { ok: true; data: Data } | { ok: false; error: KmicError };

export interface CommandHandle<Input, Data> {
  readonly state: Readonly<Ref<CommandState>>;
  readonly error: Readonly<Ref<KmicError | null>>;
  /** 处于结果未知状态，可使用原幂等键与冻结的请求内容重发确认 */
  readonly canConfirm: ComputedRef<boolean>;
  /** 发起提交；若正处于保存中则返回当前在途请求的 Promise */
  run(input: Input): Promise<CommandResult<Data>>;
  /** 结果未知时以原幂等键重发确认；若服务端已处理，将重放首次结果 */
  confirm(): Promise<CommandResult<Data>>;
  /** 用户核对结果后放弃本次提交，重置为初始状态 */
  reset(): void;
}

export interface CommandRunnerOptions<Input, Data> {
  def: CommandDef<Input, Data>;
  api: ApiClient;
  /** 提交成功后的回调，用于使关联查询缓存失效 */
  onSucceeded?: (data: Data) => void;
  newId?: () => string;
  sleep?: (ms: number) => Promise<void>;
}

/** 同幂等键请求仍在服务端处理中（409 Common.RequestInProgress）时的重试退避间隔（毫秒） */
export const inProgressDelays = [1000, 2000, 4000, 4000, 4000] as const;

const unconfirmed = (): KmicError =>
  new KmicError({ kind: 'client', message: '上次提交结果待确认，请先重试确认或核对后再提交' });

const nothingToConfirm = (): KmicError => new KmicError({ kind: 'client', message: '没有需要确认的提交' });

interface Intent<Input> {
  /** 首次发送时深拷贝冻结，避免后续表单修改影响重试载荷 */
  input: Input;
  idempotencyKey: string | undefined;
}

/** 管理单次提交意图的生命周期：幂等键、载荷冻结与结果未知后的重发确认 */
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
          // 未启用幂等键时无法安全重发，仅标记为 unknown 等待用户核对
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

// 深拷贝并剥离 Vue 响应式代理
function freeze<T>(input: T): T {
  return input === undefined ? input : (JSON.parse(JSON.stringify(input)) as T);
}
