import { useSyncExternalStore } from 'react';
import { getCurrentSafe, getEmployees, subscribeToSafeChanges, type Employee } from '../bridge/api';

// ---------------------------------------------------------------------------
// Общие данные приложения (сотрудники, сейф) поверх bridge/api.ts.
//
// Вкладки живут всё время работы приложения (App.tsx их не размонтирует),
// поэтому данные, загруженные каждой страницей "на mount", устаревали:
// сотрудник, добавленный на вкладке «Сотрудники», не появлялся в «Отчёте
// смены» и «Авансе» до перезапуска приложения. Здесь один источник правды:
// refresh* обновляет все вкладки разом, App.tsx дёргает refreshAll при
// старте, по кнопке «Обновить» в шапке и полингом, пока сервер недоступен.
//
// Ошибки здесь НЕ тостятся и не глотаются — refresh* бросает, а вызывающий
// (App) решает, что показать: разовый тост или бейдж «нет связи».
// ---------------------------------------------------------------------------

type Listener = () => void;

let employees: Employee[] = [];
let safe: number | null = null;

const listeners = new Set<Listener>();

function emit() {
  listeners.forEach((l) => l());
}

function subscribe(listener: Listener) {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

export function useEmployees(): Employee[] {
  return useSyncExternalStore(subscribe, () => employees);
}

export function useSafe(): number | null {
  return useSyncExternalStore(subscribe, () => safe);
}

/** Подписка на push-события сейфа из C# — вызывается один раз в App. */
export function startSafePush(): () => void {
  return subscribeToSafeChanges((value) => {
    safe = value;
    emit();
  });
}

export async function refreshEmployees(): Promise<void> {
  employees = await getEmployees();
  emit();
}

export async function refreshSafe(): Promise<void> {
  safe = await getCurrentSafe();
  emit();
}

/** Обновляет всё разом; false — хотя бы один запрос упал (связи нет). */
export async function refreshAll(): Promise<boolean> {
  const results = await Promise.allSettled([refreshEmployees(), refreshSafe()]);
  return results.every((r) => r.status === 'fulfilled');
}
