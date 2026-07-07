import { useMemo, useState } from 'react';
import { getAllWarehouseItems, searchWarehouseItems, type WarehouseItem } from '../bridge/api';
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

/**
 * Эквивалент StockcountUserControl: поиск позиций МойСклад, ввод факта, расчёт минусовых
 * расхождений, копирование результата текстом. Копирование как изображение (RenderResultTable
 * в оригинале — рендер Bitmap для буфера обмена) НЕ перенесено в этом проходе — сознательно,
 * это отдельная задача (canvas + Clipboard API), не входит в текущий объём.
 *
 * Также: оригинал нигде не вызывал GrpcInventoryService.SendInventory несмотря на наличие
 * inventory.proto — здесь по той же причине результат пересчёта только показывается и
 * копируется, никуда не отправляется. Уточните на бэкенде, нужен ли этот RPC вообще.
 */
export default function Stockcount() {
  const toast = useToast();
  const confirm = useConfirm();
  const [query, setQuery] = useState('');
  const [hits, setHits] = useState<WarehouseItem[]>([]);
  const [rows, setRows] = useState<Row[]>([]);
  const [loadingAll, setLoadingAll] = useState(false);
  const [showResult, setShowResult] = useState(false);

  const search = async (q: string) => {
    setQuery(q);
    if (q.trim().length < 2) {
      setHits([]);
      return;
    }
    try {
      setHits(await searchWarehouseItems(q.trim(), 15));
    } catch (e) {
      toast('error', String(e));
    }
  };

  const addItem = (item: WarehouseItem) => {
    const key = rowKey(item.name, item.article);
    setRows((prev) => {
      if (prev.some((r) => rowKey(r.name, r.article) === key)) return prev;
      return [...prev, { ...item, fact: '' }];
    });
    setQuery('');
    setHits([]);
    setShowResult(false);
  };

  const removeItem = (key: string) => {
    setRows((prev) => prev.filter((r) => rowKey(r.name, r.article) !== key));
    setShowResult(false);
  };

  const addAllFromStock = async () => {
    const ok = await confirm({
      title: 'Добавить из остатков МойСклад',
      message: 'Все позиции склада, которых ещё нет в списке, будут добавлены с фактом 0. Продолжить?',
    });
    if (!ok) return;

    setLoadingAll(true);
    try {
      const { items, error } = await getAllWarehouseItems();
      if (error) return toast('error', error);

      setRows((prev) => {
        const existing = new Set(prev.map((r) => rowKey(r.name, r.article)));
        const additions = items
          .filter((i) => !existing.has(rowKey(i.name, i.article)))
          .map((i) => ({ ...i, fact: '0' }));
        return [...prev, ...additions];
      });
      toast('success', 'Остатки загружены.');
    } catch (e) {
      toast('error', String(e));
    } finally {
      setLoadingAll(false);
    }
  };

  const negatives = useMemo(() => {
    return rows
      .filter((r) => r.fact !== '')
      .map((r) => {
        const fact = Number(r.fact.replace(',', '.'));
        const diff = fact - r.stock;
        const pct = r.stock > 0 ? (diff / r.stock) * 100 : 0;
        return { ...r, fact, diff, pct };
      })
      .filter((r) => r.diff < 0)
      .sort((a, b) => a.diff - b.diff);
  }, [rows]);

  const handleCalculate = () => setShowResult(true);

  const handleCopyText = async () => {
    const lines = [
      `Пересчёт склада  ${new Date().toLocaleString('ru-RU')}`,
      '─'.repeat(50),
      ...negatives.map(
        (r) => `${r.name}  →  ${formatQty(r.diff)} (${r.pct.toFixed(1)}%)  (МС: ${formatQty(r.stock)}  Факт: ${formatQty(r.fact)})`,
      ),
    ];
    await navigator.clipboard.writeText(lines.join('\n'));
    toast('success', 'Скопировано в буфер обмена.');
  };

  return (
    <div className="screen-grid stockcount-grid">
      <Panel title="Поиск позиций">
        <TextField
          label="Название или артикул"
          value={query}
          onChange={(e) => search(e.target.value)}
          placeholder="Начните вводить (мин. 2 символа)…"
        />
        {hits.length > 0 && (
          <ul className="search-hits">
            {hits.map((h) => (
              <li key={rowKey(h.name, h.article)} onClick={() => addItem(h)}>
                <span>{h.name}</span>
                <span className="muted tabular">ост: {formatQty(h.stock)}</span>
              </li>
            ))}
          </ul>
        )}
        <Button variant="ghost" disabled={loadingAll} onClick={addAllFromStock}>
          {loadingAll ? 'Загружаю…' : 'Добавить все остатки МойСклад'}
        </Button>
      </Panel>

      <Panel title={`Список (${rows.length})`}>
        {rows.length === 0 ? (
          <p className="muted">Список пуст — найдите позицию слева.</p>
        ) : (
          <table className="ledger-table">
            <thead>
              <tr>
                <th>Наименование</th>
                <th>МС</th>
                <th>Факт</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => {
                const key = rowKey(r.name, r.article);
                return (
                  <tr key={key}>
                    <td>{r.name}</td>
                    <td className="tabular">{formatQty(r.stock)}</td>
                    <td>
                      <input
                        className="field-input tabular table-input"
                        value={r.fact}
                        onChange={(e) => {
                          const v = e.target.value.replace(/[^0-9.,]/g, '');
                          setRows((prev) => prev.map((row) => (rowKey(row.name, row.article) === key ? { ...row, fact: v } : row)));
                          setShowResult(false);
                        }}
                      />
                    </td>
                    <td>
                      <button className="table-remove" onClick={() => removeItem(key)} aria-label="Удалить">
                        ×
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
        <Button disabled={rows.length === 0} onClick={handleCalculate} style={{ marginTop: 12 }}>
          Рассчитать расхождения
        </Button>
      </Panel>

      {showResult && (
        <Panel title={negatives.length === 0 ? 'Расхождений нет ✓' : `Минусовые расхождения: ${negatives.length}`}>
          {negatives.length > 0 && (
            <>
              <table className="ledger-table">
                <thead>
                  <tr>
                    <th>Наименование</th>
                    <th>МС</th>
                    <th>Факт</th>
                    <th>Расхождение</th>
                  </tr>
                </thead>
                <tbody>
                  {negatives.map((r) => (
                    <tr key={rowKey(r.name, r.article)}>
                      <td>{r.name}</td>
                      <td className="tabular">{formatQty(r.stock)}</td>
                      <td className="tabular">{formatQty(r.fact)}</td>
                      <td className="tabular value-negative">
                        {formatQty(r.diff)} ({r.pct.toFixed(1)}%)
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <Button variant="ghost" onClick={handleCopyText} style={{ marginTop: 12 }}>
                Скопировать текстом
              </Button>
            </>
          )}
        </Panel>
      )}
    </div>
  );
}
