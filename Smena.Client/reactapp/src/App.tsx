import { useEffect, useState } from 'react';
import Employees from './pages/Employees';
import Coming from './pages/Coming';
import Advance from './pages/Advance';
import Expense from './pages/Expense';
import Raport from './pages/Raport';
import Stockcount from './pages/Stockcount';
import { SafeTicker } from './components/SafeTicker';
import { Splash } from './components/Splash';
import { subscribeToSafeChanges } from './bridge/api';
import { useApiEngine } from './bridge/engine';
import './App.css';

const tabs = [
  { key: 'raport', label: 'Отчёт смены', component: Raport },
  { key: 'advance', label: 'Аванс / ЗП', component: Advance },
  { key: 'expense', label: 'Расход', component: Expense },
  { key: 'coming', label: 'Приход', component: Coming },
  { key: 'stockcount', label: 'Пересчёт склада', component: Stockcount },
  { key: 'employees', label: 'Сотрудники', component: Employees },
] as const;

type TabKey = (typeof tabs)[number]['key'];

export default function App() {
  const api = useApiEngine();
  const [active, setActive] = useState<TabKey>('raport');
  // Вкладка монтируется один раз при первом посещении и больше не размонтируется —
  // так стейт формы (введённые суммы, строки отчёта и т.п.) не обнуляется при
  // переключении между табами. Скрытые вкладки просто display:none, а не unmount.
  const [mounted, setMounted] = useState<Set<TabKey>>(new Set(['raport']));
  const [safe, setSafe] = useState<number | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    // Раньше ошибка тут глушилась молча (catch(() => {})) — через Engine она,
    // как и везде, всплывёт тостом слева. На готовность экрана (ready) это не
    // влияет: сплэш всё равно снимается через finally, SafeTicker сам покажет
    // "------" при null, а подписка ниже подхватит значение, как только сервер
    // станет доступен.
    api.getCurrentSafe()
      .then((v) => v !== null && setSafe(v))
      .finally(() => setReady(true));
    return subscribeToSafeChanges(setSafe);
  }, []);

  const selectTab = (key: TabKey) => {
    setActive(key);
    setMounted((prev) => (prev.has(key) ? prev : new Set(prev).add(key)));
  };

  if (!ready) return <Splash />;

  return (
    <div className="app-shell">
      <header className="app-header">
        <span className="app-title">Smena.Client</span>
        <SafeTicker value={safe} />
      </header>
      <div className="app-body">
        <nav className="tabs">
          {tabs.map((t) => (
            <button
              key={t.key}
              className={t.key === active ? 'tab active' : 'tab'}
              onClick={() => selectTab(t.key)}
            >
              {t.label}
            </button>
          ))}
        </nav>
        <main className="tab-content">
          {tabs
            .filter((t) => mounted.has(t.key))
            .map((t) => (
              <div key={t.key} hidden={t.key !== active}>
                <t.component />
              </div>
            ))}
        </main>
      </div>
    </div>
  );
}
