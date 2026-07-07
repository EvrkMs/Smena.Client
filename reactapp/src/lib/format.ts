export function formatMoney(value: number): string {
  return new Intl.NumberFormat('ru-RU').format(Math.round(value));
}

export function parseIntSafe(value: string, fallback = 0): number {
  const n = parseInt(value, 10);
  return Number.isFinite(n) ? n : fallback;
}
