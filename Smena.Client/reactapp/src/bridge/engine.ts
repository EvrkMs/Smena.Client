import { useMemo } from 'react';
import { useToast } from '../components/ui/Toast';
import * as api from './api';
import type {
  AdvanceRequest,
  Employee,
  ExpenseRequest,
  ExpenseWithPhotoRequest,
  RaportRequest,
} from './api';

// ---------------------------------------------------------------------------
// useApiEngine - единая точка вызова bridge/api.ts.
//
// bridge/api.ts остаётся чистой JSON-границей (как было). Этот слой поверх
// него берёт на себя то, что раньше дублировалось в каждой странице:
//   - catch транспортных исключений (сеть, JSON, WebView2-мост недоступен)
//   - проверку "success: false" в ответах, которые её возвращают
//   - показ toast('error', ...) в обоих случаях
//
// Страница получает либо результат вызова, либо null (если null - тост уже
// показан, страница ничего дополнительно сообщать не должна).
//
// Что сюда НЕ входит намеренно:
//   - клиентские бизнес-проверки перед отправкой (сумма > 0, minusMismatch
//     в Raport, "сумма превышает начисленное" в Advance) - это доменные
//     правила конкретного экрана, а не ошибка API;
//   - текст toast об успехе - он у каждого действия свой, сервер его не шлёт;
//   - busy-состояние кнопки - остаётся локальным в каждой странице.
// ---------------------------------------------------------------------------

type ApiResultLike = { success: boolean; message?: string };

function isApiResultLike(value: unknown): value is ApiResultLike {
  return (
    typeof value === 'object' &&
    value !== null &&
    'success' in value &&
    typeof (value as { success: unknown }).success === 'boolean'
  );
}

export function useApiEngine() {
  const toast = useToast();

  // useToast() отдаёт мемоизированную (useCallback) функцию, поэтому весь
  // возвращаемый объект можно безопасно завязать на [toast] - ссылка на api
  // стабильна между рендерами, и её можно не таскать в deps эффектов, которые
  // должны отработать только на монтировании (как было и раньше с toast/confirm).
  // `call` объявлен внутри фабрики useMemo намеренно - так у него нет внешних
  // зависимостей, кроме toast, который уже в deps-массиве ниже.
  return useMemo(() => {
    // Общий вызов: ловит исключения и авто-тостит success:false у ответов
    // такой формы. Для остальных форм ответа (массивы, числа, объекты без
    // success) просто прокидывает результат как есть.
    async function call<T>(promise: Promise<T>, fallbackMessage = 'Ошибка запроса.'): Promise<T | null> {
      try {
        const result = await promise;
        if (isApiResultLike(result) && !result.success) {
          toast('error', result.message || fallbackMessage);
          return null;
        }
        return result;
      } catch (e) {
        toast('error', e instanceof Error ? e.message : String(e));
        return null;
      }
    }

    return {
      // ---------- Constants ----------
      getConstants: () => call(api.getConstants()),

      // ---------- Employees ----------
      getEmployees: () => call(api.getEmployees()),
      addEmployee: (employee: Omit<Employee, 'id'>) => call(api.addEmployee(employee)),
      getCurrentSalary: (employeeId: string) => call(api.getCurrentSalary(employeeId)),

      // ---------- Safe / Coming ----------
      getCurrentSafe: () => call(api.getCurrentSafe()),
      addSafeComing: (amount: number, comment: string) => call(api.addSafeComing(amount, comment)),

      // ---------- Advance ----------
      sendAdvance: (req: AdvanceRequest) => call(api.sendAdvance(req)),

      // ---------- Expense ----------
      sendExpense: (req: ExpenseRequest) => call(api.sendExpense(req)),
      sendExpenseWithPhoto: (req: ExpenseWithPhotoRequest, onProgress?: (message: string) => void) =>
        call(api.sendExpenseWithPhoto(req, onProgress)),

      // ---------- Raport ----------
      sendRaportWithPhoto: (req: RaportRequest, onProgress?: (message: string) => void) =>
        call(api.sendRaportWithPhoto(req, onProgress)),

      // ---------- Photo ----------
      // Отмена действует только на фазу ожидания фото; после получения фото — no-op.
      cancelPhotoRequest: () => call(api.cancelPhotoRequest()),

      // ---------- Warehouse ----------
      searchWarehouseItems: (query: string, limit?: number) => call(api.searchWarehouseItems(query, limit)),

      // Особый случай: GetAllWarehouseItemsAsync возвращает ошибку в поле
      // `error`, а не в форме {success:false} - под общий call() не подходит,
      // поэтому обрабатывается здесь явно, но по тому же принципу (авто-тост,
      // null при неудаче).
      getAllWarehouseItems: async () => {
        const result = await call(api.getAllWarehouseItems());
        if (result && result.error) {
          toast('error', result.error);
          return null;
        }
        return result;
      },
    };
  }, [toast]);
}
