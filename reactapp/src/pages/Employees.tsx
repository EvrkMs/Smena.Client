import { useEffect, useState } from 'react';
import { addEmployee, getEmployees, type Employee } from '../bridge/api';
import { Button, NumberField, Panel, TextField } from '../components/ui/primitives';
import { useToast } from '../components/ui/Toast';

const emptyForm = { name: '', hourlyRate: '', telegramId: '', salaryThreadId: '' };

export default function Employees() {
  const toast = useToast();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);

  const reload = () => {
    getEmployees()
      .then(setEmployees)
      .catch((e) => toast('error', String(e)))
      .finally(() => setLoading(false));
  };

  useEffect(reload, []);

  const handleAdd = async () => {
    if (!form.name.trim()) return toast('error', 'Введите имя сотрудника.');

    const hourlyRate = Number(form.hourlyRate);
    if (!Number.isFinite(hourlyRate) || hourlyRate <= 0) return toast('error', 'Введите корректную ставку.');

    setBusy(true);
    try {
      const res = await addEmployee({
        name: form.name.trim(),
        hourlyRate,
        telegramId: form.telegramId ? Number(form.telegramId) : 0,
        salaryThreadId: form.salaryThreadId ? Number(form.salaryThreadId) : 0,
      });
      if (res.success) {
        setForm(emptyForm);
        toast('success', 'Сотрудник добавлен.');
        reload();
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
    <div className="screen-grid">
      <Panel title="Сотрудники">
        {loading ? (
          <p className="muted">Загрузка…</p>
        ) : employees.length === 0 ? (
          <p className="muted">Список пуст.</p>
        ) : (
          <ul className="ledger-list">
            {employees.map((e) => (
              <li key={e.id}>
                <span>{e.name}</span>
                <span className="tabular muted">{e.hourlyRate} ₽/ч</span>
              </li>
            ))}
          </ul>
        )}
      </Panel>

      <Panel title="Добавить сотрудника">
        <TextField label="Имя" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        <NumberField
          label="Ставка, ₽/час"
          value={form.hourlyRate}
          maxDigits={5}
          onValueChange={(v) => setForm({ ...form, hourlyRate: v })}
        />
        <NumberField
          label="Telegram ID (необязательно)"
          value={form.telegramId}
          onValueChange={(v) => setForm({ ...form, telegramId: v })}
        />
        <NumberField
          label="Salary thread ID (необязательно)"
          value={form.salaryThreadId}
          onValueChange={(v) => setForm({ ...form, salaryThreadId: v })}
        />
        <Button disabled={busy} onClick={handleAdd}>
          {busy ? 'Добавляю…' : 'Добавить'}
        </Button>
      </Panel>
    </div>
  );
}
