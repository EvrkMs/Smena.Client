using System.Runtime.InteropServices;
using System.Text.Json;
using Host.Grpc.Common;
using Host.Grpc.Services.Advance;
using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Expense;
using Host.Grpc.Services.Raport;
using Host.Grpc.Services.Safe;
using Host.Grpc.Services.Warehouse;
using Microsoft.Web.WebView2.Core;
using Smena.Client.Services;

namespace Smena.Client;

/// <summary>
/// Публикуется в JS через CoreWebView2.AddHostObjectToScript("api", ...).
/// Граница JSON: каждый метод принимает/возвращает строку JSON.
///
/// ВАЖНОЕ ПРАВИЛО (нашли на практике): в WebView2 АБСОЛЮТНО ЛЮБОЙ вызов через
/// chrome.webview.hostObjects.api.* на JS-стороне — это Promise, даже если C#-метод
/// объявлен как синхронный `string Foo()`, а не `Task&lt;string&gt;`. Первая версия моста
/// возвращала "request id" синхронной строкой из StartPhotoRequest и ожидала, что JS
/// получит именно строку — JS получал Promise, сравнение id проваливалось молча,
/// push-сообщение доходило до WebView2, но не находило совпадения на JS-стороне.
///
/// Поэтому здесь принцип: КАЖДЫЙ метод, пересекающий границу — Task&lt;string&gt;, и
/// весь сценарий "запросить фото → дождаться → отправить" сделан ОДНИМ await-вызовом
/// с C#-стороны (SendRaportWithPhotoAsync/SendExpenseWithPhotoAsync), а не парой
/// "начать (sync) + подписаться на push по id (async)". Промежуточный прогресс всё
/// ещё идёт через PostWebMessageAsJson, но UI просто показывает последний текст,
/// не сопоставляя id — на экране одновременно идёт только один такой сценарий,
/// сопоставление по id было лишним источником багов, а не необходимостью.
/// </summary>
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class NativeApiBridge(
    EmployeeService employeeService,
    SafeService safeService,
    AdvanceService advanceService,
    ExpenseService expenseService,
    RaportService raportService,
    PhotoService photoService,
    WarehouseService warehouseService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Назначается в MainForm сразу после EnsureCoreWebView2Async.</summary>
    public CoreWebView2? Core { get; set; }

    /// <summary>MainForm подписывается и делает core.PostWebMessageAsJson в UI-потоке.</summary>
    public event Action<string>? OnPostMessage;

    // ---------- Constants ----------

    public Task<string> GetConstantsAsync() => Task.FromResult(JsonSerializer.Serialize(new
    {
        initialCashRegister = ShiftConstants.InitialCashRegister,
        maxEmployeesPerShift = ShiftConstants.MaxEmployeesPerShift,
        maxHoursPerShift = ShiftConstants.MaxHoursPerShift,
        maxAmountDigits = ShiftConstants.MaxAmountDigits,
        maxHoursDigits = ShiftConstants.MaxHoursDigits
    }, JsonOptions));

    // ---------- Employees ----------

    public async Task<string> GetEmployeesAsync()
    {
        await employeeService.LoadOrReloadListAsync();
        var dto = employeeService.Employees.Select(e => new
        {
            id = e.Id,
            name = e.Name,
            hourlyRate = e.HourlyRate,
            telegramId = e.TelegramId,
            salaryThreadId = e.SalaryThreadId
        });
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public async Task<string> AddEmployeeAsync(string employeeJson)
    {
        var input = JsonSerializer.Deserialize<EmployeeDto>(employeeJson, JsonOptions)
            ?? throw new ArgumentException("Invalid employee payload.");

        var employee = new GrpcEmployee
        {
            Name = input.Name,
            HourlyRate = input.HourlyRate,
            TelegramId = input.TelegramId,
            SalaryThreadId = input.SalaryThreadId
        };

        var (success, message) = await employeeService.AddEmployeeAsync(employee);
        return JsonSerializer.Serialize(new { success, message }, JsonOptions);
    }

    public async Task<string> GetCurrentSalaryAsync(string employeeId)
    {
        var (success, currentSalary, message) = await employeeService.GetCurrentSalaryAsync(employeeId);
        return JsonSerializer.Serialize(new { success, currentSalary, message }, JsonOptions);
    }

    // ---------- Safe / Coming ----------

    public async Task<string> GetCurrentSafeAsync()
    {
        var current = await safeService.RefreshCurrentSafeAsync();
        return JsonSerializer.Serialize(new { current }, JsonOptions);
    }

    public async Task<string> AddSafeComingAsync(string operationJson)
    {
        var input = JsonSerializer.Deserialize<SafeComingDto>(operationJson, JsonOptions)
            ?? throw new ArgumentException("Invalid safe operation payload.");

        var request = new SafeOperationAdd
        {
            Amount = input.Amount,
            Comment = input.Comment ?? string.Empty,
            Type = SafeOperationTypeGrpc.Coming
        };

        var response = await safeService.AddOperationSafeAsync(request);
        return JsonSerializer.Serialize(new { success = response.Success, message = response.Message }, JsonOptions);
    }

    // ---------- Advance ----------

    public async Task<string> SendAdvanceAsync(string requestJson)
    {
        var input = JsonSerializer.Deserialize<AdvanceDto>(requestJson, JsonOptions)
            ?? throw new ArgumentException("Invalid advance payload.");

        var request = new GrpcAdvanceRequest
        {
            EmployeeId = input.EmployeeId,
            Amount = input.Amount,
            IsNonCash = input.IsNonCash,
            IsSalary = input.IsSalary,
            ExtractFromSafe = input.ExtractFromSafe,
            Comment = input.Comment ?? string.Empty
        };

        var (success, message) = await advanceService.SendAdvanceAsync(request);
        return JsonSerializer.Serialize(new { success, message }, JsonOptions);
    }

    // ---------- Expense ----------

    /// <summary>Расход без фото — прямой одношаговый вызов.</summary>
    public async Task<string> SendExpenseAsync(string requestJson)
    {
        var input = JsonSerializer.Deserialize<ExpenseDto>(requestJson, JsonOptions)
            ?? throw new ArgumentException("Invalid expense payload.");

        var (success, message) = await expenseService.AddExpenseOperationAsync(new GrpcExpenseAdd
        {
            Amount = input.Amount,
            Comment = input.Comment ?? string.Empty,
            FromSafe = input.FromSafe,
            IsNonCash = !input.FromSafe,
            SendPhoto = false
        });
        return JsonSerializer.Serialize(new { success, message }, JsonOptions);
    }

    /// <summary>Расход С фото: запрос фото у сотрудника и отправка — один await-сценарий на C#-стороне.</summary>
    public async Task<string> SendExpenseWithPhotoAsync(string requestJson)
    {
        var input = JsonSerializer.Deserialize<ExpenseWithPhotoDto>(requestJson, JsonOptions)
            ?? throw new ArgumentException("Invalid expense payload.");

        void Report(string message) => PostMessage(new { type = "expenseSendProgress", message });

        Report("Запрашиваю фото…");
        var photo = await photoService.RequestPhotosAsync(input.EmployeeId, Report);
        if (!photo.Success || string.IsNullOrWhiteSpace(photo.SessionKey))
            return JsonSerializer.Serialize(new { success = false, message = photo.Message }, JsonOptions);

        Report("Отправляю расход…");
        var (success, message) = await expenseService.AddExpenseOperationAsync(new GrpcExpenseAdd
        {
            Amount = input.Amount,
            Comment = input.Comment ?? string.Empty,
            FromSafe = input.FromSafe,
            IsNonCash = !input.FromSafe,
            SendPhoto = true,
            PhotoSessionKey = photo.SessionKey,
            SenderName = input.SenderName ?? string.Empty
        });
        return JsonSerializer.Serialize(new { success, message }, JsonOptions);
    }

    // ---------- Raport ----------
    // Отчёт ВСЕГДА идёт с фото (см. Raport.tsx — первый сотрудник смены подтверждает фото),
    // поэтому единственный метод — "с фото", без отдельного "быстрого" варианта.

    public async Task<string> SendRaportWithPhotoAsync(string requestJson)
    {
        var input = JsonSerializer.Deserialize<RaportDto>(requestJson, JsonOptions)
            ?? throw new ArgumentException("Invalid raport payload.");

        if (input.Employees.Count == 0)
            return JsonSerializer.Serialize(new { success = false, message = "Не выбрано ни одного сотрудника." }, JsonOptions);

        void Report(string message) => PostMessage(new { type = "raportSendProgress", message });

        Report("Запрашиваю фото…");
        var photo = await photoService.RequestPhotosAsync(input.Employees[0].EmployeeId, Report);
        if (!photo.Success || string.IsNullOrWhiteSpace(photo.SessionKey))
            return JsonSerializer.Serialize(new { success = false, message = photo.Message }, JsonOptions);

        Report("Отправляю отчёт…");
        var request = new GrpcRaportRequest
        {
            FactCash = input.FactCash,
            FactNonCash = input.FactNonCash,
            ProgramCash = input.ProgramCash,
            ProgramNonCash = input.ProgramNonCash,
            FactSafe = input.FactSafe,
            WhyMinus = input.WhyMinus ?? string.Empty,
            SendPhoto = true,
            PhotoSessionKey = photo.SessionKey
        };
        request.Employees.AddRange(input.Employees.Select(e => new EmployeeRaportSalary
        {
            EmployeeId = e.EmployeeId,
            Hours = e.Hours,
            Minus = e.Minus
        }));

        var (success, message) = await raportService.SendRaportAsync(request);
        return JsonSerializer.Serialize(new { success, message }, JsonOptions);
    }

    private void PostMessage(object payload) =>
        OnPostMessage?.Invoke(JsonSerializer.Serialize(payload, JsonOptions));

    // ---------- Warehouse (Пересчёт / МойСклад) ----------

    public async Task<string> SearchWarehouseItemsAsync(string query, int limit)
    {
        var items = await warehouseService.SearchItemsAsync(query, limit);
        return JsonSerializer.Serialize(items.Select(ToItemDto), JsonOptions);
    }

    public async Task<string> GetAllWarehouseItemsAsync()
    {
        var (items, lastRefreshedUtc, error) = await warehouseService.GetAllItemsAsync();
        return JsonSerializer.Serialize(new { items = items.Select(ToItemDto), lastRefreshedUtc, error }, JsonOptions);
    }

    private static object ToItemDto(GrpcWarehouseItem i) =>
        new { name = i.Name, article = i.Article, folder = i.Folder, stock = i.Stock };

    // ---------- DTO ----------

    private sealed record EmployeeDto(string Name, int HourlyRate, long TelegramId, int SalaryThreadId);
    private sealed record SafeComingDto(int Amount, string? Comment);
    private sealed record AdvanceDto(string EmployeeId, int Amount, bool IsNonCash, bool IsSalary, bool ExtractFromSafe, string? Comment);
    private sealed record ExpenseDto(int Amount, string? Comment, bool FromSafe);
    private sealed record ExpenseWithPhotoDto(int Amount, string? Comment, bool FromSafe, string EmployeeId, string? SenderName);
    private sealed record EmployeeRaportSalaryDto(string EmployeeId, int Hours, int Minus);
    private sealed record RaportDto(
        int FactCash, int FactNonCash, int ProgramCash, int ProgramNonCash, int FactSafe,
        string? WhyMinus, List<EmployeeRaportSalaryDto> Employees);
}
