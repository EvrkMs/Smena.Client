import { useEffect, useState } from 'react';
import type { Employee } from '../bridge/api';
import { useApiEngine } from '../bridge/engine';
import { Button, Checkbox, NumberField, Panel, Select, TextField } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';
import { formatMoney, parseIntSafe } from '../lib/format';

/** Эквивалент ExpenseUserControl: расход (из сейфа или нет), опционально с запросом фото у сотрудника. */
export default function Expense() {
  const api = useApiEngine();
  const toast = useToast();
  const confirm = useConfirm();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [amount, setAmount] = useState('');
  const [comment, setComment] = useState('');
  const [fromSafe, setFromSafe] = useState(true);
  const [sendPhoto, setSendPhoto] = useState(false);
  const [employeeId, setEmployeeId] = useState('');
  const [busy, setBusy] = useState(false);
  const [progress, setProgress] = useState('');

  useEffect(() => {
    api.getEmployees().then((list) => list && setEmployees(list));
  }, []);

  const handleSubmit = async () => {
    const value = parseIntSafe(amount);
    if (value <= 0) return toast('error', 'Введите сумму больше нуля.');
    if (sendPhoto && !employeeId) return toast('error', 'Выберите сотрудника для отправки фото.');

    const employee = employees.find((e) => e.id === employeeId);
    const ok = await confirm({
      title: 'Расход',
      message:
        `${formatMoney(value)} ₽ ${fromSafe ? '(из сейфа)' : '(безнал)'}` +
        (comment ? `\n${comment}` : '') +
        (sendPhoto ? `\nФото запрошено у: ${employee?.name ?? ''}` : ''),
      danger: true,
      confirmLabel: 'Провести расход',
    });
    if (!ok) return;

    setBusy(true);
    const res = sendPhoto
      ? await api.sendExpenseWithPhoto({ amount: value, comment, fromSafe, employeeId, senderName: employee?.name }, setProgress)
      : await api.sendExpense({ amount: value, comment, fromSafe });
    setBusy(false);
    setProgress('');

    if (res) {
      toast('success', 'Расход проведён.');
      setAmount('');
      setComment('');
    }
  };

  return (
    <div className="screen-grid screen-grid-single">
      <Panel title="Расход">
        <NumberField label="Сумма, ₽" value={amount} maxDigits={6} onValueChange={setAmount} />
        <TextField label="Комментарий" value={comment} onChange={(e) => setComment(e.target.value)} />
        <Checkbox label="Списать из сейфа" checked={fromSafe} onChange={setFromSafe} />
        <Checkbox label="Запросить фото у сотрудника" checked={sendPhoto} onChange={setSendPhoto} />

        {sendPhoto && (
          <Select label="Сотрудник" value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
            <option value="">— выберите —</option>
            {employees.map((e) => (
              <option key={e.id} value={e.id}>
                {e.name}
              </option>
            ))}
          </Select>
        )}

        {progress && <p className="muted photo-status">{progress}</p>}

        <Button variant="danger" disabled={busy} onClick={handleSubmit}>
          {busy ? 'Провожу…' : 'Провести расход'}
        </Button>
      </Panel>
    </div>
  );
}
