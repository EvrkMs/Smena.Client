import { useEffect, useState } from 'react';
import Employees from './pages/Employees';
import Coming from './pages/Coming';
import Advance from './pages/Advance';
import Expense from './pages/Expense';
import Raport from './pages/Raport';
import Stockcount from './pages/Stockcount';
import { SafeTicker } from './components/SafeTicker';
import { Splash } from './components/Splash';
import { getCurrentSafe, subscribeToSafeChanges } from './bridge/api';
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
  const [active, setActive] = useState<TabKey>('raport');
  // Вкладка монтируется один раз при первом посещении и больше не размонтируется —
  // так стейт формы (введённые суммы, строки отчёта и т.п.) не обнуляется при
  // переключении между табами. Скрытые вкладки просто display:none, а не unmount.
  const [mounted, setMounted] = useState<Set<TabKey>>(new Set(['raport']));
  const [safe, setSafe] = useState<number | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    getCurrentSafe()
      .then(setSafe)
      .catch(() => {})
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
