import { useEffect, useState } from 'react';
import { getCurrentSalary, getEmployees, sendAdvance, type Employee } from '../bridge/api';
import { Button, Checkbox, NumberField, Panel, Select } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';
import { formatMoney, parseIntSafe } from '../lib/format';

type Kind = 'advance' | 'salary';

/** Эквивалент AdvanceUserControl: выдача аванса или ЗП, наличные/безнал, из сейфа или нет. */
export default function Advance() {
  const toast = useToast();
  const confirm = useConfirm();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [employeeId, setEmployeeId] = useState('');
  const [amount, setAmount] = useState('');
  const [kind, setKind] = useState<Kind>('advance');
  const [isNonCash, setIsNonCash] = useState(false);
  const [extractFromSafe, setExtractFromSafe] = useState(true);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    getEmployees().then(setEmployees).catch((e) => toast('error', String(e)));
  }, []);

  const handleSubmit = async () => {
    if (!employeeId) return toast('error', 'Выберите сотрудника.');
    const value = parseIntSafe(amount);
    if (value <= 0) return toast('error', 'Введите сумму больше нуля.');

    let salaryCheck: { success: boolean; currentSalary: number; message: string };
    try {
      salaryCheck = await getCurrentSalary(employeeId);
    } catch (e) {
      return toast('error', String(e));
    }
    if (!salaryCheck.success) return toast('error', salaryCheck.message || 'Не удалось получить текущую ЗП.');
    if (value > salaryCheck.currentSalary) {
      return toast(
        'error',
        `Сумма превышает начисленное: доступно ${formatMoney(salaryCheck.currentSalary)} ₽.`,
      );
    }

    const employee = employees.find((e) => e.id === employeeId);
    const ok = await confirm({
      title: kind === 'salary' ? 'Выдача ЗП' : 'Выдача аванса',
      message:
        `${employee?.name ?? ''}\n${formatMoney(value)} ₽ ${isNonCash ? '(безнал)' : '(наличные)'}` +
        (extractFromSafe ? '\nиз сейфа' : ''),
    });
    if (!ok) return;

    setBusy(true);
    try {
      const res = await sendAdvance({
        employeeId,
        amount: value,
        isNonCash,
        isSalary: kind === 'salary',
        extractFromSafe,
        comment: kind === 'salary' ? 'ЗП' : 'Аванс',
      });
      if (res.success) {
        toast('success', 'Выдача проведена.');
        setAmount('');
      } else {
        toast('error', res.message || 'Сервер вернул ошибку.');
      }
    } catch (e) {
      toast('error', String(e));
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="screen-grid screen-grid-single">
      <Panel title="Аванс / ЗП">
        <Select label="Сотрудник" value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
          <option value="">— выберите —</option>
          {employees.map((e) => (
            <option key={e.id} value={e.id}>
              {e.name}
            </option>
          ))}
        </Select>

        <NumberField label="Сумма, ₽" value={amount} maxDigits={6} onValueChange={setAmount} />

        <div className="radio-row">
          <label className={`radio-pill ${kind === 'advance' ? 'active' : ''}`}>
            <input type="radio" checked={kind === 'advance'} onChange={() => setKind('advance')} />
            Аванс
          </label>
          <label className={`radio-pill ${kind === 'salary' ? 'active' : ''}`}>
            <input type="radio" checked={kind === 'salary'} onChange={() => setKind('salary')} />
            ЗП
          </label>
        </div>

        <Checkbox label="Безналичный расчёт" checked={isNonCash} onChange={setIsNonCash} />
        <Checkbox label="Списать из сейфа" checked={extractFromSafe} onChange={setExtractFromSafe} />

        <Button disabled={busy} onClick={handleSubmit}>
          {busy ? 'Провожу…' : 'Выдать'}
        </Button>
      </Panel>
    </div>
  );
}
