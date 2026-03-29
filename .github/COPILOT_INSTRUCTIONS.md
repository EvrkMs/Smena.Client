# Инструкция по разработке Smena.Client для GitHub Copilot

## 1. Технологический стек
- **Платформа**: .NET 10 (`net10.0-windows`)
- **Тип проекта**: Windows Forms (WinForms)
- **Отдельная сборка**: Компилируется в один `.exe` (не используем интерфейсы или DI-контейнер).
- **UI Библиотека**: `MaterialSkin.2` (v2.3.1).
- **Связь с сервером**: gRPC (`Grpc.Net.Client`, `Google.Protobuf`).
- **Конфигурация**: `Microsoft.Extensions.Configuration` (`appsettings.json` + `EnvironmentVariables`).
- **Хранение данных**: Встроенный `System.Text.Json` (пакет Newtonsoft обойдем стороной) для локального кеширования состояний окон в `%LOCALAPPDATA%/Smena.Client/form-cache.json`.

## 2. Архитектура и Структура папок
- `Program.cs` — точка входа, конфигурация, обработчики глобальных ошибок.
- `MainForm.cs` — главное окно с вкладками (`MaterialTabControl`). Работает как оркестратор всех UserControl.
- `components/` — части интерфейса (каждая вкладка — это отдельный `UserControl`). **Никогда** не помещать весь код в один файл. Используем `partial class` для разделения логики (пример: `RaportUserControl.cs`, `RaportUserControl.Events.cs`, `RaportUserControl.Cache.cs`, `RaportUserControl.Validation.cs`). Не редактировать руками файлы `*.Designer.cs` для добавления логики.
- `services/` — классы-обертки над gRPC клиентами. Каждый сервис отвечает за одну Proto-службу (напр. `SafeService`, `EmployeeService`). Использовать primary constructors.
- `Models/` — чистые структуры данных (DTO) без логики (`ReportData.cs`, `EmployeeHoursList.cs`).
- `Helpers/` — переиспользуемые статические методы, например, `GrpcCallHelper.cs` (для оборачивания gRPC-вызовов и отлова ошибок), `EmployeeComboHelper.cs` (для перезагрузки списков сотрудников).
- `ShiftConstants.cs` — все магические числа (лимиты времени, максимальные суммы, таймауты). **Не** использовать константы в коде напрямую.

## 3. Правила написания кода

### 3.1. Кеширование форм (FormCacheService)
- Состояние инпутов на форме должно сохраняться при случайном закрытии приложения по принципу "сохраняй на каждое изменение" (напр. в событии `TextChanged`).
- Сохранение не должно триггериться до полной загрузки всех списков с сервера.
- У каждого UserControl есть методы:
  - `EnableCache()` — вызывается из MainForm **после** того как списки `Employee` загружены с сервера. В нем вызывается `RestoreCachedValues()`, а затем флаг `_cacheEnabled = true`.
  - `SaveFieldToCache()` — всегда проверяет `if (!_cacheEnabled || formCache == null) return;`.
- Идентификаторы (GUID) сотрудников сохраняем/восстанавливаем вместо их имен.
- Для `ComboBox` метод `RestoreCachedEmployee` должен искать `emp.Id` в `comboBox.Items`.

### 3.2. Коммуникация с сервером (gRPC)
- Обертки над gRPC вызовами в сервисах (напр., `AdvanceService`, `ExpenseService`) не должны содержать собственные блоки `try-catch`.
- Всегда использовать метод-обертку `GrpcCallHelper.CallAsync()` для обработки исключений gRPC (он возвращает кортеж `(bool Success, string Message)`).
- Не использовать фоновые потоки без `CancellationToken`.

### 3.3. UI и Многопоточность (WinForms + Async)
- Строго соблюдаем разделение UI-потока.
- При получении событий из фоновых потоков (напр. от gRPC стрима `SubscribeSafe`), обновлять UI элементы нужно через проверку `InvokeRequired`:
```csharp
if (InvokeRequired)
{
    Invoke(() => MethodName(sender, e));
    return;
}
```
- Избегать `async void`, за исключением обработчиков событий (например: `private async void OnSendClick(object? sender, EventArgs e)`).
- Блокирующие/идут долго по сети операции — только через `await`. Кнопки (напр. кнопку отправки) на время операции выключать (`Enabled = false`) и включать только в блоке `finally`.

### 3.4. Визуальный стиль
- При создании новых элементов UI:
  - Используем палитру `DeepPurple` + `Cyan` акцент: `Color.FromArgb(30, 18, 80)` для фонов вкладок, `Color.FromArgb(35, 25, 75)` для фонов `ListBox`.
  - Избегать ярких красных/желтых цветов, если это не уведомление об ошибке.
  - Для кнопок целевого действия устанавливать `UseAccentColor = true`.

### 3.5. Запрещенные паттерны:
- ❌ Dependency Injection (через конструкторы прокидывается вручную).
- ❌ Интерфейсы (`ISomeService`) в рамках клиентского приложения, нет mocking'а.
- ❌ Юнит-тесты не требуются (пока явно не запрошены).
- ❌ Ручное парсинг CSV / XML. Только `System.Text.Json`.

## 4. Сценарий работы над фичей
1. Понять цель: работаем с формой (UI), либо с запросом к серверу (Services).
2. Вынести константы в `ShiftConstants.cs`.
3. Добавить логику в `UserControl`. Если файл стал больше 300 строк — разбить через `partial`.
4. Подписать элемент на кеширование, и проверить, что `_cacheEnabled` препятствует стиранию при первичной инициализации.
5. Вызвать бизнес-логику через сервис (`Task<(bool, string)>`), обработать ответ в UI `MessageBox`'ом.
