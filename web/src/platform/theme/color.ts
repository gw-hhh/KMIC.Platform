export interface Hsl {
  h: number;
  s: number;
  l: number;
}

export function formatHsl({ h, s, l }: Hsl): string {
  return `hsl(${String(h)} ${String(s)}% ${String(l)}%)`;
}

function toRgb({ h, s, l }: Hsl): [number, number, number] {
  const sat = s / 100;
  const light = l / 100;
  const amount = sat * Math.min(light, 1 - light);
  const channel = (n: number): number => {
    const k = (n + h / 30) % 12;
    return light - amount * Math.max(-1, Math.min(k - 3, Math.min(9 - k, 1)));
  };
  return [channel(0), channel(8), channel(4)];
}

function luminance(color: Hsl): number {
  const [r, g, b] = toRgb(color).map((v) => (v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4)) as [
    number,
    number,
    number,
  ];
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

/** WCAG 对比度，文字至少要 4.5 */
export function contrastRatio(a: Hsl, b: Hsl): number {
  const [light, dark] = [luminance(a), luminance(b)].sort((x, y) => y - x) as [number, number];
  return (light + 0.05) / (dark + 0.05);
}
