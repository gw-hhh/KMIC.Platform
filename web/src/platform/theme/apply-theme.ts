import { buildCssVariables, type Appearance, type ThemeRuntime } from './appearance';

export function applyTheme(root: HTMLElement, appearance: Appearance, runtime: ThemeRuntime): void {
  for (const [name, value] of Object.entries(buildCssVariables(appearance, runtime))) {
    root.style.setProperty(name, value);
  }
  root.dataset['theme'] = runtime.resolvedMode;
  root.dataset['colorWeak'] = String(appearance.colorWeak);
  root.style.colorScheme = runtime.resolvedMode;
}

/** 以点击坐标为圆心，通过圆形扩散动画切换深浅主题 */
export async function switchWithRipple(
  origin: { x: number; y: number },
  change: () => Promise<void> | void,
  options: { reducedMotion: boolean; durationMs: number },
): Promise<void> {
  if (!('startViewTransition' in document) || options.reducedMotion) {
    await change();
    return;
  }

  const transition = document.startViewTransition(async () => {
    await change();
  });
  await transition.ready;

  const point = `${String(origin.x)}px ${String(origin.y)}px`;
  const from = `circle(0 at ${point})`;
  const radius = Math.hypot(Math.max(origin.x, innerWidth - origin.x), Math.max(origin.y, innerHeight - origin.y));
  document.documentElement.animate(
    { clipPath: [from, `circle(${String(radius)}px at ${point})`] },
    { duration: options.durationMs, easing: 'ease-in', pseudoElement: '::view-transition-new(root)' },
  );
  await transition.finished;
}
