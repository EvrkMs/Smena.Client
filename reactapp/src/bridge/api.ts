import type { NativeApiHostObject } from './webview2';

// ---------- Domain types ----------

export interface Employee {
  id: string;
  name: string;
  hourlyRate: number;
  telegramId: number;
  salaryThreadId: number;
}

export interface ShiftConstants {
  initialCashRegister: number;
  maxEmployeesPerShift: number;
  maxHoursPerShift: number;
  maxAmountDigits: number;
  maxHoursDigits: number;
}

export interface ApiResult {
  success: boolean;
  message: string;
}

export interface WarehouseItem {
  name: string;
  article: string;
  folder: string;
  stock: number;
}

// ---------- Host detection ----------

export const isHosted = typeof window.chrome?.webview?.hostObjects?.api !== 'undefined';

function hostApi(): NativeApiHostObject {
  if (!isHosted) throw new Error('Not running inside WebView2 host.');
  return window.chrome!.webview!.hostObjects.api;
}

// ---------- Dev mock (npm run dev / npm run preview в обычном браузере) ----------
// Мок нарочно async везде — так локальная разработка ведёт себя так же, как
// реальный WebView2 host object proxy (там ВСЁ асинхронно, без исключений),
// и класс багов "забыл await" ловится уже на этапе npm run dev, а не в проде.

const mockEmployees: Employee[] = [
  { id: 'e1', name: 'Иванов Иван', hourlyRate: 250, telegramId: 0, salaryThreadId: 0 },
  { id: 'e2', name: 'Петрова Мария', hourlyRate: 230, telegramId: 0, salaryThreadId: 0 },
];
let mockSafe = 1000;
const mockWarehouse: WarehouseItem[] = [
  { name: 'Кофе зерновой 1кг', article: 'A-1001', folder: 'Напитки', stock: 12 },
  { name: 'Стакан 300мл', article: 'A-1002', folder: 'Расходники', stock: 340 },
  { name: 'Сироп ванильный', article: 'A-1003', folder: 'Напитки', stock: 4 },
];

type MockListener = (data: unknown) => void;
const mockListeners = new Set<MockListener>();
function dispatchMock(data: unknown) {
  mockListeners.forEach((l) => l(data));
}
function delay(ms: number) {
  return new Promise((r) => setTimeout(r, ms));
}

const mock: NativeApiHostObject = {
  async GetConstantsAsync() {
    return JSON.stringify({
      initialCashRegister: 1000,
      maxEmployeesPerShift: 3,
      maxHoursPerShift: 12,
      maxAmountDigits: 6,
      maxHoursDigits: 2,
    } satisfies ShiftConstants);
  },
  async GetEmployeesAsync() {
    return JSON.stringify(mockEmployees);
  },
  async AddEmployeeAsync(json) {
    const e = JSON.parse(json) as Employee;
    mockEmployees.push({ ...e, id: `e${mockEmployees.length + 1}` });
    return JSON.stringify({ success: true, message: '' });
  },
  async GetCurrentSalaryAsync() {
    return JSON.stringify({ success: true, currentSalary: 3500, message: '' });
  },
  async GetCurrentSafeAsync() {
    return JSON.stringify({ current: mockSafe });
  },
  async AddSafeComingAsync(json) {
    const op = JSON.parse(json) as { amount: number };
    mockSafe += op.amount;
    return JSON.stringify({ success: true, message: '' });
  },
  async SendAdvanceAsync() {
    return JSON.stringify({ success: true, message: 'mock ok' });
  },
  async SendExpenseAsync() {
    return JSON.stringify({ success: true, message: 'mock ok' });
  },
  async SendExpenseWithPhotoAsync() {
    dispatchMock({ type: 'expenseSendProgress', message: 'Запрашиваю фото…' });
    await delay(400);
    dispatchMock({ type: 'expenseSendProgress', message: 'Фото получено.' });
    await delay(200);
    return JSON.stringify({ success: true, message: 'mock ok' });
  },
  async SendRaportWithPhotoAsync() {
    dispatchMock({ type: 'raportSendProgress', message: 'Запрашиваю фото…' });
    await delay(400);
    dispatchMock({ type: 'raportSendProgress', message: 'Фото получено.' });
    await delay(200);
    dispatchMock({ type: 'raportSendProgress', message: 'Отправляю отчёт…' });
    await delay(200);
    return JSON.stringify({ success: true, message: 'mock ok' });
  },
  async SearchWarehouseItemsAsync(query) {
    const q = query.toLowerCase();
    return JSON.stringify(mockWarehouse.filter((i) => i.name.toLowerCase().includes(q)));
  },
  async GetAllWarehouseItemsAsync() {
    return JSON.stringify({ items: mockWarehouse, lastRefreshedUtc: new Date().toISOString(), error: '' });
  },
};

function api(): NativeApiHostObject {
  return isHosted ? hostApi() : mock;
}

// ---------- Constants ----------

export async function getConstants(): Promise<ShiftConstants> {
  return JSON.parse(await api().GetConstantsAsync());
}

// ---------- Employees ----------

export async function getEmployees(): Promise<Employee[]> {
  return JSON.parse(await api().GetEmployeesAsync());
}

export async function addEmployee(employee: Omit<Employee, 'id'>): Promise<ApiResult> {
  return JSON.parse(await api().AddEmployeeAsync(JSON.stringify(employee)));
}

export async function getCurrentSalary(
  employeeId: string,
): Promise<{ success: boolean; currentSalary: number; message: string }> {
  return JSON.parse(await api().GetCurrentSalaryAsync(employeeId));
}

// ---------- Safe / Coming ----------

export async function getCurrentSafe(): Promise<number> {
  const { current } = JSON.parse(await api().GetCurrentSafeAsync());
  return current;
}

export async function addSafeComing(amount: number, comment: string): Promise<ApiResult> {
  return JSON.parse(await api().AddSafeComingAsync(JSON.stringify({ amount, comment })));
}

export function subscribeToSafeChanges(onChange: (current: number) => void): () => void {
  return onMessage((data) => {
    const d = data as { type?: string; current?: number };
    if (d?.type === 'safeChanged' && typeof d.current === 'number') onChange(d.current);
  });
}

// ---------- Advance ----------

export interface AdvanceRequest {
  employeeId: string;
  amount: number;
  isNonCash: boolean;
  isSalary: boolean;
  extractFromSafe: boolean;
  comment: string;
}

export async function sendAdvance(req: AdvanceRequest): Promise<ApiResult> {
  return JSON.parse(await api().SendAdvanceAsync(JSON.stringify(req)));
}

// ---------- Expense ----------

export interface ExpenseRequest {
  amount: number;
  comment: string;
  fromSafe: boolean;
}

export async function sendExpense(req: ExpenseRequest): Promise<ApiResult> {
  return JSON.parse(await api().SendExpenseAsync(JSON.stringify(req)));
}

export interface ExpenseWithPhotoRequest extends ExpenseRequest {
  employeeId: string;
  senderName?: string;
}

/** Один await-вызов: C# сам запрашивает фото и шлёт расход. Прогресс — через onProgress. */
export async function sendExpenseWithPhoto(
  req: ExpenseWithPhotoRequest,
  onProgress?: (message: string) => void,
): Promise<ApiResult> {
  const unsubscribe = onProgress ? subscribeToProgress('expenseSendProgress', onProgress) : null;
  try {
    return JSON.parse(await api().SendExpenseWithPhotoAsync(JSON.stringify(req)));
  } finally {
    unsubscribe?.();
  }
}

// ---------- Raport ----------

export interface RaportEmployeeSalary {
  employeeId: string;
  hours: number;
  minus: number;
}

export interface RaportRequest {
  factCash: number;
  factNonCash: number;
  programCash: number;
  programNonCash: number;
  factSafe: number;
  whyMinus: string;
  employees: RaportEmployeeSalary[];
}

/** Один await-вызов: C# сам запрашивает фото у первого сотрудника и шлёт отчёт. */
export async function sendRaportWithPhoto(
  req: RaportRequest,
  onProgress?: (message: string) => void,
): Promise<ApiResult> {
  const unsubscribe = onProgress ? subscribeToProgress('raportSendProgress', onProgress) : null;
  try {
    return JSON.parse(await api().SendRaportWithPhotoAsync(JSON.stringify(req)));
  } finally {
    unsubscribe?.();
  }
}

/** Общая подписка на прогресс-сообщения одного типа — без сопоставления по id (сценарий на экране один). */
function subscribeToProgress(type: string, onMessage_: (message: string) => void): () => void {
  return onMessage((data) => {
    const d = data as { type?: string; message?: string };
    if (d?.type === type && typeof d.message === 'string') onMessage_(d.message);
  });
}

// ---------- Warehouse ----------

export async function searchWarehouseItems(query: string, limit = 15): Promise<WarehouseItem[]> {
  return JSON.parse(await api().SearchWarehouseItemsAsync(query, limit));
}

export async function getAllWarehouseItems(): Promise<{
  items: WarehouseItem[];
  lastRefreshedUtc: string;
  error: string;
}> {
  return JSON.parse(await api().GetAllWarehouseItemsAsync());
}

// ---------- Generic message channel ----------

function onMessage(handler: (data: unknown) => void): () => void {
  if (!isHosted) {
    mockListeners.add(handler);
    return () => mockListeners.delete(handler);
  }
  const wrapped = (ev: { data: unknown }) => handler(ev.data);
  window.chrome!.webview!.addEventListener('message', wrapped);
  return () => window.chrome!.webview!.removeEventListener('message', wrapped);
}
