import { inject, type InjectionKey } from 'vue';
import type { Workspace } from './workspace';

export const workspaceKey: InjectionKey<Workspace> = Symbol('kmic.workspace');

export function useWorkspace(): Workspace {
  const workspace = inject(workspaceKey, null);
  if (workspace === null) {
    throw new Error('workspaceKey 未注册，请在 main.ts 里 provide');
  }
  return workspace;
}
