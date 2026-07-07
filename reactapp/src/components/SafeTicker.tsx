import './safeTicker.css';

export function SafeTicker({ value }: { value: number | null }) {
  const digits = value === null ? '------' : Math.round(value).toString().padStart(6, '0');

  return (
    <div className="safe-ticker" title="Текущая сумма в сейфе">
      <span className="safe-ticker-label">СЕЙФ</span>
      <span className="safe-ticker-display tabular">
        {digits.split('').map((d, i) => (
          <span key={i} className="safe-ticker-digit">
            {d}
          </span>
        ))}
        <span className="safe-ticker-currency">₽</span>
      </span>
    </div>
  );
}
