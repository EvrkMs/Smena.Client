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
    private readonly PhotoService photoService;
    private readonly GrpcService grpcService;
    private readonly FormCacheService formCache;

    public MainForm(GrpcService grpcService, FormCacheService formCache)
    {
        this.grpcService = grpcService;
        this.formCache = formCache;

        employeeService = new(grpcService);
        safeService = new(grpcService);
        raportService = new(grpcService);
        advanceService = new(grpcService);
        expenseService = new(grpcService);
        photoService = new(grpcService);

        InitializeComponent();

        raportUserControl.Initialize(employeeService, safeService, raportService, photoService, formCache);
        advanceUserControl1.Initialize(employeeService, advanceService, formCache);
        expenseUserControl1.Initialize(employeeService, expenseService, photoService, formCache);
        comingUserControl1.Initialize(safeService, formCache);
        AddEmployeesTab();

        materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.EnforceBackcolorOnAllComponents = true;
        materialSkinManager.AddFormToManage(this);
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(
            Primary.DeepPurple700,
            Primary.DeepPurple900,
            Primary.DeepPurple400,
            Accent.Cyan700,
            TextShade.WHITE
        );

        Shown += OnShownAsync;
    }

    private async void OnShownAsync(object? sender, EventArgs e)
    {
        // Load data asynchronously after the form is shown to avoid blocking the UI thread.
        try
        {
            await employeeService.LoadOrReloadListAsync();
        }
        catch { /* list stays empty; user can retry */ }

        raportUserControl.EnableCache();
        advanceUserControl1.EnableCache();
        expenseUserControl1.EnableCache();
        comingUserControl1.EnableCache();

        try
        {
            await safeService.RefreshCurrentSafeAsync();
        }
        catch { /* safe stays 0; explicit refresh will update later */ }

        safeService.StartSubscription();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        raportUserControl.UnsubscribeFromEvents();
        advanceUserControl1.UnsubscribeFromEvents();
        expenseUserControl1.UnsubscribeFromEvents();
        comingUserControl1.UnsubscribeFromEvents();
        formCache.Dispose();
        safeService.Dispose();
        grpcService.Dispose();

        base.OnFormClosed(e);
    }

    private void AddEmployeesTab()
    {
        var tab = new TabPage("Сотрудники")
        {
            BackColor = Color.FromArgb(30, 18, 80)
        };

        var listBox = new ListBox
        {
            BackColor = Color.FromArgb(35, 25, 75),
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
                var (success, message) = await employeeService.AddEmployeeAsync(employee);
                if (success)
                {
                    nameBox.Clear();
                    hourlyRateBox.Clear();
                    telegramIdBox.Clear();
                    salaryThreadBox.Clear();
                }
                else
                {
                    MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
