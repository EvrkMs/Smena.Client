import { useEffect, useState } from 'react';
import type { Employee } from '../bridge/api';
import { useApiEngine } from '../bridge/engine';
import { Button, Checkbox, NumberField, Panel, Select } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';
import { formatMoney, parseIntSafe } from '../lib/format';

type Kind = 'advance' | 'salary';

/** Эквивалент AdvanceUserControl: выдача аванса или ЗП, наличные/безнал, из сейфа или нет. */
export default function Advance() {
  const api = useApiEngine();
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
    api.getEmployees().then((list) => list && setEmployees(list));
  }, []);

  const handleSubmit = async () => {
    if (!employeeId) return toast('error', 'Выберите сотрудника.');
    const value = parseIntSafe(amount);
    if (value <= 0) return toast('error', 'Введите сумму больше нуля.');

    // getCurrentSalary — обычный вызов через Engine: если провалится (сеть/success:false),
    // тост уже показан, дальше идти незачем.
    const salaryCheck = await api.getCurrentSalary(employeeId);
    if (!salaryCheck) return;
    // А вот "сумма больше начисленного" — доменное правило конкретно этого экрана,
    // не ошибка API, поэтому остаётся здесь, а не в Engine.
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
    const res = await api.sendAdvance({
      employeeId,
      amount: value,
      isNonCash,
      isSalary: kind === 'salary',
      extractFromSafe,
      comment: kind === 'salary' ? 'ЗП' : 'Аванс',
    });
    setBusy(false);
    if (res) {
      toast('success', 'Выдача проведена.');
      setAmount('');
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
