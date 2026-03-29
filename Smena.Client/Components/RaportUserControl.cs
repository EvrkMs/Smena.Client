using Host.Grpc.Services.Employee;
using MaterialSkin.Controls;
using Smena.Client.Helpers;
using Smena.Client.Models;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class RaportUserControl : UserControl
{
    private EmployeeService? employeeService;
    private SafeService? safeService;
    private RaportService? raportService;
    private PhotoService? photoService;
    private FormCacheService? formCache;
    private readonly List<ComboBox> comboBoxes = [];
    private readonly List<MaterialTextBox> hoursTextBoxes = [];
    private readonly List<MaterialTextBox> minusTextBoxes = [];
    private readonly List<MaterialTextBox> numericTextBoxes = [];
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };
    private readonly BindingList<EmployeeHoursList> employeesData = [];
    private bool isUpdating;
    private bool isNormalizingInput;
    private readonly Dictionary<MaterialTextBox, string> previousHoursValues = [];
    private readonly Dictionary<string, int> employeeSalaryCache = new(StringComparer.Ordinal);
    private bool isValidatingHours;
    private bool isValidatingMinus;
    private bool isRestoringCache;

    private const string CachePrefix = "Raport.";

    public RaportUserControl()
    {
        InitializeComponent();
        InitializeCollections();
    }

    public void Initialize(
        EmployeeService employeeService,
        SafeService safeService,
        RaportService raportService,
        PhotoService photoService,
        FormCacheService formCache)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(safeService);
        ArgumentNullException.ThrowIfNull(raportService);
        ArgumentNullException.ThrowIfNull(photoService);
        ArgumentNullException.ThrowIfNull(formCache);

        this.employeeService = employeeService;
        this.safeService = safeService;
        this.raportService = raportService;
        this.photoService = photoService;
        this.formCache = formCache;

        SubscribeToEvents();
        RestoreCachedValues();
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
        RebuildEmployeesList();

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
        report.SafeDiscrepancy = report.FactSafe - (int)report.ProgramSafe;

        report.Revenue = (report.FactCash - ReportData.InitialCash) + report.FactNonCash;
        report.TotalSalary = employeesData.Sum(e => e.Salary);
        report.Total = report.Revenue - report.TotalSalary;

        int discrepancy = (report.FactCash + report.FactNonCash) - (report.ProgramCash + report.ProgramNonCash);
        report.CashDiscrepancy = discrepancy < 0 ? discrepancy : 0;

        report.Employees = [.. employeesData];

        return report;
    }

    // ── Event wiring ────────────────────────────────────────────

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
        textBoxSafe.TextChanged += OnFactSafeChanged;

        foreach (var comboBox in comboBoxes)
        {
            comboBox.SelectedIndexChanged += OnComboBoxSelectedIndexChanged;
            comboBox.SelectedIndexChanged += DataSalaryRaport;
            comboBox.SelectedIndexChanged += ForceComboRedraw;
            comboBox.SelectedIndexChanged += SaveFieldToCache;
        }

        foreach (var textBox in numericTextBoxes)
        {
            textBox.KeyPress += FilterNumericInputRaport;
            textBox.TextChanged += NormalizeNumericInput;
            textBox.TextChanged += DataSalaryRaport;
            textBox.TextChanged += UpdateReportPreview;
            textBox.TextChanged += SaveFieldToCache;
        }

        textBoxWhyMinus.TextChanged += SaveFieldToCache;

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
            comboBox.SelectedIndexChanged -= SaveFieldToCache;
        }
    }

    private void UnsubscribeNumericTextBoxes()
    {
        textBoxSafe.TextChanged -= OnFactSafeChanged;
        textBoxWhyMinus.TextChanged -= SaveFieldToCache;

        foreach (var textBox in numericTextBoxes)
        {
            textBox.KeyPress -= FilterNumericInputRaport;
            textBox.TextChanged -= NormalizeNumericInput;
            textBox.TextChanged -= DataSalaryRaport;
            textBox.TextChanged -= UpdateReportPreview;
            textBox.TextChanged -= SaveFieldToCache;
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

    // ── Employee data management ────────────────────────────────

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

        if (!IsRowActive(index) ||
            comboBox.SelectedItem is not GrpcEmployee employee ||
            employee == nullComboBox)
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

        AddEmployeeIfValid(0, comboBoxFirstNameRaport, textBoxHoursFirstNameRaport, textBoxMinusFirstNameRaport);
        AddEmployeeIfValid(1, comboBoxSecondNameRaport, textBoxHoursSecondNameRaport, textBoxMinusSecondNameRaport);
        AddEmployeeIfValid(2, comboBoxThirdNameRaport, textBoxHoursThirdNameRaport, textBoxMinusThirdNameRaport);

        UpdateListboxSalaryRaport();
    }

    private void AddEmployeeIfValid(int index, ComboBox comboBox, MaterialTextBox hoursBox, MaterialTextBox minusBox)
    {
        if (!IsRowActive(index) ||
            comboBox.SelectedItem is not GrpcEmployee employee ||
            employee == nullComboBox)
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

    // ── ComboBox UI ─────────────────────────────────────────────

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

        textBoxHoursFirstNameRaport.Visible = firstValid;
        textBoxMinusFirstNameRaport.Visible = firstValid;
        comboBoxSecondNameRaport.Visible = firstValid;

        textBoxHoursSecondNameRaport.Visible = firstValid && secondValid;
        textBoxMinusSecondNameRaport.Visible = firstValid && secondValid;

        comboBoxThirdNameRaport.Visible = firstValid && secondValid;

        textBoxHoursThirdNameRaport.Visible = firstValid && secondValid && thirdValid;
        textBoxMinusThirdNameRaport.Visible = firstValid && secondValid && thirdValid;

        if (!firstValid) return;

        if (!secondValid)
        {
            comboBoxThirdNameRaport.SelectedIndex = 0;
        }
    }

    private bool IsRowActive(int index)
    {
        if (index < 0 || index >= comboBoxes.Count)
            return false;

        if (comboBoxes[index].SelectedItem is not GrpcEmployee employee || employee == nullComboBox)
            return false;

        return hoursTextBoxes[index].Visible && minusTextBoxes[index].Visible;
    }

    private void ForceComboRedraw(object? sender, EventArgs e)
    {
        if (sender is Control control)
        {
            control.Invalidate();
            control.Refresh();
        }
    }

    // ── Dispose ─────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnsubscribeFromEvents();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
