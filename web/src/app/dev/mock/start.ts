import { setupWorker } from 'msw/browser';
import { handlers } from './handlers';

export async function startMock(): Promise<void> {
  await setupWorker(...handlers).start({ onUnhandledFrame: 'bypass', quiet: true });
}
