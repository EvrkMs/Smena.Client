import { useState } from 'react';
import { useApiEngine } from '../bridge/engine';
import { Button, NumberField, Panel, TextField } from '../components/ui/primitives';
import { useConfirm } from '../components/ui/ConfirmDialog';
import { useToast } from '../components/ui/Toast';
import { useSafe } from '../lib/appData';
import { formatMoney, parseIntSafe } from '../lib/format';

/** Эквивалент ComingUserControl: только положительный приход в сейф, без варианта расхода. */
export default function Coming() {
  const api = useApiEngine();
  const toast = useToast();
  const confirm = useConfirm();
  // Из общего store: начальное значение и push-обновления сейфа держит App (appData).
  const current = useSafe();
  const [amount, setAmount] = useState('');
  const [comment, setComment] = useState('');
  const [busy, setBusy] = useState(false);

  const handleSubmit = async () => {
    const value = parseIntSafe(amount);
    if (value <= 0) return toast('error', 'Введите сумму больше нуля.');

    const ok = await confirm({
      title: 'Приход в сейф',
      message: `Добавить ${formatMoney(value)} ₽ в сейф?${comment ? `\nКомментарий: ${comment}` : ''}`,
    });
    if (!ok) return;

    setBusy(true);
    const res = await api.addSafeComing(value, comment);
    setBusy(false);
    if (res) {
      toast('success', 'Приход зафиксирован.');
      setAmount('');
      setComment('');
    }
  };

  return (
    <div className="screen-grid screen-grid-single">
      <Panel title="Приход в сейф">
        <p className="muted" style={{ marginTop: -8, marginBottom: 18 }}>
          Текущая сумма в сейфе: <span className="tabular">{current === null ? '—' : formatMoney(current)} ₽</span>
        </p>
        <NumberField label="Сумма прихода, ₽" value={amount} maxDigits={6} onValueChange={setAmount} />
        <TextField label="Комментарий" value={comment} onChange={(e) => setComment(e.target.value)} />
        <Button disabled={busy} onClick={handleSubmit}>
          {busy ? 'Провожу…' : 'Провести приход'}
        </Button>
      </Panel>
    </div>
  );
}
