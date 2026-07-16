import { useEffect, useState, useMemo } from 'react';
import { subscribeToSafeChanges, type Employee, type ShiftConstants } from '../bridge/api';
import { useApiEngine } from '../bridge/engine';
import { Button, NumberField, Panel, Select, TextField } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';
import { formatMoney, parseIntSafe } from '../lib/format';
import { usePersistedState } from '../lib/hooks';

interface Row {
  employeeId: string;
  hours: string;
  minus: string;
}

const emptyRow: Row = { employeeId: '', hours: '', minus: '' };

const defaultConstants: ShiftConstants = {
  initialCashRegister: 1000,
  maxEmployeesPerShift: 3,
  maxHoursPerShift: 12,
  maxAmountDigits: 6,
  maxHoursDigits: 2,
};

export default function Raport() {
  const api = useApiEngine();
  const toast = useToast();
  const confirm = useConfirm();

  const [constants, setConstants] = useState<ShiftConstants>(defaultConstants);
  const [employees, setEmployees] = useState<Employee[]>([]);
  
  // Кешируемые состояния
  const [rows, setRows] = usePersistedState<Row[]>('raport_rows', [{ ...emptyRow }]);
  const [factCash, setFactCash] = usePersistedState('raport_factCash', '');
  const [factNonCash, setFactNonCash] = usePersistedState('raport_factNonCash', '');
  const [programCash, setProgramCash] = usePersistedState('raport_programCash', '');
  const [programNonCash, setProgramNonCash] = usePersistedState('raport_programNonCash', '');
  const [factSafe, setFactSafe] = usePersistedState('raport_factSafe', '');
  const [whyMinus, setWhyMinus] = usePersistedState('raport_whyMinus', '');

  // Состояния, которые не нужно кешировать (данные из API)
  const [programSafe, setProgramSafe] = useState<number | null>(null);
  const [busy, setBusy] = useState(false);
  const [progress, setProgress] = useState('');

  useEffect(() => {
    api.getConstants().then((c) => c && setConstants(c));
    api.getEmployees().then((list) => list && setEmployees(list));
    api.getCurrentSafe().then((v) => v !== null && setProgramSafe(v));
    // Вкладка не размонтируется после первого открытия (App.tsx держит все табы
    // смонтированными через display:none), поэтому одного getCurrentSafe() на mount
    // недостаточно — без подписки programSafe замирает на значении из момента первого
    // открытия вкладки и дальше расходится с реальным сейфом. Именно это приводило
    // к тому, что "Расхождение по сейфу" в предпросмотре не совпадало с тем, что
    // реально проверяет сервер при отправке отчёта (сервер берёт currentSafe свежим
    // из БД), из-за чего сумма "минусов" не сходилась и отчёт отклонялся.
    return subscribeToSafeChanges(setProgramSafe);
  }, []);

  const activeRows = rows.filter((r) => r.employeeId);
  const totalHours = activeRows.reduce((sum, r) => sum + parseIntSafe(r.hours), 0);

  const updateRow = (index: number, patch: Partial<Row>) => {
    setRows((prev) => {
      const next = [...prev];
      next[index] = { ...next[index], ...patch };
      const activeCount = next.filter((r) => r.employeeId).length;
      if (patch.employeeId !== undefined && index === next.length - 1 && activeCount === next.length && next.length < constants.maxEmployeesPerShift) {
        next.push({ ...emptyRow });
      }
      if (patch.employeeId === '') return next.slice(0, index + 1);
      return next;
    });
  };

  const handleHoursChange = (index: number, value: string) => {
    const candidateTotal = activeRows.reduce((sum, r, i) => sum + (i === index ? parseIntSafe(value) : parseIntSafe(r.hours)), 0);
    if (candidateTotal > constants.maxHoursPerShift) {
      toast('error', `Суммарно часов не может быть больше ${constants.maxHoursPerShift}.`);
      return;
    }
    updateRow(index, { hours: value });
  };

  const employeePayments = useMemo(() => {
    return activeRows.map((r) => {
      const emp = employees.find((e) => e.id === r.employeeId);
      const earned = (emp?.hourlyRate ?? 0) * parseIntSafe(r.hours);
      return { name: emp?.name ?? 'Неизвестен', amount: Math.max(0, earned - parseIntSafe(r.minus)) };
    });
  }, [activeRows, employees]);

  const salaryTotal = useMemo(() => employeePayments.reduce((sum, p) => sum + p.amount, 0), [employeePayments]);

  const factCashN = parseIntSafe(factCash);
  const factNonCashN = parseIntSafe(factNonCash);
  const programCashN = parseIntSafe(programCash);
  const programNonCashN = parseIntSafe(programNonCash);
  const factSafeN = parseIntSafe(factSafe);

  const revenue = factCashN - constants.initialCashRegister + factNonCashN;
  const total = revenue - salaryTotal;
  const cashDiscrepancy = factCashN + factNonCashN - (programCashN + programNonCashN);
  const safeDiscrepancy = programSafe === null ? 0 : factSafeN - programSafe;

  // Зеркало серверной формулы (RaportOperationsService.CalculateDeltas):
  // недостача по кассе и недостача по сейфу считаются НЕЗАВИСИМО друг от друга —
  // излишек в одном не гасит недостачу в другом. Пример: касса -100, сейф +100 →
  // сотрудник всё равно должен получить минус 100 (не 0), потому что сервер
  // суммирует max(0,-cashDelta) + max(0,-safeDelta), а не netting (cashDelta+safeDelta).
  const totalMinusExpected = (cashDiscrepancy < 0 ? -cashDiscrepancy : 0) + (safeDiscrepancy < 0 ? -safeDiscrepancy : 0);
  const totalMinusEntered = activeRows.reduce((sum, r) => sum + parseIntSafe(r.minus), 0);
  // Проверяем только когда есть хотя бы один сотрудник — до этого сумма минусов
  // бессмысленна, а ошибку "Добавьте сотрудника" и так покажет handleSend.
  const minusMismatch = activeRows.length > 0 && totalMinusEntered !== totalMinusExpected;

  const formatDiscrepancy = (val: number) => {
    const formatted = formatMoney(val);
    return val > 0 ? `+${formatted}` : formatted;
  };

  const handleSend = async () => {
    if (activeRows.length === 0) return toast('error', 'Добавьте сотрудника.');
    // Раньше это узнавали только после ответа сервера — уже дождавшись фото от
    // сотрудника через Telegram. Теперь считаем ту же сумму заранее и просто не
    // даём отправить, пока минус не сойдётся — кнопка ниже задизейблена тем же
    // условием (minusMismatch), это дублирующая проверка на случай гонки.
    if (minusMismatch) return toast('error', `Сумма минусов должна быть равна ${totalMinusExpected}, введено ${totalMinusEntered}.`);
    if (cashDiscrepancy < 0 && !whyMinus.trim()) return toast('error', 'Укажите причину недостачи.');

    // Доп. модалка именно на расхождение по сейфу — отдельно от общего
    // подтверждения ниже. Даже если минус уже сходится (проверка выше это
    // гарантирует), кассир должен явно увидеть цифру расхождения и подтвердить
    // её осознанно, а не просто нажать общее "Отправить отчёт?".
    if (safeDiscrepancy !== 0) {
      const okSafe = await confirm({
        title: 'Расхождение по сейфу',
        message: `Обнаружено расхождение по сейфу: ${formatDiscrepancy(safeDiscrepancy)} ₽. Продолжить закрытие смены?`,
      });
      if (!okSafe) return;
    }
    
    const ok = await confirm({ title: 'Отправка отчёта', message: 'Отправить отчет?' });
    if (!ok) return;

    setBusy(true);
    setProgress('Запрашиваю фото…');

    // result.success === false (сервер отклонил отчёт по бизнес-правилам, например
    // "Сумма минусов должна быть равна N") и сетевые/JSON-исключения теперь ловит
    // и тостит Engine одинаково — раньше именно рассинхрон между "не бросает
    // исключение" и "нужно проверять result.success" был источником бага, когда
    // форма показывала "Отчёт отправлен" и чистилась, даже если отчёт не сохранился.
    const result = await api.sendRaportWithPhoto({
      factCash: factCashN, factNonCash: factNonCashN, programCash: programCashN,
      programNonCash: programNonCashN, factSafe: factSafeN, whyMinus,
      employees: activeRows.map((r) => ({ employeeId: r.employeeId, hours: parseIntSafe(r.hours), minus: parseIntSafe(r.minus) })),
    }, setProgress);

    setBusy(false);
    setProgress('');

    if (!result) return;

    toast('success', 'Отчёт отправлен.');

    // Очистка кэша после отправки
    localStorage.removeItem('raport_rows');
    localStorage.removeItem('raport_factCash');
    localStorage.removeItem('raport_factNonCash');
    localStorage.removeItem('raport_programCash');
    localStorage.removeItem('raport_programNonCash');
    localStorage.removeItem('raport_factSafe');
    localStorage.removeItem('raport_whyMinus');

    setRows([{ ...emptyRow }]);
    setFactCash(''); setFactNonCash(''); setProgramCash(''); setProgramNonCash(''); setFactSafe(''); setWhyMinus('');
  };

  return (
    <div className="screen-grid raport-grid">
      <div className="left-column" style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
        <Panel title="Сотрудники смены">
          {rows.map((row, i) => (
            <div className="raport-row" key={i}>
              <Select label={`Сотрудник ${i + 1}`} value={row.employeeId} onChange={(e) => updateRow(i, { employeeId: e.target.value })}>
                <option value="">— не выбран —</option>
                {employees.map((e) => <option key={e.id} value={e.id}>{e.name}</option>)}
              </Select>
              <NumberField label="Часы" value={row.hours} onValueChange={(v) => handleHoursChange(i, v)} disabled={!row.employeeId} />
              <NumberField label="Минус" value={row.minus} onValueChange={(v) => updateRow(i, { minus: v })} disabled={!row.employeeId} />
            </div>
          ))}
          <p className="muted tabular" style={{ marginTop: 4 }}>Часов по смене: {totalHours} / {constants.maxHoursPerShift}</p>
        </Panel>

        {employeePayments.length > 0 && (
          <Panel title="К выплате сотрудникам">
            <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
              {employeePayments.map((p, i) => (
                <li key={i} style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0' }}>
                  <span>{p.name}:</span>
                  <span className="tabular">{formatMoney(p.amount)} ₽</span>
                </li>
              ))}
            </ul>
          </Panel>
        )}
      </div>

      <Panel title="Касса">
        <div className="raport-stack">
          <NumberField label="Факт нал., ₽" value={factCash} onValueChange={setFactCash} />
          <NumberField label="Факт безнал., ₽" value={factNonCash} onValueChange={setFactNonCash} />
          <NumberField label="Программа нал., ₽" value={programCash} onValueChange={setProgramCash} />
          <NumberField label="Программа безнал., ₽" value={programNonCash} onValueChange={setProgramNonCash} />
        </div>
        <NumberField label="Факт в сейфе, ₽" value={factSafe} onValueChange={setFactSafe} />
        {cashDiscrepancy < 0 && <TextField label="Причина недостачи" value={whyMinus} onChange={(e) => setWhyMinus(e.target.value)} />}
      </Panel>

      <Panel title="Предпросмотр">
        <dl className="preview-list">
          <div><dt>Выручка</dt><dd className="tabular">{formatMoney(revenue)} ₽</dd></div>
          <div><dt>ФОТ смены</dt><dd className="tabular">{formatMoney(salaryTotal)} ₽</dd></div>
          <div className="preview-total"><dt>Итого</dt><dd className="tabular">{formatMoney(total)} ₽</dd></div>
          <div>
            <dt>Расхождение по кассе</dt>
            <dd className={`tabular ${cashDiscrepancy < 0 ? 'value-negative' : cashDiscrepancy > 0 ? 'value-positive' : ''}`}>
              {formatDiscrepancy(cashDiscrepancy)} ₽
            </dd>
          </div>
          <div>
            <dt>Расхождение по сейфу</dt>
            <dd className={`tabular ${safeDiscrepancy < 0 ? 'value-negative' : safeDiscrepancy > 0 ? 'value-positive' : ''}`}>
              {formatDiscrepancy(safeDiscrepancy)} ₽
            </dd>
          </div>
        </dl>
        {activeRows.length > 0 && (
          <p className={`muted tabular ${minusMismatch ? 'value-negative' : ''}`} style={{ marginTop: 4 }}>
            Введено минуса: {totalMinusEntered} / требуется {totalMinusExpected}
          </p>
        )}
        {progress && <p className="muted photo-status">{progress}</p>}
        <Button disabled={busy || minusMismatch} onClick={handleSend}>{busy ? 'Отправляю…' : 'Отправить отчёт'}</Button>
      </Panel>
    </div>
  );
}