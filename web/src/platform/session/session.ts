/** 会话令牌与代际（epoch）管理：令牌仅驻留内存；身份变更时递增 epoch，用于识别并丢弃过期请求的响应 */
export interface Session {
  readonly token: string | null;
  readonly epoch: number;
  setToken(token: string): void;
  changeIdentity(): void;
  expire(): void;
  onExpired(listener: () => void): () => void;
  /** 登录、注销、切换角色及会话失效时均会触发 */
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
