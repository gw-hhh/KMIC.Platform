import type { components } from '@/generated/api';

type Problem = components['schemas']['KmicProblem'];

export type KmicErrorKind = 'problem' | 'network' | 'timeout' | 'aborted' | 'client';

export type KmicOutcome = 'failed' | 'unknown';

export interface KmicFieldError {
  path: string;
  code: string;
  message: string;
}

export interface KmicErrorInit {
  kind: KmicErrorKind;
  message: string;
  status?: number;
  code?: string;
  detail?: string;
  errors?: readonly KmicFieldError[];
  retryable?: boolean;
  currentVersion?: number;
  traceId?: string;
  retryAfterSeconds?: number;
  outcome?: KmicOutcome;
  cause?: unknown;
}

/** 平台统一错误类型；业务逻辑应依据 code 判断，而非匹配错误文案 */
export class KmicError extends Error {
  readonly kind: KmicErrorKind;
  readonly status: number | undefined;
  readonly code: string | undefined;
  readonly detail: string | undefined;
  readonly errors: readonly KmicFieldError[];
  readonly retryable: boolean;
  readonly currentVersion: number | undefined;
  readonly traceId: string | undefined;
  readonly retryAfterSeconds: number | undefined;
  readonly outcome: KmicOutcome | undefined;

  constructor(init: KmicErrorInit) {
    super(init.message, { cause: init.cause });
    this.name = 'KmicError';
    this.kind = init.kind;
    this.status = init.status;
    this.code = init.code;
    this.detail = init.detail;
    this.errors = init.errors ?? [];
    this.retryable = init.retryable ?? false;
    this.currentVersion = init.currentVersion;
    this.traceId = init.traceId;
    this.retryAfterSeconds = init.retryAfterSeconds;
    this.outcome = init.outcome;
  }

  static fromProblem(
    problem: Problem,
    retryAfterSeconds: number | undefined,
    outcome: KmicOutcome | undefined,
  ): KmicError {
    return new KmicError({
      kind: 'problem',
      message: problem.title,
      status: problem.status,
      code: problem.code,
      ...(problem.detail === undefined ? {} : { detail: problem.detail }),
      ...(problem.errors === undefined ? {} : { errors: problem.errors }),
      retryable: problem.retryable,
      ...(problem.currentVersion === undefined ? {} : { currentVersion: problem.currentVersion }),
      traceId: problem.traceId,
      ...(retryAfterSeconds === undefined ? {} : { retryAfterSeconds }),
      ...(outcome === undefined ? {} : { outcome }),
    });
  }

  static fromStatus(status: number, traceId: string | undefined, outcome: KmicOutcome | undefined): KmicError {
    return new KmicError({
      kind: 'problem',
      message: status >= 500 ? '服务暂时不可用，请稍后再试' : '请求失败',
      status,
      ...(traceId === undefined ? {} : { traceId }),
      ...(outcome === undefined ? {} : { outcome }),
    });
  }
}
