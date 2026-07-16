import { useMemo, useState } from 'react';
import type { WarehouseItem } from '../bridge/api';
import { useApiEngine } from '../bridge/engine';
import { Button, Panel, TextField } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';

interface Row extends WarehouseItem {
  fact: string;
}

function rowKey(name: string, article: string) {
  return `${name}||${article}`;
}

function formatQty(v: number): string {
  return Number.isInteger(v) ? String(v) : v.toFixed(3).replace(/0+$/, '').replace(/\.$/, '');
}

export default function Stockcount() {
  const api = useApiEngine();
  const toast = useToast();
  const confirm = useConfirm();
  const [query, setQuery] = useState('');
  const [hits, setHits] = useState<WarehouseItem[]>([]);
  const [rows, setRows] = useState<Row[]>([]);
  const [loadingAll, setLoadingAll] = useState(false);
  const [showResult, setShowResult] = useState(false);

  const search = async (q: string) => {
    setQuery(q);
    if (q.trim().length < 2) { setHits([]); return; }
    const found = await api.searchWarehouseItems(q.trim(), 15);
    if (found) setHits(found);
  };

  const addItem = (item: WarehouseItem) => {
    const key = rowKey(item.name, item.article);
    setRows((prev) => {
      if (prev.some((r) => rowKey(r.name, r.article) === key)) return prev;
      return [...prev, { ...item, fact: '' }];
    });
    setQuery(''); setHits([]); setShowResult(false);
  };

  const removeItem = (key: string) => {
    setRows((prev) => prev.filter((r) => rowKey(r.name, r.article) !== key));
  };

  const clearAll = async () => {
    const ok = await confirm({ title: 'Очистка', message: 'Удалить все позиции из списка?' });
    if (ok) {
      setRows([]);
      setShowResult(false);
    }
  };

  const addAllFromStock = async () => {
    const ok = await confirm({ title: 'Добавить из остатков', message: 'Загрузить все позиции?' });
    if (!ok) return;
    setLoadingAll(true);
    const result = await api.getAllWarehouseItems();
    setLoadingAll(false);
    if (!result) return;

    const { items } = result;
    setRows((prev) => {
      const existing = new Set(prev.map((r) => rowKey(r.name, r.article)));
      return [...prev, ...items.filter(i => i.stock > 0 && !existing.has(rowKey(i.name, i.article))).map(i => ({ ...i, fact: '' }))];
    });
  };

  const negatives = useMemo(() => {
    return rows
      .map((r) => {
        const fact = r.fact === '' ? 0 : Number(r.fact.replace(',', '.'));
        const diff = fact - r.stock;
        const pct = r.stock > 0 ? (diff / r.stock) * 100 : 0;
        return { ...r, fact, diff, pct };
      })
      .filter((r) => r.diff < 0)
      .sort((a, b) => a.diff - b.diff);
  }, [rows]);

  return (
    <div className="screen-grid stockcount-grid">
      <div className="stock-left-col" style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
        <Panel title="Поиск позиций">
          <TextField label="Название или артикул" value={query} onChange={(e) => search(e.target.value)} />
          {hits.length > 0 && (
            <ul className="search-hits">
              {hits.map((h) => (
                <li key={rowKey(h.name, h.article)} onClick={() => addItem(h)}>
                  {h.name} <span className="muted">ост: {formatQty(h.stock)}</span>
                </li>
              ))}
            </ul>
          )}
          <Button variant="ghost" disabled={loadingAll} onClick={addAllFromStock}>
            {loadingAll ? 'Загружаю…' : 'Добавить все остатки'}
          </Button>
        </Panel>

        {showResult && (
          <Panel title={negatives.length === 0 ? 'Расхождений нет ✓' : `Расхождения: ${negatives.length}`}>
            {negatives.length > 0 && (
              <>
                <table className="ledger-table">
                  <thead><tr><th>Товар</th><th>Разница</th></tr></thead>
                  <tbody>
                    {negatives.map((r) => (
                      <tr key={rowKey(r.name, r.article)}>
                        <td>{r.name}</td>
                        <td className="tabular value-negative">{formatQty(r.diff)} ({r.pct.toFixed(1)}%)</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <Button variant="ghost" onClick={async () => {
                   await navigator.clipboard.writeText(negatives.map(r => `${r.name}: ${formatQty(r.diff)}`).join('\n'));
                   toast('success', 'Скопировано');
                }} style={{ marginTop: 8 }}>Скопировать</Button>
              </>
            )}
          </Panel>
        )}
      </div>

      <Panel title={`Список (${rows.length})`}>
        <table className="ledger-table">
          <thead><tr><th>Наименование</th><th>МС</th><th>Факт</th><th /></tr></thead>
          <tbody>
            {rows.map((r) => {
              const key = rowKey(r.name, r.article);
              return (
                <tr key={key}>
                  <td>{r.name}</td>
                  <td className="tabular">{formatQty(r.stock)}</td>
                  <td>
                    <input className="field-input tabular table-input" value={r.fact} placeholder="0"
                      onChange={(e) => {
                        const v = e.target.value.replace(/[^0-9.,]/g, '');
                        setRows((prev) => prev.map((row) => (rowKey(row.name, row.article) === key ? { ...row, fact: v } : row)));
                        setShowResult(false);
                      }}
                    />
                  </td>
                  <td><button className="table-remove" onClick={() => removeItem(key)}>×</button></td>
                </tr>
              );
            })}
          </tbody>
        </table>
        <div style={{ display: 'flex', gap: '8px', marginTop: '12px' }}>
          <Button disabled={rows.length === 0} onClick={() => setShowResult(true)}>Рассчитать</Button>
          <Button variant="ghost" disabled={rows.length === 0} onClick={clearAll}>Очистить</Button>
        </div>
      </Panel>
    </div>
  );
}