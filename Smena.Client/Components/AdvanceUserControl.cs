using Host.Grpc.Services.Advance;
using Host.Grpc.Services.Employee;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class AdvanceUserControl : UserControl
{
    private EmployeeService? employeeService;
    private AdvanceService? advanceService;
    private FormCacheService? formCache;
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };
    private bool isRestoringCache;

    private const string CachePrefix = "Advance.";

    public AdvanceUserControl()
    {
        InitializeComponent();
        SetupDefaultStates();
    }

    public void Initialize(EmployeeService employeeService, AdvanceService advanceService, FormCacheService formCache)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(advanceService);
        ArgumentNullException.ThrowIfNull(formCache);

        this.employeeService = employeeService;
        this.advanceService = advanceService;
        this.formCache = formCache;

        SubscribeToEvents();
        RestoreCachedValues();
    }

    public void UnsubscribeFromEvents()
    {
        if (employeeService != null)
        {
            employeeService.Employees.ListChanged -= Event_ListEmployeeChange;
        }

        buttonSendExtractSalary.Click -= OnSendClick;
        textBoxSalaryExtractAmount.KeyPress -= FilterNumericInput;
        checkBoxExtractSalaryFromSafe.CheckedChanged -= OnBezNalChanged;
    }

    private void SetupDefaultStates()
    {
        checkBoxExtractSalaryFromSafe.Checked = false;
        checkBoxAdvanceExtract.Checked = true;
        checkBoxSalaryAdvance.Checked = false;
    }

    private void SubscribeToEvents()
    {
        if (employeeService != null)
        {
            employeeService.Employees.ListChanged += Event_ListEmployeeChange;
            LoadEmployees();
        }

        buttonSendExtractSalary.Click += OnSendClick;
        textBoxSalaryExtractAmount.KeyPress += FilterNumericInput;
        checkBoxExtractSalaryFromSafe.CheckedChanged += OnBezNalChanged;

        // Cache hooks
        textBoxSalaryExtractAmount.TextChanged += SaveFieldToCache;
        comboBoxExtractSalaryName.SelectedIndexChanged += SaveFieldToCache;
        checkBoxExtractSalaryFromSafe.CheckedChanged += SaveFieldToCache;
        checkBoxAdvanceExtract.CheckedChanged += SaveFieldToCache;
        checkBoxSalaryAdvance.CheckedChanged += SaveFieldToCache;

        checkBoxAdvanceExtract.CheckedChanged += (s, e) =>
        {
            if (checkBoxAdvanceExtract.Checked)
                checkBoxSalaryAdvance.Checked = false;
        };

        checkBoxSalaryAdvance.CheckedChanged += (s, e) =>
        {
            if (checkBoxSalaryAdvance.Checked)
                checkBoxAdvanceExtract.Checked = false;
        };
    }

    private void OnBezNalChanged(object? sender, EventArgs e)
    {
        buttonSendExtractSalary.Text = checkBoxExtractSalaryFromSafe.Checked
            ? "Отправить (Б/Н)"
            : "Отправить (из сейфа)";
    }

    private void Event_ListEmployeeChange(object? sender, ListChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => Event_ListEmployeeChange(sender, e));
            return;
        }

        LoadEmployees();
    }

    private void LoadEmployees()
    {
        if (employeeService == null) return;

        var employees = employeeService.Employees.ToList();
        var selectedId = (comboBoxExtractSalaryName.SelectedItem as GrpcEmployee)?.Id;

        comboBoxExtractSalaryName.DataSource = null;
        comboBoxExtractSalaryName.DisplayMember = nameof(GrpcEmployee.Name);
        comboBoxExtractSalaryName.ValueMember = nameof(GrpcEmployee.Id);

        List<GrpcEmployee> list = [nullComboBox, .. employees];
        comboBoxExtractSalaryName.DataSource = list;

        if (selectedId != null)
        {
            var employee = employees.FirstOrDefault(e => e.Id == selectedId);
            if (employee != null)
            {
                comboBoxExtractSalaryName.SelectedItem = employee;
            }
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
            int newLength = textBox.TextLength - textBox.SelectionLength + 1;
            if (newLength > 6)
            {
                e.Handled = true;
            }
        }
    }

    private async void OnSendClick(object? sender, EventArgs e)
    {
        if (comboBoxExtractSalaryName.SelectedItem is not GrpcEmployee employee || employee == nullComboBox)
        {
            MessageBox.Show(
                "Выберите сотрудника!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (!int.TryParse(textBoxSalaryExtractAmount.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show(
                "Введите корректную сумму!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var salaryCheck = await employeeService!.GetCurrentSalaryAsync(employee.Id);
        if (!salaryCheck.Success)
        {
            MessageBox.Show(
                salaryCheck.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (salaryCheck.CurrentSalary <= 0)
        {
            MessageBox.Show(
                "У сотрудника нет доступной ЗП для выплаты.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (amount > salaryCheck.CurrentSalary)
        {
            MessageBox.Show(
                $"Нельзя выдать больше текущей ЗП ({salaryCheck.CurrentSalary} руб.).",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var isNonCash = checkBoxExtractSalaryFromSafe.Checked;
        var request = new GrpcAdvanceRequest
        {
            EmployeeId = employee.Id,
            Amount = amount,
            IsNonCash = isNonCash,
            IsSalary = checkBoxSalaryAdvance.Checked,
            ExtractFromSafe = !isNonCash,
            Comment = checkBoxSalaryAdvance.Checked ? "ЗП" : "Аванс"
        };

        var confirmMessage = $"Выплатить {(request.IsSalary ? "ЗП" : "аванс")} " +
                           $"{request.Amount} руб. сотруднику {employee.Name}" +
                           $"{(request.IsNonCash ? " (Б/Н)" : " (из сейфа)")}?";

        var result = MessageBox.Show(
            confirmMessage,
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        buttonSendExtractSalary.Enabled = false;

        try
        {
            var (success, message) = await advanceService!.SendAdvanceAsync(request);

            if (success)
            {
                MessageBox.Show(
                    message,
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ClearFields();
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
            buttonSendExtractSalary.Enabled = true;
        }
    }

    private void ClearFields()
    {
        textBoxSalaryExtractAmount.Clear();
        comboBoxExtractSalaryName.SelectedIndex = 0;
        checkBoxExtractSalaryFromSafe.Checked = false;
        checkBoxAdvanceExtract.Checked = true;
        checkBoxSalaryAdvance.Checked = false;
        buttonSendExtractSalary.Text = "Отправить (из сейфа)";

        formCache?.ClearPrefix(CachePrefix);
    }

    private void SaveFieldToCache(object? sender, EventArgs e)
    {
        if (isRestoringCache || formCache == null) return;

        formCache.Set(CachePrefix + "Amount", textBoxSalaryExtractAmount.Text);
        var emp = comboBoxExtractSalaryName.SelectedItem as GrpcEmployee;
        formCache.Set(CachePrefix + "Employee", emp != null && emp != nullComboBox ? emp.Name : null);
        formCache.Set(CachePrefix + "IsNonCash", checkBoxExtractSalaryFromSafe.Checked ? "1" : "0");
        formCache.Set(CachePrefix + "IsAdvance", checkBoxAdvanceExtract.Checked ? "1" : "0");
        formCache.Set(CachePrefix + "IsSalary", checkBoxSalaryAdvance.Checked ? "1" : "0");
    }

    private void RestoreCachedValues()
    {
        if (formCache == null) return;

        isRestoringCache = true;
        try
        {
            textBoxSalaryExtractAmount.Text = formCache.Get(CachePrefix + "Amount") ?? string.Empty;

            var cachedName = formCache.Get(CachePrefix + "Employee");
            if (!string.IsNullOrWhiteSpace(cachedName))
            {
                for (var j = 0; j < comboBoxExtractSalaryName.Items.Count; j++)
                {
                    if (comboBoxExtractSalaryName.Items[j] is GrpcEmployee emp &&
                        string.Equals(emp.Name, cachedName, StringComparison.Ordinal))
                    {
                        comboBoxExtractSalaryName.SelectedIndex = j;
                        break;
                    }
                }
            }

            if (formCache.Get(CachePrefix + "IsNonCash") == "1")
                checkBoxExtractSalaryFromSafe.Checked = true;
            if (formCache.Get(CachePrefix + "IsSalary") == "1")
            {
                checkBoxSalaryAdvance.Checked = true;
                checkBoxAdvanceExtract.Checked = false;
            }
        }
        finally
        {
            isRestoringCache = false;
        }
    }

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
