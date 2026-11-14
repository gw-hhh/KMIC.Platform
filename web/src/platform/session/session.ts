/**
 * 会话：访问令牌和会话纪元
 *
 * 访问令牌只在内存里，刷新页面后经刷新 Cookie 重新取得。纪元把“用户 + 当前角色 + 模拟会话”合起来看，
 * 每次变化加一：发出请求时记下纪元，响应回来纪元变了就丢弃，免得上一个身份的结果落到下一个身份的界面上
 */
export interface Session {
  /** 当前的访问令牌，没有登录时为空 */
  readonly token: string | null;
  /** 会话纪元，身份变化时加一 */
  readonly epoch: number;
  setToken(token: string): void;
  /** 身份变了（换用户、切换角色、开始或结束模拟）：纪元加一 */
  changeIdentity(): void;
  /** 登录已经失效：清掉令牌，纪元加一，并通知监听者弹出登录 */
  expire(): void;
  /** 监听登录失效，返回取消监听的函数 */
  onExpired(listener: () => void): () => void;
}

export function createSession(): Session {
  let token: string | null = null;
  let epoch = 0;
  const listeners = new Set<() => void>();

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
      epoch++;
    },
    expire() {
      token = null;
      epoch++;
      for (const listener of [...listeners]) {
        listener();
      }
    },
    onExpired(listener) {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
  };
}
