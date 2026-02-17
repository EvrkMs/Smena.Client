using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Raport;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class RaportUserControl : UserControl
{
    #region Nested Types
    public class EmployeeHoursList
    {
        public GrpcEmployee? Employee { get; set; }
        public int Hours { get; set; }
        public int Minus { get; set; }
        public int HourlyRate => Employee?.HourlyRate ?? 0;
        public int Salary => (Hours * HourlyRate) - Minus;

        public string SalaryInfo => this.ToString();

        public override string ToString() =>
            Employee?.Name != null ? $"{Employee.Name} - {Salary} руб." : "";
    }

    public class ReportData
    {
        public DateTime Date { get; set; }
        public int FactCash { get; set; }
        public int FactNonCash { get; set; }
        public int Revenue { get; set; }
        public int Total { get; set; }
        public int CashDiscrepancy { get; set; }
        public int SafeDiscrepancy { get; set; }
        public int FactSafe { get; set; }
        public int NewSafe { get; set; }
        public int ProgramCash { get; set; }
        public int ProgramNonCash { get; set; }
        public long ProgramSafe { get; set; }
        public List<EmployeeHoursList> Employees { get; set; } = [];
        public int TotalSalary { get; set; }

        public const int InitialCash = 1000;
    }
    #endregion

    #region Fields

    private EmployeeService? employeeService;
    private SafeService? safeService;
    private RaportService? raportService;
    private PhotoService? photoService;
    private readonly List<ComboBox> comboBoxes = [];
    private readonly List<MaterialTextBox> hoursTextBoxes = [];
    private readonly List<MaterialTextBox> minusTextBoxes = [];
    private readonly List<MaterialTextBox> numericTextBoxes = [];
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };
    private readonly BindingList<EmployeeHoursList> employeesData = [];
    private bool isUpdating = false;
    private bool isNormalizingInput = false;
    private readonly Dictionary<MaterialTextBox, string> previousHoursValues = [];
    private readonly Dictionary<string, int> employeeSalaryCache = new(StringComparer.Ordinal);
    private bool isValidatingHours = false;
    private bool isValidatingMinus = false;

    #endregion

    #region Constructors

    public RaportUserControl()
    {
        InitializeComponent();
        InitializeCollections();
    }

    #endregion

    #region Public API

    public void Initialize(EmployeeService employeeService, SafeService safeService, RaportService raportService, PhotoService photoService)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(safeService);
        ArgumentNullException.ThrowIfNull(raportService);
        ArgumentNullException.ThrowIfNull(photoService);

        this.employeeService = employeeService;
        this.safeService = safeService;
        this.raportService = raportService;
        this.photoService = photoService;

        SubscribeToEvents();
    }

    public void UnsubscribeFromEvents()
    {
        employeeService?.Employees.ListChanged -= Event_ListEmployeeChange;
        safeService?.SafeChanged -= Event_SafeChanged;
        UnsubscribeComboBoxes();
        UnsubscribeNumericTextBoxes();

        buttonСalculate.Click -= OnCalculateClick;
        buttonSend.Click -= OnSendClick;
    }

    public IReadOnlyList<EmployeeHoursList> GetEmployeesData() => employeesData;

    public ReportData GenerateReport()
    {
        var report = new ReportData
        {
            Date = DateTime.Now,
            FactCash = int.TryParse(textBoxFactCash.Text, out var fc) ? fc : 0,
            FactNonCash = int.TryParse(textBoxFactNonCash.Text, out var fnc) ? fnc : 0,

            ProgramCash = int.TryParse(textBoxProgramCash.Text, out var pc) ? pc : 0,
            ProgramNonCash = int.TryParse(textBoxProgramNonCash.Text, out var pnc) ? pnc : 0,
            ProgramSafe = safeService?.CurrentSafe ?? 0,

            FactSafe = int.TryParse(textBoxSafe.Text, out var fs) ? fs : 0
        };
        report.NewSafe = report.FactSafe + (report.FactCash - ReportData.InitialCash);

        int safeDiscrepancy = report.FactSafe - (int)report.ProgramSafe;
        report.SafeDiscrepancy = safeDiscrepancy > 0 ? 0 : safeDiscrepancy;

        report.Revenue = (report.FactCash - ReportData.InitialCash) + report.FactNonCash;
        report.TotalSalary = employeesData.Sum(e => e.Salary);
        report.Total = report.Revenue - report.TotalSalary;

        int discrepancy = (report.FactCash + report.FactNonCash) - (report.ProgramCash + report.ProgramNonCash);
        report.CashDiscrepancy = discrepancy < 0 ? discrepancy : 0;

        report.Employees = [.. employeesData];

        return report;
    }

    #endregion

    #region Event Handlers

    private async void OnCalculateClick(object? sender, EventArgs e)
    {
        if (employeesData.Count == 0)
        {
            MessageBox.Show(
                "Выберите хотя бы одного сотрудника!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var report = GenerateReport();
        if (!ValidateMinusTotals(report, showMessage: true))
        {
            return;
        }

        if (!await ValidateEmployeeMinusRulesAsync(showMessage: true))
        {
            return;
        }

        UpdateReportPreview(null, EventArgs.Empty);

        MessageBox.Show(
            "Расчёт выполнен успешно! Проверьте данные и нажмите 'Отправить' для отправки отчёта.",
            "Расчёт завершён",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private async void OnSendClick(object? sender, EventArgs e)
    {
        if (employeesData.Count == 0)
        {
            MessageBox.Show(
                "Выберите хотя бы одного сотрудника!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var report = GenerateReport();

        // Показываем подтверждение с итоговыми данными
        var totalMinusCash = Math.Abs(report.CashDiscrepancy);
        var totalMinusSafe = report.SafeDiscrepancy < 0 ? -report.SafeDiscrepancy : 0;
        var totalMinusEntered = employeesData.Sum(e => e.Minus);
        var totalMinusExpected = totalMinusCash + totalMinusSafe;

        if (!ValidateMinusTotals(report, showMessage: true))
        {
            return;
        }

        if (!await ValidateEmployeeMinusRulesAsync(showMessage: true))
        {
            return;
        }

        var confirmMessage = $"Отчёт за смену:\n\n" +
                           $"Выручка: {report.Revenue} руб.\n" +
                           $"ЗП сотрудников: {report.TotalSalary} руб.\n" +
                           $"Итог: {report.Total} руб.\n\n";

        if (totalMinusCash > 0 || totalMinusSafe > 0)
        {
            confirmMessage += $"Минус по кассе: {totalMinusCash} руб.\n" +
                            $"Минус по сейфу: {totalMinusSafe} руб.\n" +
                            $"Всего минусов: {totalMinusCash + totalMinusSafe} руб.\n" +
                            $"Указано минусов: {totalMinusEntered} руб.\n\n";
        }

        confirmMessage += "Отправить отчёт?";

        var result = MessageBox.Show(
            confirmMessage,
            "Подтверждение отправки",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        listBoxSendInformation.Items.Clear();
        string? photoSessionKey = null;
        var firstEmployee = employeesData.FirstOrDefault()?.Employee;
        if (firstEmployee == null)
        {
            MessageBox.Show(
                "Выберите первого сотрудника для получения фото.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        buttonSend.Enabled = false;
        buttonСalculate.Enabled = false;

        try
        {
            var photoResult = await photoService!.RequestPhotosAsync(
                firstEmployee.Id,
                message => AddSendInfo(message));

            if (!photoResult.Success || string.IsNullOrWhiteSpace(photoResult.SessionKey))
            {
                MessageBox.Show(
                    photoResult.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            photoSessionKey = photoResult.SessionKey;

            var request = new GrpcRaportRequest
            {
                FactCash = report.FactCash,
                FactNonCash = report.FactNonCash,
                ProgramCash = report.ProgramCash,
                ProgramNonCash = report.ProgramNonCash,
                FactSafe = report.FactSafe,
                WhyMinus = textBoxWhyMinus.Text,
                SendPhoto = true,
                PhotoSessionKey = photoSessionKey ?? string.Empty
            };

            foreach (var emp in report.Employees)
            {
                request.Employees.Add(new EmployeeRaportSalary
                {
                    EmployeeId = emp.Employee?.Id ?? "",
                    Hours = emp.Hours,
                    Minus = emp.Minus,
                });
            }

            var (success, message) = await raportService!.SendRaportAsync(request);

            if (success)
            {
                MessageBox.Show(
                    message,
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ClearAllFields();
            }
            else
            {
                MessageBox.Show(
                    message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        finally
        {
            buttonSend.Enabled = true;
            buttonСalculate.Enabled = true;
        }
    }

    #endregion

    #region Private Methods

    private void InitializeCollections()
    {
        comboBoxes.AddRange([
            comboBoxFirstNameRaport,
            comboBoxSecondNameRaport,
            comboBoxThirdNameRaport
        ]);

        hoursTextBoxes.AddRange([
            textBoxHoursFirstNameRaport,
            textBoxHoursSecondNameRaport,
            textBoxHoursThirdNameRaport
        ]);

        minusTextBoxes.AddRange([
            textBoxMinusFirstNameRaport,
            textBoxMinusSecondNameRaport,
            textBoxMinusThirdNameRaport
        ]);

        numericTextBoxes.AddRange([
            textBoxHoursFirstNameRaport,
            textBoxHoursSecondNameRaport,
            textBoxHoursThirdNameRaport,
            textBoxMinusFirstNameRaport,
            textBoxMinusSecondNameRaport,
            textBoxMinusThirdNameRaport,
            textBoxFactCash,
            textBoxFactNonCash,
            textBoxProgramCash,
            textBoxProgramNonCash,
            textBoxSafe
        ]);

        foreach (var hoursBox in hoursTextBoxes)
        {
            previousHoursValues[hoursBox] = "0";
        }
    }

    private void SubscribeToEvents()
    {
        employeeService?.Employees.ListChanged += Event_ListEmployeeChange;
        safeService?.SafeChanged += Event_SafeChanged;

        foreach (var comboBox in comboBoxes)
        {
            comboBox.SelectedIndexChanged += OnComboBoxSelectedIndexChanged;
            comboBox.SelectedIndexChanged += DataSalaryRaport;
            comboBox.SelectedIndexChanged += ForceComboRedraw;
        }

        foreach (var textBox in numericTextBoxes)
        {
            textBox.KeyPress += FilterNumericInput;
            textBox.TextChanged += NormalizeNumericInput;
            textBox.TextChanged += DataSalaryRaport;
            textBox.TextChanged += UpdateReportPreview;
        }

        foreach (var hoursBox in hoursTextBoxes)
        {
            hoursBox.TextChanged += ValidateTotalHours;
        }

        foreach (var minusBox in minusTextBoxes)
        {
            minusBox.TextChanged += ValidateMinusInputAsync;
        }

        buttonСalculate.Click += OnCalculateClick;
        buttonSend.Click += OnSendClick;
    }

    private void UnsubscribeComboBoxes()
    {
        foreach (var comboBox in comboBoxes)
        {
            comboBox.SelectedIndexChanged -= OnComboBoxSelectedIndexChanged;
            comboBox.SelectedIndexChanged -= DataSalaryRaport;
            comboBox.SelectedIndexChanged -= ForceComboRedraw;
        }
    }

    private void UnsubscribeNumericTextBoxes()
    {
        foreach (var textBox in numericTextBoxes)
        {
            textBox.KeyPress -= FilterNumericInput;
            textBox.TextChanged -= NormalizeNumericInput;
            textBox.TextChanged -= DataSalaryRaport;
            textBox.TextChanged -= UpdateReportPreview;
        }

        foreach (var hoursBox in hoursTextBoxes)
        {
            hoursBox.TextChanged -= ValidateTotalHours;
        }

        foreach (var minusBox in minusTextBoxes)
        {
            minusBox.TextChanged -= ValidateMinusInputAsync;
        }
    }

    private void FilterNumericInput(object? sender, KeyPressEventArgs e)
    {
        if (sender is not MaterialTextBox textBox) return;

        if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
        {
            e.Handled = true;
            return;
        }

        if (!char.IsControl(e.KeyChar))
        {
            bool isHoursField = hoursTextBoxes.Contains(textBox);
            int maxLength = isHoursField ? 2 : 6;

            int newLength = textBox.TextLength - textBox.SelectionLength + 1;

            if (newLength > maxLength)
            {
                e.Handled = true;
            }
        }
    }

    private void NormalizeNumericInput(object? sender, EventArgs e)
    {
        if (isNormalizingInput || sender is not MaterialTextBox textBox)
        {
            return;
        }

        var text = textBox.Text ?? string.Empty;
        var digitsOnly = new string(text.Where(char.IsDigit).ToArray());

        if (digitsOnly == text)
        {
            return;
        }

        isNormalizingInput = true;
        try
        {
            textBox.Text = digitsOnly;
            textBox.SelectionStart = digitsOnly.Length;
        }
        finally
        {
            isNormalizingInput = false;
        }
    }

    private void ValidateTotalHours(object? sender, EventArgs e)
    {
        if (isValidatingHours || sender is not MaterialTextBox changedBox) return;

        const int MaxTotalHours = 12;

        int totalHours = 0;
        foreach (var hoursBox in hoursTextBoxes)
        {
            if (int.TryParse(hoursBox.Text, out var hours))
            {
                totalHours += hours;
            }
        }

        if (totalHours > MaxTotalHours)
        {
            isValidatingHours = true;
            try
            {
                if (previousHoursValues.TryGetValue(changedBox, out var prevValue))
                {
                    changedBox.Text = prevValue;
                }
                else
                {
                    changedBox.Clear();
                }

                MessageBox.Show(
                    $"Суммарное количество часов не может превышать {MaxTotalHours} часов!",
                    "Ограничение по часам",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            finally
            {
                isValidatingHours = false;
            }
        }
        else
        {
            previousHoursValues[changedBox] = changedBox.Text;
        }
    }

    private async void ValidateMinusInputAsync(object? sender, EventArgs e)
    {
        if (isValidatingMinus || isNormalizingInput || sender is not MaterialTextBox minusBox)
        {
            return;
        }

        var index = minusTextBoxes.IndexOf(minusBox);
        if (index < 0 || employeeService == null)
        {
            return;
        }

        if (comboBoxes[index].SelectedItem is not GrpcEmployee employee || employee == nullComboBox)
        {
            return;
        }

        if (!int.TryParse(minusBox.Text, out var minus) || minus <= 0)
        {
            return;
        }

        var salaryCheck = await GetCurrentSalaryAsync(employee.Id);
        if (!salaryCheck.Success || salaryCheck.CurrentSalary >= 0)
        {
            return;
        }

        var hours = int.TryParse(hoursTextBoxes[index].Text, out var parsedHours) ? parsedHours : 0;
        var maxMinus = hours * employee.HourlyRate;
        if (minus <= maxMinus)
        {
            return;
        }

        isValidatingMinus = true;
        try
        {
            minusBox.Text = maxMinus.ToString();
            minusBox.SelectionStart = minusBox.TextLength;

            MessageBox.Show(
                $"Для сотрудника {employee.Name} текущая ЗП отрицательная ({salaryCheck.CurrentSalary} руб.).\n" +
                $"Минус ограничен {maxMinus} руб. (часы {hours} * ставка {employee.HourlyRate}).",
                "Ограничение минуса",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        finally
        {
            isValidatingMinus = false;
        }
    }

    private void Event_ListEmployeeChange(object? sender, ListChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => Event_ListEmployeeChange(sender, e));
            return;
        }

        employeeSalaryCache.Clear();

        var employees = employeeService?.Employees.ToList() ?? [];

        var selectedIds = comboBoxes
            .Select(cb => (cb.SelectedItem as GrpcEmployee)?.Id)
            .ToList();

        foreach (var comboBox in comboBoxes)
        {
            comboBox.DataSource = null;
            comboBox.DisplayMember = nameof(GrpcEmployee.Name);
            comboBox.ValueMember = nameof(GrpcEmployee.Id);

            List<GrpcEmployee> list = [nullComboBox, .. employees];
            comboBox.DataSource = list;
        }

        for (int i = 0; i < comboBoxes.Count; i++)
        {
            if (selectedIds[i] != null)
            {
                var employee = employees.FirstOrDefault(e => e.Id == selectedIds[i]);
                if (employee != null)
                {
                    comboBoxes[i].SelectedItem = employee;
                }
            }
        }
    }

    private void Event_SafeChanged(object? sender, long newSafe)
    {
        if (InvokeRequired)
        {
            Invoke(() => Event_SafeChanged(sender, newSafe));
            return;
        }

        UpdateReportPreview(sender, EventArgs.Empty);
    }

    private void DataSalaryRaport(object? sender, EventArgs e)
    {
        if (isUpdating) return;

        isUpdating = true;
        try
        {
            var changedControl = sender as Control;
            int employeeIndex = GetEmployeeIndex(changedControl);

            if (employeeIndex >= 0)
            {
                UpdateEmployeeData(employeeIndex);
            }
            else
            {
                RebuildEmployeesList();
            }
        }
        finally
        {
            isUpdating = false;
        }
    }

    private void UpdateReportPreview(object? sender, EventArgs e)
    {
        if (isUpdating) return;

        var report = GenerateReport();
        var preview = FormatReportPreview(report);

        listBoxRaport.Items.Clear();
        foreach (var line in preview.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                listBoxRaport.Items.Add(line);
            }
        }
    }

    
    private void AddSendInfo(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => AddSendInfo(message));
            return;
        }

        listBoxSendInformation.Items.Add(message);
    }
    private string FormatReportPreview(ReportData report)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(report.Date.ToString("dd.MM.yyyy HH:mm"));
        sb.AppendLine();
        sb.AppendLine($"Нал: {report.FactCash}");
        sb.AppendLine($"Б/Н: {report.FactNonCash}");
        sb.AppendLine($"Выручка: {report.Revenue}");
        sb.AppendLine($"Итог: {report.Total}");
        sb.AppendLine();
        sb.AppendLine($"Минус по кассе: {report.CashDiscrepancy}");
        sb.AppendLine($"Минус по сейфу: {report.SafeDiscrepancy}");
        sb.AppendLine();
        sb.AppendLine($"Факт сейфа: {report.FactSafe}");
        sb.AppendLine($"Теперь сейфа: {report.NewSafe}");
        sb.AppendLine();
        sb.AppendLine("==програмные данные==");
        sb.AppendLine($"Нал: {report.ProgramCash}");
        sb.AppendLine($"Безнал: {report.ProgramNonCash}");
        sb.AppendLine($"Сейф: {report.ProgramSafe}");

        if (report.TotalSalary > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Всего ЗП: {report.TotalSalary} руб.");
        }

        return sb.ToString();
    }

    private bool ValidateMinusTotals(ReportData report, bool showMessage)
    {
        var totalMinusCash = Math.Abs(report.CashDiscrepancy);
        var totalMinusSafe = report.SafeDiscrepancy < 0 ? -report.SafeDiscrepancy : 0;
        var totalMinusEntered = employeesData.Sum(e => e.Minus);
        var totalMinusExpected = totalMinusCash + totalMinusSafe;

        if (totalMinusEntered == totalMinusExpected)
        {
            return true;
        }

        if (showMessage)
        {
            MessageBox.Show(
                $"Сумма минусов должна быть равна {totalMinusExpected} руб.\n" +
                $"Указано минусов: {totalMinusEntered} руб.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        return false;
    }

    private async Task<bool> ValidateEmployeeMinusRulesAsync(bool showMessage)
    {
        if (employeeService == null)
        {
            return false;
        }

        foreach (var emp in employeesData)
        {
            if (emp.Employee == null || string.IsNullOrWhiteSpace(emp.Employee.Id))
            {
                continue;
            }

            var salaryCheck = await GetCurrentSalaryAsync(emp.Employee.Id);
            if (!salaryCheck.Success)
            {
                if (showMessage)
                {
                    MessageBox.Show(
                        salaryCheck.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }

                return false;
            }

            if (salaryCheck.CurrentSalary >= 0)
            {
                continue;
            }

            var maxMinus = emp.Hours * emp.HourlyRate;
            if (emp.Minus <= maxMinus)
            {
                continue;
            }

            if (showMessage)
            {
                MessageBox.Show(
                    $"Для сотрудника {emp.Employee.Name} при отрицательной ЗП ({salaryCheck.CurrentSalary} руб.) " +
                    $"минус не может превышать {maxMinus} руб.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            return false;
        }

        return true;
    }

    private async Task<(bool Success, int CurrentSalary, string Message)> GetCurrentSalaryAsync(string employeeId)
    {
        if (employeeSalaryCache.TryGetValue(employeeId, out var cachedSalary))
        {
            return (true, cachedSalary, string.Empty);
        }

        if (employeeService == null)
        {
            return (false, 0, "Сервис сотрудников недоступен.");
        }

        var salaryCheck = await employeeService.GetCurrentSalaryAsync(employeeId);
        if (salaryCheck.Success)
        {
            employeeSalaryCache[employeeId] = salaryCheck.CurrentSalary;
        }

        return salaryCheck;
    }

    private int GetEmployeeIndex(Control? control)
    {
        if (control == null) return -1;

        for (int i = 0; i < comboBoxes.Count; i++)
        {
            if (control == comboBoxes[i] ||
                control == hoursTextBoxes[i] ||
                control == minusTextBoxes[i])
            {
                return i;
            }
        }

        return -1;
    }

    private void UpdateEmployeeData(int index)
    {
        var comboBox = comboBoxes[index];
        var hoursBox = hoursTextBoxes[index];
        var minusBox = minusTextBoxes[index];

        if (comboBox.SelectedItem is not GrpcEmployee employee || employee == nullComboBox)
        {
            RebuildEmployeesList();
            return;
        }

        if (index >= employeesData.Count)
        {
            RebuildEmployeesList();
            return;
        }

        employeesData[index].Employee = employee;
        employeesData[index].Hours = int.TryParse(hoursBox.Text, out var h) ? h : 0;
        employeesData[index].Minus = int.TryParse(minusBox.Text, out var m) ? m : 0;

        employeesData.ResetItem(index);
    }

    private void RebuildEmployeesList()
    {
        employeesData.Clear();

        AddEmployeeIfValid(comboBoxFirstNameRaport, textBoxHoursFirstNameRaport, textBoxMinusFirstNameRaport);
        AddEmployeeIfValid(comboBoxSecondNameRaport, textBoxHoursSecondNameRaport, textBoxMinusSecondNameRaport);
        AddEmployeeIfValid(comboBoxThirdNameRaport, textBoxHoursThirdNameRaport, textBoxMinusThirdNameRaport);

        UpdateListboxSalaryRaport();
    }

    private void AddEmployeeIfValid(ComboBox comboBox, MaterialTextBox hoursBox, MaterialTextBox minusBox)
    {
        if (comboBox.SelectedItem is not GrpcEmployee employee || employee == nullComboBox)
            return;

        employeesData.Add(new EmployeeHoursList
        {
            Employee = employee,
            Hours = int.TryParse(hoursBox.Text, out var h) ? h : 0,
            Minus = int.TryParse(minusBox.Text, out var m) ? m : 0
        });
    }

    private void UpdateListboxSalaryRaport()
    {
        if (listBoxSalaryRaport.DataSource == null)
        {
            listBoxSalaryRaport.DisplayMember = nameof(EmployeeHoursList.SalaryInfo);
            listBoxSalaryRaport.DataSource = employeesData;
        }
        else
        {
            employeesData.ResetBindings();
        }
    }

    private void ForceComboRedraw(object? sender, EventArgs e)
    {
        if (sender is Control control)
        {
            control.Invalidate();
            control.Refresh();
        }
    }

    private void ClearAllFields()
    {
        textBoxFactCash.Clear();
        textBoxFactNonCash.Clear();
        textBoxProgramCash.Clear();
        textBoxProgramNonCash.Clear();
        textBoxSafe.Clear();
        textBoxWhyMinus.Clear();

        foreach (var comboBox in comboBoxes)
        {
            comboBox.SelectedIndex = 0;
        }

        ClearAllTextBoxes();

        employeesData.Clear();
        employeeSalaryCache.Clear();
        listBoxRaport.Items.Clear();
        listBoxSendInformation.Items.Clear();
    }

    #endregion

    #region ComboBox Logic

    private void OnComboBoxSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnComboBoxSelectedIndexChanged(sender, e));
            return;
        }

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        var firstValid = comboBoxFirstNameRaport.SelectedItem is GrpcEmployee first && first != nullComboBox;
        var secondValid = comboBoxSecondNameRaport.SelectedItem is GrpcEmployee second && second != nullComboBox;
        var thirdValid = comboBoxThirdNameRaport.SelectedItem is GrpcEmployee third && third != nullComboBox;

        comboBoxSecondNameRaport.Visible = firstValid;

        textBoxMinusFirstNameRaport.Visible =
        textBoxHoursSecondNameRaport.Visible =
        textBoxMinusSecondNameRaport.Visible = firstValid && secondValid;

        comboBoxThirdNameRaport.Visible = firstValid && secondValid;

        textBoxHoursThirdNameRaport.Visible =
        textBoxMinusThirdNameRaport.Visible = firstValid && secondValid && thirdValid;

        if (!firstValid)
        {
            ClearAllTextBoxes();
        }
        else if (!secondValid)
        {
            comboBoxThirdNameRaport.SelectedIndex = 0;
            ClearSecondAndThirdTextBoxes();
        }
        else if (!thirdValid)
        {
            ClearThirdTextBoxes();
        }
    }

    private void ClearAllTextBoxes()
    {
        foreach (var textBox in hoursTextBoxes.Concat(minusTextBoxes))
        {
            textBox.Clear();
        }

        foreach (var hoursBox in hoursTextBoxes)
        {
            previousHoursValues[hoursBox] = "0";
        }
    }

    private void ClearSecondAndThirdTextBoxes()
    {
        textBoxHoursSecondNameRaport.Clear();
        textBoxMinusSecondNameRaport.Clear();
        previousHoursValues[textBoxHoursSecondNameRaport] = "0";

        ClearThirdTextBoxes();
    }

    private void ClearThirdTextBoxes()
    {
        textBoxHoursThirdNameRaport.Clear();
        textBoxMinusThirdNameRaport.Clear();
        previousHoursValues[textBoxHoursThirdNameRaport] = "0";
    }

    #endregion

    #region Cleanup

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnsubscribeFromEvents();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}


