using Host.Grpc.Services.Employee;
using MaterialSkin;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client;

internal partial class MainForm : MaterialForm
{
    private readonly EmployeeService employeeService;
    private readonly SafeService safeService;
    private readonly RaportService raportService;
    private readonly AdvanceService advanceService;
    private readonly ExpenseService expenseService;
    private readonly InventoryService inventoryService;
    private readonly PhotoService photoService;
    private readonly GrpcService grpcService;

    public MainForm(GrpcService grpcService)
    {
        this.grpcService = grpcService;

        employeeService = new(grpcService);
        safeService = new(grpcService);
        raportService = new(grpcService);
        advanceService = new(grpcService);
        expenseService = new(grpcService);
        inventoryService = new(grpcService);
        photoService = new(grpcService);

        InitializeComponent();

        raportUserControl.Initialize(employeeService, safeService, raportService, photoService);
        advanceUserControl1.Initialize(employeeService, advanceService);
        expenseUserControl1.Initialize(employeeService, expenseService, photoService);
        comingUserControl1.Initialize(safeService);
        inventoryUserControl1.Initialize(employeeService, inventoryService);
        AddEmployeesTab();

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
        advanceUserControl1.UnsubscribeFromEvents();
        expenseUserControl1.UnsubscribeFromEvents();
        comingUserControl1.UnsubscribeFromEvents();
        inventoryUserControl1.UnsubscribeFromEvents();
        safeService.Dispose();
        grpcService.Dispose();

        base.OnFormClosed(e);
    }

    private void AddEmployeesTab()
    {
        var tab = new TabPage("Сотрудники")
        {
            BackColor = Color.FromArgb(40, 40, 40)
        };

        var listBox = new ListBox
        {
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 12F),
            Location = new Point(24, 24),
            Size = new Size(420, 520),
        };

        var nameBox = new MaterialTextBox
        {
            Hint = "Имя",
            Location = new Point(480, 24),
            Size = new Size(320, 50),
        };

        var hourlyRateBox = new MaterialTextBox
        {
            Hint = "Ставка (руб/час)",
            Location = new Point(480, 90),
            Size = new Size(320, 50),
        };

        var telegramIdBox = new MaterialTextBox
        {
            Hint = "Telegram ID",
            Location = new Point(480, 156),
            Size = new Size(320, 50),
        };

        var salaryThreadBox = new MaterialTextBox
        {
            Hint = "Salary thread ID",
            Location = new Point(480, 222),
            Size = new Size(320, 50),
        };

        var addButton = new MaterialButton
        {
            Text = "Добавить",
            Location = new Point(480, 296),
            Size = new Size(140, 36),
        };

        void RefreshList()
        {
            listBox.DataSource = null;
            listBox.DisplayMember = nameof(GrpcEmployee.Name);
            listBox.DataSource = employeeService.Employees.ToList();
        }

        employeeService.Employees.ListChanged += (_, __) =>
        {
            if (InvokeRequired)
            {
                Invoke(RefreshList);
            }
            else
            {
                RefreshList();
            }
        };

        RefreshList();

        addButton.Click += async (_, __) =>
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Введите имя сотрудника.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(hourlyRateBox.Text, out var hourlyRate) || hourlyRate < 0)
            {
                MessageBox.Show("Введите корректную ставку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            long telegramId = 0;
            if (!string.IsNullOrWhiteSpace(telegramIdBox.Text) &&
                !long.TryParse(telegramIdBox.Text, out telegramId))
            {
                MessageBox.Show("Некорректный Telegram ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int salaryThreadId = 0;
            if (!string.IsNullOrWhiteSpace(salaryThreadBox.Text) &&
                !int.TryParse(salaryThreadBox.Text, out salaryThreadId))
            {
                MessageBox.Show("Некорректный Salary thread ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var employee = new GrpcEmployee
            {
                Name = nameBox.Text.Trim(),
                HourlyRate = hourlyRate,
                TelegramId = telegramId,
                SalaryThreadId = salaryThreadId
            };

            addButton.Enabled = false;
            try
            {
                var ok = await employeeService.AddEmployeeAsync(employee);
                if (ok)
                {
                    nameBox.Clear();
                    hourlyRateBox.Clear();
                    telegramIdBox.Clear();
                    salaryThreadBox.Clear();
                }
            }
            finally
            {
                addButton.Enabled = true;
            }
        };

        tab.Controls.Add(listBox);
        tab.Controls.Add(nameBox);
        tab.Controls.Add(hourlyRateBox);
        tab.Controls.Add(telegramIdBox);
        tab.Controls.Add(salaryThreadBox);
        tab.Controls.Add(addButton);

        materialTabControl1.TabPages.Add(tab);
    }
}
