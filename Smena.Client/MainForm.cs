using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Smena.Client.Services;

namespace Smena.Client;

/// <summary>
/// Тонкая C#-оболочка: WebView2 + нативный splash-оверлей на время его инициализации.
/// Вся бизнес/gRPC-логика — в Services/*, не изменена, вызывается через NativeApiBridge.
/// </summary>
internal sealed class MainForm : Form
{
    private readonly ConstantsService constantsService;
    private readonly EmployeeService employeeService;
    private readonly SafeService safeService;
    private readonly RaportService raportService;
    private readonly AdvanceService advanceService;
    private readonly ExpenseService expenseService;
    private readonly PhotoService photoService;
    private readonly WarehouseService warehouseService;
    private readonly GrpcService grpcService;

    private WebView2? webView;
    private NativeApiBridge? bridge;
    private Panel? splash;

    public MainForm(GrpcService grpcService)
    {
        this.grpcService = grpcService;

        constantsService = new(grpcService);
        employeeService = new(grpcService);
        safeService = new(grpcService);
        raportService = new(grpcService);
        advanceService = new(grpcService);
        expenseService = new(grpcService);
        photoService = new(grpcService);
        warehouseService = new(grpcService);

        Text = "Smena.Client";
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ColorTranslator.FromHtml("#12161C");

        BuildSplash();
        Shown += async (_, _) => await InitializeWebViewAsync();
    }

    /// <summary>
    /// Нативный WinForms-оверлей — WebView2 не рисует ничего, пока не отработает
    /// EnsureCoreWebView2Async + первая навигация, так что без этого экрана пользователь
    /// первые ~1-2 секунды видит пустое чёрное окно.
    /// </summary>
    private void BuildSplash()
    {
        splash = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorTranslator.FromHtml("#12161C")
        };

        var label = new Label
        {
            Text = "Smena.Client",
            ForeColor = ColorTranslator.FromHtml("#C08B3E"),
            Font = new Font("Segoe UI", 22f, FontStyle.Regular),
            AutoSize = true
        };
        var sub = new Label
        {
            Text = "Загрузка…",
            ForeColor = ColorTranslator.FromHtml("#8892A0"),
            Font = new Font("Segoe UI", 10f),
            AutoSize = true
        };

        splash.Resize += (_, _) =>
        {
            label.Location = new Point((splash.Width - label.PreferredWidth) / 2, splash.Height / 2 - 30);
            sub.Location = new Point((splash.Width - sub.PreferredWidth) / 2, splash.Height / 2 + 10);
        };

        splash.Controls.Add(label);
        splash.Controls.Add(sub);
        Controls.Add(splash);
        splash.BringToFront();
    }

    private async Task InitializeWebViewAsync()
    {
        webView = new WebView2 { Dock = DockStyle.Fill, Visible = false };
        Controls.Add(webView);

        try
        {
            var fixedRuntimePath = Path.Combine(AppContext.BaseDirectory, "webview2runtime");
            var browserExecutableFolder = Directory.Exists(fixedRuntimePath) ? fixedRuntimePath : null;

            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Smena.Client", "WebView2");

            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: browserExecutableFolder,
                userDataFolder: userDataFolder);

            await webView.EnsureCoreWebView2Async(env);
        }
        catch (Exception ex)
        {
            splash!.Controls.Clear();
            splash.Controls.Add(new Label
            {
                Text = $"Не удалось запустить встроенный браузер (WebView2):\n{ex.Message}\n\n" +
                       "Проверьте WebView2 Runtime или папку webview2runtime рядом с exe.",
                ForeColor = Color.IndianRed,
                Font = new Font("Segoe UI", 10f),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            return;
        }

        var core = webView.CoreWebView2;
        // DevTools отключены — для диагностики они больше не нужны (JS-ошибки ловит
        // ErrorBoundary, транспортные — Engine тостами, C#-ошибки пишутся в errors.log).
        core.Settings.AreDevToolsEnabled = false;
        // Браузерные хоткеи отключены: F5/Ctrl+R перезагружали страницу WebView2 —
        // JS терял запущенный сценарий (например, отправку расхода с фото), а C#
        // доводил его до конца: риск двойного проведения операции.
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = false;

        var webUiFolder = Path.Combine(AppContext.BaseDirectory, "webui");
        core.SetVirtualHostNameToFolderMapping(
            "app.local", webUiFolder, CoreWebView2HostResourceAccessKind.Allow);

        bridge = new NativeApiBridge(
            constantsService, employeeService, safeService, advanceService, expenseService,
            raportService, photoService, warehouseService)
        {
            Core = core
        };
        bridge.OnPostMessage += json =>
        {
            if (InvokeRequired) Invoke(() => core.PostWebMessageAsJson(json));
            else core.PostWebMessageAsJson(json);
        };
        core.AddHostObjectToScript("api", bridge);

        WireSafePushEvents(core);

        core.NavigationCompleted += (_, _) =>
        {
            webView.Visible = true;
            splash?.Hide();
        };

        safeService.StartSubscription();

        core.Navigate("https://app.local/index.html");
    }

    private void WireSafePushEvents(CoreWebView2 core)
    {
        safeService.SafeChanged += (_, current) =>
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new { type = "safeChanged", current });
            if (InvokeRequired) Invoke(() => core.PostWebMessageAsJson(payload));
            else core.PostWebMessageAsJson(payload);
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        safeService.Dispose();
        grpcService.Dispose();
        webView?.Dispose();
        base.OnFormClosed(e);
    }
}
