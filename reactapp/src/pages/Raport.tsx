import { useEffect, useState } from 'react';
import {
  getConstants,
  getCurrentSafe,
  getEmployees,
  sendRaportWithPhoto,
  type Employee,
  type ShiftConstants,
} from '../bridge/api';
import { Button, NumberField, Panel, Select, TextField } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';
import { formatMoney, parseIntSafe } from '../lib/format';

interface Row {
  employeeId: string;
  hours: string;
  minus: string;
}

const emptyRow: Row = { employeeId: '', hours: '', minus: '' };

// Разумные значения по умолчанию, пока реальные константы не подгрузились с C#-стороны
// (GetConstantsAsync — асинхронный вызов, как и всё остальное в WebView2 host object proxy).
const defaultConstants: ShiftConstants = {
  initialCashRegister: 1000,
  maxEmployeesPerShift: 3,
  maxHoursPerShift: 12,
  maxAmountDigits: 6,
  maxHoursDigits: 2,
};

/**
 * Эквивалент RaportUserControl — воспроизведена бизнес-логика (каскад строк сотрудников,
 * лимит часов на смену, расхождения по кассе/сейфу, фото-подтверждение), НЕ построчный
 * порт оригинального .cs.
 */
export default function Raport() {
  const toast = useToast();
  const confirm = useConfirm();

  const [constants, setConstants] = useState<ShiftConstants>(defaultConstants);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [rows, setRows] = useState<Row[]>([{ ...emptyRow }]);
  const [factCash, setFactCash] = useState('');
  const [factNonCash, setFactNonCash] = useState('');
  const [programCash, setProgramCash] = useState('');
  const [programNonCash, setProgramNonCash] = useState('');
  const [factSafe, setFactSafe] = useState('');
  const [programSafe, setProgramSafe] = useState<number | null>(null);
  const [whyMinus, setWhyMinus] = useState('');
  const [busy, setBusy] = useState(false);
  const [progress, setProgress] = useState('');

  useEffect(() => {
    getConstants().then(setConstants).catch((e) => toast('error', String(e)));
    getEmployees().then(setEmployees).catch((e) => toast('error', String(e)));
    getCurrentSafe().then(setProgramSafe).catch(() => {});
  }, []);

  const activeRows = rows.filter((r) => r.employeeId);
  const totalHours = activeRows.reduce((sum, r) => sum + parseIntSafe(r.hours), 0);

  const updateRow = (index: number, patch: Partial<Row>) => {
    setRows((prev) => {
      const next = [...prev];
      next[index] = { ...next[index], ...patch };

      const activeCount = next.filter((r) => r.employeeId).length;
      if (
        patch.employeeId !== undefined &&
        index === next.length - 1 &&
        activeCount === next.length &&
        next.length < constants.maxEmployeesPerShift
      ) {
        next.push({ ...emptyRow });
      }
      if (patch.employeeId === '') {
        return next.slice(0, index + 1);
      }
      return next;
    });
  };

  const handleHoursChange = (index: number, value: string) => {
    const candidateTotal = activeRows.reduce(
      (sum, r, i) => sum + (i === index ? parseIntSafe(value) : parseIntSafe(r.hours)),
      0,
    );
    if (candidateTotal > constants.maxHoursPerShift) {
      toast('error', `Суммарно часов по смене не может быть больше ${constants.maxHoursPerShift}.`);
      return;
    }
    updateRow(index, { hours: value });
  };

  const salaryTotal = activeRows.reduce((sum, r) => {
    const emp = employees.find((e) => e.id === r.employeeId);
    const earned = (emp?.hourlyRate ?? 0) * parseIntSafe(r.hours);
    return sum + Math.max(0, earned - parseIntSafe(r.minus));
  }, 0);

  const factCashN = parseIntSafe(factCash);
  const factNonCashN = parseIntSafe(factNonCash);
  const programCashN = parseIntSafe(programCash);
  const programNonCashN = parseIntSafe(programNonCash);
  const factSafeN = parseIntSafe(factSafe);

  const revenue = factCashN - constants.initialCashRegister + factNonCashN;
  const total = revenue - salaryTotal;
  const cashDiscrepancy = factCashN + factNonCashN - (programCashN + programNonCashN);
  const safeDiscrepancy = programSafe === null ? 0 : factSafeN - programSafe;

  const validate = (): string | null => {
    if (activeRows.length === 0) return 'Добавьте хотя бы одного сотрудника.';
    if (activeRows.some((r) => parseIntSafe(r.hours) <= 0)) return 'Укажите часы для каждого сотрудника в смене.';
    if (!factCash && !factNonCash) return 'Укажите факт по кассе (наличные/безнал).';
    if (cashDiscrepancy < 0 && !whyMinus.trim()) return 'Есть недостача по кассе — укажите причину.';
    return null;
  };

  const handleSend = async () => {
    const error = validate();
    if (error) return toast('error', error);

    const ok = await confirm({
      title: 'Отправка отчёта',
      message:
        `Итог: ${formatMoney(total)} ₽\n` +
        (safeDiscrepancy !== 0 ? `Расхождение по сейфу: ${formatMoney(safeDiscrepancy)} ₽\n` : '') +
        (cashDiscrepancy < 0 ? `Недостача по кассе: ${formatMoney(cashDiscrepancy)} ₽\n` : '') +
        'Отправить отчёт и запросить фото у первого сотрудника?',
      danger: cashDiscrepancy < 0,
    });
    if (!ok) return;

    setBusy(true);
    setProgress('Запрашиваю фото…');
    try {
      const res = await sendRaportWithPhoto(
        {
          factCash: factCashN,
          factNonCash: factNonCashN,
          programCash: programCashN,
          programNonCash: programNonCashN,
          factSafe: factSafeN,
          whyMinus,
          employees: activeRows.map((r) => ({
            employeeId: r.employeeId,
            hours: parseIntSafe(r.hours),
            minus: parseIntSafe(r.minus),
          })),
        },
        setProgress,
      );

      if (res.success) {
        toast('success', 'Отчёт отправлен.');
        setRows([{ ...emptyRow }]);
        setFactCash('');
        setFactNonCash('');
        setProgramCash('');
        setProgramNonCash('');
        setFactSafe('');
        setWhyMinus('');
      } else {
        toast('error', res.message || 'Сервер вернул ошибку.');
      }
    } catch (e) {
      toast('error', String(e));
    } finally {
      setBusy(false);
      setProgress('');
    }
  };

  return (
    <div className="screen-grid raport-grid">
      <Panel title="Сотрудники смены">
        {rows.map((row, i) => (
          <div className="raport-row" key={i}>
            <Select
              label={`Сотрудник ${i + 1}`}
              value={row.employeeId}
              onChange={(e) => updateRow(i, { employeeId: e.target.value })}
            >
              <option value="">— не выбран —</option>
              {employees.map((e) => (
                <option key={e.id} value={e.id}>
                  {e.name}
                </option>
              ))}
            </Select>
            <NumberField
              label="Часы"
              value={row.hours}
              maxDigits={constants.maxHoursDigits}
              onValueChange={(v) => handleHoursChange(i, v)}
              disabled={!row.employeeId}
            />
            <NumberField
              label="Минус"
              value={row.minus}
              maxDigits={constants.maxAmountDigits}
              onValueChange={(v) => updateRow(i, { minus: v })}
              disabled={!row.employeeId}
            />
          </div>
        ))}
        <p className="muted tabular" style={{ marginTop: 4 }}>
          Часов по смене: {totalHours} / {constants.maxHoursPerShift}
        </p>
      </Panel>

      <Panel title="Касса">
        <div className="raport-row">
          <NumberField label="Факт нал., ₽" value={factCash} maxDigits={constants.maxAmountDigits} onValueChange={setFactCash} />
          <NumberField label="Факт безнал., ₽" value={factNonCash} maxDigits={constants.maxAmountDigits} onValueChange={setFactNonCash} />
        </div>
        <div className="raport-row">
          <NumberField label="Программа нал., ₽" value={programCash} maxDigits={constants.maxAmountDigits} onValueChange={setProgramCash} />
          <NumberField label="Программа безнал., ₽" value={programNonCash} maxDigits={constants.maxAmountDigits} onValueChange={setProgramNonCash} />
        </div>
        <NumberField label="Факт в сейфе, ₽" value={factSafe} maxDigits={constants.maxAmountDigits} onValueChange={setFactSafe} />
        <p className="muted tabular" style={{ marginTop: -6 }}>
          По программе в сейфе: {programSafe === null ? '—' : formatMoney(programSafe)} ₽
        </p>
        {cashDiscrepancy < 0 && (
          <TextField label="Причина недостачи" value={whyMinus} onChange={(e) => setWhyMinus(e.target.value)} />
        )}
      </Panel>

      <Panel title="Предпросмотр">
        <dl className="preview-list">
          <div><dt>Выручка</dt><dd className="tabular">{formatMoney(revenue)} ₽</dd></div>
          <div><dt>ФОТ смены</dt><dd className="tabular">{formatMoney(salaryTotal)} ₽</dd></div>
          <div className="preview-total"><dt>Итого</dt><dd className="tabular">{formatMoney(total)} ₽</dd></div>
          <div>
            <dt>Расхождение по кассе</dt>
            <dd className={`tabular ${cashDiscrepancy < 0 ? 'value-negative' : ''}`}>
              {formatMoney(cashDiscrepancy)} ₽
            </dd>
          </div>
          <div>
            <dt>Расхождение по сейфу</dt>
            <dd className={`tabular ${safeDiscrepancy < 0 ? 'value-negative' : ''}`}>
              {formatMoney(safeDiscrepancy)} ₽
            </dd>
          </div>
        </dl>
        {progress && <p className="muted photo-status">{progress}</p>}
        <Button disabled={busy} onClick={handleSend}>
          {busy ? 'Отправляю…' : 'Отправить отчёт'}
        </Button>
      </Panel>
    </div>
  );
}
