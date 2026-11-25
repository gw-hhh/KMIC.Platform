/**
 * 访问令牌和 epoch。令牌只放内存；身份变化时 epoch 加一，用来丢弃过期的响应
 */
export interface Session {
  readonly token: string | null;
  readonly epoch: number;
  setToken(token: string): void;
  changeIdentity(): void;
  expire(): void;
  onExpired(listener: () => void): () => void;
  /** 登录、退出、切换角色、登录失效都会触发 */
  onEpochChanged(listener: () => void): () => void;
}

export function createSession(): Session {
  let token: string | null = null;
  let epoch = 0;
  const expiredListeners = new Set<() => void>();
  const epochListeners = new Set<() => void>();

  function nextEpoch(): void {
    epoch++;
    for (const listener of [...epochListeners]) {
      listener();
    }
  }

  return {
    get token() {
      return token;
    },
    get epoch() {
      return epoch;
    },
    setToken(value) {
      token = value;
    },
    changeIdentity() {
      nextEpoch();
    },
    expire() {
      token = null;
      nextEpoch();
      for (const listener of [...expiredListeners]) {
        listener();
      }
    },
    onExpired(listener) {
      expiredListeners.add(listener);
      return () => expiredListeners.delete(listener);
    },
    onEpochChanged(listener) {
      epochListeners.add(listener);
      return () => epochListeners.delete(listener);
    },
  };
}
