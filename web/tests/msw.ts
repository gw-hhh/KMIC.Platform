import { setupWorker } from 'msw/browser';

export const worker = setupWorker();

export async function startWorker(): Promise<void> {
  await worker.start({ quiet: true, onUnhandledRequest: 'error' });
}
