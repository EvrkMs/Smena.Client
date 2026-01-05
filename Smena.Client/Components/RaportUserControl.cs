using Host.Grpc.Services.Employee;
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
        public int CashSafeDeduction { get; set; } // Минус по кассе/сейфу
        public int Salary => (Hours * HourlyRate) - Minus - CashSafeDeduction;

        public const int HourlyRate = 190;

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

        public const int InitialCash = 1000; // Начальная сумма в кассе
    }
    #endregion

    #region Fields

    private EmployeeService? employeeService;
    private SafeService? safeService;
    private readonly List<ComboBox> comboBoxes = [];
    private readonly List<MaterialTextBox> hoursTextBoxes = [];
    private readonly List<MaterialTextBox> minusTextBoxes = [];
    private readonly List<MaterialTextBox> numericTextBoxes = [];
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };
    private readonly BindingList<EmployeeHoursList> employeesData = [];
    private bool isUpdating = false;
    private readonly Dictionary<MaterialTextBox, string> previousHoursValues = [];
    private bool isValidatingHours = false;

    #endregion

    #region Constructors

    public RaportUserControl()
    {
        InitializeComponent();
        InitializeCollections();
    }

    #endregion

    #region Public API

    public void Initialize(EmployeeService employeeService, SafeService safeService)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(safeService);

        this.employeeService = employeeService;
        this.safeService = safeService;
        SubscribeToEvents();
    }

    public void UnsubscribeFromEvents()
    {
        employeeService?.Employees.ListChanged -= Event_ListEmployeeChange;
        safeService?.SafeChanged -= Event_SafeChanged;
        UnsubscribeComboBoxes();
        UnsubscribeNumericTextBoxes();
    }

    public IReadOnlyList<EmployeeHoursList> GetEmployeesData() => employeesData;

    public bool ValidateAndDistributeDiscrepancies()
    {
        var report = GenerateReport();
        int totalDiscrepancy = Math.Abs(report.CashDiscrepancy) + Math.Abs(report.SafeDiscrepancy);

        if (totalDiscrepancy == 0)
        {
            // Нет минусов - очищаем все вычеты
            foreach (var emp in employeesData)
            {
                emp.CashSafeDeduction = 0;
            }
            employeesData.ResetBindings();
            return true;
        }

        if (employeesData.Count == 0)
        {
            MessageBox.Show(
                "Невозможно распределить минусы: не выбран ни один сотрудник!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return false;
        }

        if (employeesData.Count == 1)
        {
            // Автоматически весь минус на одного сотрудника
            employeesData[0].CashSafeDeduction = totalDiscrepancy;
            employeesData.ResetBindings();
            UpdateReportPreview(null, EventArgs.Empty);
            return true;
        }

        // Несколько сотрудников - показываем диалог
        return ShowDiscrepancyDistributionDialog(totalDiscrepancy);
    }

    public ReportData GenerateReport()
    {
        var report = new ReportData
        {
            Date = DateTime.Now
        };

        // Факт
        report.FactCash = int.TryParse(textBoxFactCash.Text, out var fc) ? fc : 0;
        report.FactNonCash = int.TryParse(textBoxFactNonCash.Text, out var fnc) ? fnc : 0;

        // Программа
        report.ProgramCash = int.TryParse(textBoxProgramCash.Text, out var pc) ? pc : 0;
        report.ProgramNonCash = int.TryParse(textBoxProgramNonCash.Text, out var pnc) ? pnc : 0;
        report.ProgramSafe = safeService?.CurrentSafe ?? 0;

        // Сейф
        report.FactSafe = int.TryParse(textBoxSafe.Text, out var fs) ? fs : 0;
        report.NewSafe = report.FactSafe + (report.FactCash - ReportData.InitialCash);

        int safeDiscrepancy = report.FactSafe - (int)report.ProgramSafe;
        report.SafeDiscrepancy = safeDiscrepancy > 0 ? 0 : safeDiscrepancy;

        // Выручка и итог
        report.Revenue = (report.FactCash - ReportData.InitialCash) + report.FactNonCash;
        report.TotalSalary = employeesData.Sum(e => e.Salary); // Уже включает CashSafeDeduction
        report.Total = report.Revenue - report.TotalSalary;

        // Расхождение по кассе
        int discrepancy = (report.FactCash + report.FactNonCash) - (report.ProgramCash + report.ProgramNonCash);
        report.CashDiscrepancy = discrepancy < 0 ? discrepancy : 0;

        // Сотрудники
        report.Employees = employeesData.ToList();

        return report;
    }

    #endregion

    #region Discrepancy Distribution

    private bool ShowDiscrepancyDistributionDialog(int totalDiscrepancy)
    {
        using var dialog = new Form
        {
            Text = "Распределение минусов",
            Width = 400,
            Height = 300,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        int yPos = 10;

        // Заголовок
        var lblTitle = new Label
        {
            Text = $"Общий минус для распределения: {totalDiscrepancy} руб.",
            Location = new Point(10, yPos),
            AutoSize = true,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };
        panel.Controls.Add(lblTitle);
        yPos += 30;

        // Поля для каждого сотрудника
        var deductionBoxes = new Dictionary<EmployeeHoursList, TextBox>();

        foreach (var emp in employeesData)
        {
            var lblName = new Label
            {
                Text = emp.Employee?.Name ?? "Неизвестно",
                Location = new Point(10, yPos),
                Width = 200
            };
            panel.Controls.Add(lblName);

            var txtDeduction = new TextBox
            {
                Location = new Point(220, yPos - 3),
                Width = 100,
                Text = emp.CashSafeDeduction.ToString()
            };
            txtDeduction.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                    e.Handled = true;
            };
            panel.Controls.Add(txtDeduction);

            var lblRub = new Label
            {
                Text = "руб.",
                Location = new Point(330, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblRub);

            deductionBoxes[emp] = txtDeduction;
            yPos += 30;
        }

        yPos += 10;

        // Кнопки
        var btnOk = new Button
        {
            Text = "ОК",
            Location = new Point(150, yPos),
            DialogResult = DialogResult.OK
        };
        panel.Controls.Add(btnOk);

        var btnCancel = new Button
        {
            Text = "Отмена",
            Location = new Point(240, yPos),
            DialogResult = DialogResult.Cancel
        };
        panel.Controls.Add(btnCancel);

        dialog.Controls.Add(panel);
        dialog.AcceptButton = btnOk;
        dialog.CancelButton = btnCancel;

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            // Валидация и применение
            int sum = 0;
            var newDeductions = new Dictionary<EmployeeHoursList, int>();

            foreach (var kvp in deductionBoxes)
            {
                if (int.TryParse(kvp.Value.Text, out var deduction))
                {
                    newDeductions[kvp.Key] = deduction;
                    sum += deduction;
                }
                else
                {
                    MessageBox.Show(
                        $"Некорректное значение для сотрудника {kvp.Key.Employee?.Name}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }
            }

            if (sum != totalDiscrepancy)
            {
                MessageBox.Show(
                    $"Сумма распределенных минусов ({sum} руб.) не совпадает с общим минусом ({totalDiscrepancy} руб.)!",
                    "Ошибка распределения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }

            // Применяем значения
            foreach (var kvp in newDeductions)
            {
                kvp.Key.CashSafeDeduction = kvp.Value;
            }

            employeesData.ResetBindings();
            UpdateReportPreview(null, EventArgs.Empty);
            return true;
        }

        return false; // Пользователь отменил
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

        // Инициализация словаря предыдущих значений
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
            textBox.TextChanged += DataSalaryRaport;
            textBox.TextChanged += UpdateReportPreview;
        }

        // Валидация суммы часов
        foreach (var hoursBox in hoursTextBoxes)
        {
            hoursBox.TextChanged += ValidateTotalHours;
        }
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
            textBox.TextChanged -= DataSalaryRaport;
            textBox.TextChanged -= UpdateReportPreview;
        }

        foreach (var hoursBox in hoursTextBoxes)
        {
            hoursBox.TextChanged -= ValidateTotalHours;
        }
    }

    private void FilterNumericInput(object? sender, KeyPressEventArgs e)
    {
        if (sender is not MaterialTextBox textBox) return;

        // Разрешаем только цифры и управляющие клавиши
        if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
        {
            e.Handled = true;
            return;
        }

        // Проверяем максимальную длину с учетом выделенного текста
        if (!char.IsControl(e.KeyChar))
        {
            bool isHoursField = hoursTextBoxes.Contains(textBox);
            int maxLength = isHoursField ? 2 : 6;

            // Вычисляем реальную длину после ввода символа
            // Если текст выделен - он будет заменен новым символом
            int newLength = textBox.TextLength - textBox.SelectionLength + 1;

            if (newLength > maxLength)
            {
                e.Handled = true;
            }
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
                // Восстанавливаем предыдущее значение
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
            // Сохраняем корректное значение
            previousHoursValues[changedBox] = changedBox.Text;
        }
    }

    private void Event_ListEmployeeChange(object? sender, ListChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => Event_ListEmployeeChange(sender, e));
            return;
        }

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

            // Показываем распределение минусов если они есть
            int totalDeductions = report.Employees.Sum(e => e.CashSafeDeduction);
            if (totalDeductions > 0)
            {
                sb.AppendLine();
                sb.AppendLine("==распределение минусов==");
                foreach (var emp in report.Employees)
                {
                    if (emp.CashSafeDeduction > 0)
                    {
                        sb.AppendLine($"{emp.Employee?.Name}: -{emp.CashSafeDeduction} руб.");
                    }
                }
            }
        }

        return sb.ToString();
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

        // Сбрасываем вычеты при изменении состава сотрудников
        foreach (var emp in employeesData)
        {
            emp.CashSafeDeduction = 0;
        }

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

        // Сбрасываем сохраненные значения часов
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