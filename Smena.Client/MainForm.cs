using MaterialSkin;
using MaterialSkin.Controls;
using Smena.Client.Services;

namespace Smena.Client;

internal partial class MainForm : MaterialForm
{
    private readonly EmployeeService employeeService;
    private readonly SafeService safeService;
    private readonly RaportService raportService;
    private readonly AdvanceService advanceService;
    private readonly ExpenseService expenseService;
    private readonly InventoryService inventoryService;
    private readonly GrpcService grpcService;

    public MainForm()
    {
        // Инициализация gRPC канала
        grpcService = new("http://localhost:5000");

        // Инициализация всех сервисов
        employeeService = new(grpcService.Channel);
        safeService = new(grpcService.Channel);
        raportService = new(grpcService.Channel);
        advanceService = new(grpcService.Channel);
        expenseService = new(grpcService.Channel);
        inventoryService = new(grpcService.Channel);

        InitializeComponent();

        // Инициализация всех UserControl'ов
        raportUserControl.Initialize(employeeService, safeService, raportService);
        advanceUserControl1.Initialize(employeeService, advanceService);
        expenseUserControl1.Initialize(employeeService, expenseService);
        comingUserControl1.Initialize(safeService);
        inventoryUserControl1.Initialize(employeeService, inventoryService);

        // Настройка Material Design темы
        materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.EnforceBackcolorOnAllComponents = true;
        materialSkinManager.AddFormToManage(this);
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(
            Primary.Purple800,
            Primary.Purple900,
            Primary.Purple500,
            Accent.Lime700,
            TextShade.WHITE
        );
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Отписываемся от всех событий перед закрытием
        raportUserControl.UnsubscribeFromEvents();
        advanceUserControl1.UnsubscribeFromEvents();
        expenseUserControl1.UnsubscribeFromEvents();
        comingUserControl1.UnsubscribeFromEvents();
        inventoryUserControl1.UnsubscribeFromEvents();

        base.OnFormClosed(e);
    }
}