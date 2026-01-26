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
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };

    public AdvanceUserControl()
    {
        InitializeComponent();
        SetupDefaultStates();
    }

    public void Initialize(EmployeeService employeeService, AdvanceService advanceService)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(advanceService);

        this.employeeService = employeeService;
        this.advanceService = advanceService;

        SubscribeToEvents();
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