import { useCallback, useEffect, useState } from 'react';
import Employees from './pages/Employees';
import Coming from './pages/Coming';
import Advance from './pages/Advance';
import Expense from './pages/Expense';
import Raport from './pages/Raport';
import Stockcount from './pages/Stockcount';
import { SafeTicker } from './components/SafeTicker';
import { Splash } from './components/Splash';
import { useToast } from './components/ui/Toast';
import { refreshAll, startSafePush, useSafe } from './lib/appData';
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
  const toast = useToast();
  const safe = useSafe();
  const [active, setActive] = useState<TabKey>('raport');
  // Вкладка монтируется один раз при первом посещении и больше не размонтируется —
  // так стейт формы (введённые суммы, строки отчёта и т.п.) не обнуляется при
  // переключении между табами. Скрытые вкладки просто display:none, а не unmount.
  const [mounted, setMounted] = useState<Set<TabKey>>(new Set(['raport']));
  const [ready, setReady] = useState(false);
  const [offline, setOffline] = useState(false);
  const [refreshing, setRefreshing] = useState(false);

  // Единая перезагрузка данных (сотрудники + сейф) в общий store. Без тостов:
  // разовые уведомления решают вызывающие, а состояние связи показывает бейдж.
  const runRefresh = useCallback(async () => {
    const ok = await refreshAll();
    setOffline(!ok);
    return ok;
  }, []);

  useEffect(() => {
    runRefresh()
      .then((ok) => {
        if (!ok) toast('error', 'Нет связи с сервером — продолжаю попытки в фоне.');
      })
      .finally(() => setReady(true));
    return startSafePush();
  }, []);

  // Полинг, пока сервер недоступен: тихие повторы раз в 15 секунд. Когда связь
  // вернётся, offline снимется, бейдж исчезнет и полинг остановится сам.
  useEffect(() => {
    if (!offline) return;
    const timer = window.setInterval(() => {
      void runRefresh();
    }, 15_000);
    return () => window.clearInterval(timer);
  }, [offline, runRefresh]);

  const handleRefresh = async () => {
    setRefreshing(true);
    const ok = await runRefresh();
    setRefreshing(false);
    if (ok) toast('success', 'Данные обновлены.');
    else toast('error', 'Нет связи с сервером.');
  };

  const selectTab = (key: TabKey) => {
    setActive(key);
    setMounted((prev) => (prev.has(key) ? prev : new Set(prev).add(key)));
  };

  if (!ready) return <Splash />;

  return (
    <div className="app-shell">
      <header className="app-header">
        <span className="app-title">Smena.Client</span>
        <div className="app-header-right">
          {offline && <span className="offline-badge">нет связи</span>}
          <button
            className="refresh-btn"
            title="Обновить данные с сервера (сотрудники, сейф)"
            onClick={handleRefresh}
            disabled={refreshing}
          >
            ⟳
          </button>
          <SafeTicker value={safe} />
        </div>
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
