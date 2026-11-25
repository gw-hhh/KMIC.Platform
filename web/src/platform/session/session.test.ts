import { describe, expect, it } from 'vitest';
import { createSession } from './session';

describe('会话', () => {
  it('一开始没有令牌，epoch 是 0', () => {
    const session = createSession();

    expect([session.token, session.epoch]).toEqual([null, 0]);
  });

  it('身份变化时 epoch 加一，令牌保留', () => {
    const session = createSession();
    session.setToken('t');

    session.changeIdentity();

    expect([session.token, session.epoch]).toEqual(['t', 1]);
  });

  it('登录失效时清掉令牌，epoch 加一，并通知监听者', () => {
    const session = createSession();
    session.setToken('t');
    let called = 0;
    session.onExpired(() => {
      called++;
    });

    session.expire();

    expect([session.token, session.epoch, called]).toEqual([null, 1, 1]);
  });

  it('身份变化和登录失效都通知 epoch 的监听者', () => {
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
