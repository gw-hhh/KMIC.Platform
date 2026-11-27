import { describe, expect, it } from 'vitest';
import { createSession } from './session';

describe('会话', () => {
  it('初始状态下无令牌且 epoch 为 0', () => {
    const session = createSession();

    expect([session.token, session.epoch]).toEqual([null, 0]);
  });

  it('身份变更时递增 epoch 并保留当前令牌', () => {
    const session = createSession();
    session.setToken('t');

    session.changeIdentity();

    expect([session.token, session.epoch]).toEqual(['t', 1]);
  });

  it('会话失效时清除令牌、递增 epoch 并通知监听者', () => {
    const session = createSession();
    session.setToken('t');
    let called = 0;
    session.onExpired(() => {
      called++;
    });

    session.expire();

    expect([session.token, session.epoch, called]).toEqual([null, 1, 1]);
  });

  it('身份变更与会话失效均触发 epoch 变更监听', () => {
    const session = createSession();
    const seen: number[] = [];
    session.onEpochChanged(() => {
      seen.push(session.epoch);
    });

    session.changeIdentity();
    session.expire();

    expect(seen).toEqual([1, 2]);
  });

  it('取消监听后不再通知', () => {
    const session = createSession();
    let called = 0;
    const stop = session.onExpired(() => {
      called++;
    });

    stop();
    session.expire();

    expect(called).toBe(0);
  });
});
