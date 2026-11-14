import { describe, expect, it } from 'vitest';
import { KmicError } from './kmicError';

const problem = {
  type: 'urn:kmic:error:Common.Conflict',
  title: '数据已被修改',
  status: 409,
  code: 'Common.Conflict',
  traceId: 'trace-9',
  retryable: false,
  currentVersion: 4,
};

describe('KmicError', () => {
  it('按 ProblemDetails 的字段转换，可选字段缺失时用默认值', () => {
    const error = KmicError.fromProblem(problem, undefined, 'failed');

    expect([error.message, error.status, error.code, error.traceId, error.currentVersion, error.outcome]).toEqual([
      '数据已被修改',
      409,
      'Common.Conflict',
      'trace-9',
      4,
      'failed',
    ]);
    expect([error.errors, error.retryable, error.retryAfterSeconds]).toEqual([[], false, undefined]);
  });

  it('不是 ProblemDetails 时按状态码归类，5xx 提示服务暂时不可用', () => {
    const gateway = KmicError.fromStatus(502, 'gw', undefined);
    const other = KmicError.fromStatus(404, undefined, undefined);

    expect([gateway.message, gateway.traceId]).toEqual(['服务暂时不可用，请稍后再试', 'gw']);
    expect([other.message, other.traceId]).toEqual(['请求没有成功', undefined]);
  });
});
