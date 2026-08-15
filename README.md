# Smena.Client

Десктопный клиент кассира для [Smena](https://github.com/EvrkMs/Smena) — сервера, который закрывает смены на кассе, считает ЗП/сейф/безнал и шлёт отчёты в Telegram. Этот репозиторий — только клиентская часть; вся бизнес-логика и хранение данных на сервере, клиент лишь собирает ввод кассира и вызывает сервер по gRPC.

## Устройство

Тонкая WinForms-оболочка (`MainForm`) хостит **WebView2** с интерфейсом на React/TypeScript (`Smena.Client/reactapp`) — вся вёрстка и UI-логика там. WinForms-часть не рисует ничего, кроме splash-экрана на время инициализации WebView2.

Граница между C# и JS — `NativeApiBridge`, опубликованный в `chrome.webview.hostObjects.api` через `CoreWebView2.AddHostObjectToScript`. Правило моста: каждый метод, пересекающий границу, — `Task<string>` с JSON внутри (в WebView2 любой вызов хост-объекта из JS — Promise, даже для синхронных C#-методов; смешивать sync-возврат с async push по общему id — источник багов, наступили на практике, см. комментарий в `NativeApiBridge.cs`).

Сами вызовы к серверу — в `Smena.Client/Services/*` (`AdvanceService`, `ConstantsService`, `EmployeeServices`, `ExpenseService`, `PhotoService`, `RaportService`, `SafeService`, `WarehouseService`) через `GrpcService` и контракты в `Smena.Client/Protos` — это копия `.proto`-файлов из серверного репозитория [Smena](https://github.com/EvrkMs/Smena) (`Smena/Protos`), синхронизируется вручную при изменении контракта на сервере.

Константы смены (лимиты часов/сотрудников, начальная касса и т.п.) приходят с сервера через `GrpcConstantsService`; локальный `ShiftConstants.cs` — только фолбэк на случай недоступности сервера.

## Подключение к серверу

При первом запуске, если адрес сервера нигде не найден, `ConnectionSetupForm` спрашивает его у пользователя и сохраняет в `%LOCALAPPDATA%\Smena.Client\appsettings.json` — этот файл подключается поверх `appsettings.json` рядом с exe и перекрывает его. Адрес/ключ API можно задать также через переменные окружения (`Grpc__Address`, `API_KEY`/`AVA_SMENA_API_KEY`, либо `PRIMARY_DOMAIN`+`GRPC_PORT` для сборки адреса).

## Структура репозитория

```
Smena.Client/
  Program.cs                — точка входа, разрешение адреса сервера/ключа API
  MainForm.cs                — WinForms-хост WebView2 + splash
  NativeApiBridge.cs         — граница C#/JS, публикуется в WebView2
  GrpcService.cs              — gRPC-канал к серверу
  Services/                   — вызовы конкретных gRPC-сервисов сервера
  Protos/                     — копия контрактов сервера
  Helpers/                    — вспомогательное (лог ошибок, обёртка gRPC-вызовов)
  reactapp/                   — UI на React + TypeScript + Vite
    src/pages/                — экраны (Raport, Advance, Expense, Coming, Employees, Stockcount)
    src/bridge/                — обёртка над chrome.webview.hostObjects.api
    src/components/            — общие компоненты (ErrorBoundary, SafeTicker, Splash)
TODO.md                       — открытый бэклог по клиенту
```

## Разработка

Бэкенд UI — обычный Vite-проект:

```bash
cd Smena.Client/reactapp
npm install
npm run dev
```

Сама форма кассира — обычный .NET WinForms-проект (`Smena.Client/Smena.Client.csproj`). При сборке MSBuild сам собирает `reactapp` (`npm ci` при отсутствии `node_modules`, затем `npm run build`) и копирует `reactapp/dist` в `$(OutDir)webui` — оттуда WebView2 подхватывает `https://app.local/index.html` через `SetVirtualHostNameToFolderMapping`. Отдельно гонять `npm run build` вручную не нужно, `dotnet build`/`dotnet run` делают это сами.
