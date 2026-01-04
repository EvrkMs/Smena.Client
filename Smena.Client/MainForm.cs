using Host.Grpc.Services.Employee;
using MaterialSkin;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;
using System.Xml.Linq;

namespace Smena.Client;

internal partial class MainForm : MaterialForm
{
    private readonly EmployeeService employeeService;
    private readonly SafeService safeService;
    private readonly GrpcService grpcService;

    public static readonly GrpcEmployee NullComboBox = new() { Name = "Нет" };

    public MainForm()
    {
        grpcService = new("http://192.168.88.254:5000");
        employeeService = new(grpcService.Channel);
        safeService = new(grpcService.Channel);

        InitializeComponent();

        raportUserControl.Initialize(employeeService, safeService);

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
        raportUserControl.UnsubscribeFromEvents();

        base.OnFormClosed(e);
    }
}
