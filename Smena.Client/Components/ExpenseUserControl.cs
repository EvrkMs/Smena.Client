using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Expense;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class ExpenseUserControl : UserControl
{
    private EmployeeService? employeeService;
    private ExpenseService? _expenseService;
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };

    public ExpenseUserControl()
    {
        InitializeComponent();
        SetupDefaultStates();
    }

    public void Initialize(EmployeeService employeeService, ExpenseService _expenseService)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(_expenseService);

        this.employeeService = employeeService;
        this._expenseService = _expenseService;

        SubscribeToEvents();
    }

    public void UnsubscribeFromEvents()
    {
        if (employeeService != null)
        {
            employeeService.Employees.ListChanged -= Event_ListEmployeeChange;
        }

        buttonSendExpenses.Click -= OnSendClick;
        textBoxAmountExpenses.KeyPress -= FilterNumericInput;
        checkBoxPhotoSendExpenses.CheckedChanged -= OnPhotoCheckChanged;
    }

    private void SetupDefaultStates()
    {
        checkBoxFromSafeExpenses.Checked = true;
        checkBoxPhotoSendExpenses.Checked = false;
        comboBoxPhotoSendExpenses.Visible = false;
    }

    private void SubscribeToEvents()
    {
        if (employeeService != null)
        {
            employeeService.Employees.ListChanged += Event_ListEmployeeChange;
            LoadEmployees();
        }

        buttonSendExpenses.Click += OnSendClick;
        textBoxAmountExpenses.KeyPress += FilterNumericInput;
        textBoxCommentExpenses.KeyPress += FilterNumericInput;
        checkBoxPhotoSendExpenses.CheckedChanged += OnPhotoCheckChanged;
    }

    private void OnPhotoCheckChanged(object? sender, EventArgs e)
    {
        comboBoxPhotoSendExpenses.Visible = checkBoxPhotoSendExpenses.Checked;
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

        comboBoxPhotoSendExpenses.DataSource = null;
        comboBoxPhotoSendExpenses.DisplayMember = nameof(GrpcEmployee.Name);
        comboBoxPhotoSendExpenses.ValueMember = nameof(GrpcEmployee.Id);

        List<GrpcEmployee> list = [nullComboBox, .. employees];
        comboBoxPhotoSendExpenses.DataSource = list;
    }

    private void FilterNumericInput(object? sender, KeyPressEventArgs e)
    {
        if (sender is not MaterialTextBox textBox) return;

        // Для комментария разрешаем всё
        if (textBox == textBoxCommentExpenses)
            return;

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
        if (!uint.TryParse(textBoxAmountExpenses.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show(
                "Введите корректную сумму расхода!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (checkBoxPhotoSendExpenses.Checked &&
            (comboBoxPhotoSendExpenses.SelectedItem is not GrpcEmployee employee ||
             employee == nullComboBox))
        {
            MessageBox.Show(
                "Выберите получателя фото!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var request = new GrpcExpenseAdd
        {
            Amount = amount,
            Comment = textBoxCommentExpenses.Text,
            FromSafe = checkBoxFromSafeExpenses.Checked,
            SendPhoto = checkBoxPhotoSendExpenses.Checked,
        };

        var confirmMessage = $"Добавить расход {request.Amount} руб." +
                           $"{(request.FromSafe ? " (из сейфа)" : "")}?";

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            confirmMessage += $"\nКомментарий: {request.Comment}";
        }

        var result = MessageBox.Show(
            confirmMessage,
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        buttonSendExpenses.Enabled = false;

        try
        {
            var res = await _expenseService!.AddExpenseOperationAsync(request);

            if (res.Value)
            {
                MessageBox.Show(
                    res.Message,
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ClearFields();
            }
            else
            {
                MessageBox.Show(
                    res.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        finally
        {
            buttonSendExpenses.Enabled = true;
        }
    }

    private void ClearFields()
    {
        textBoxAmountExpenses.Clear();
        textBoxCommentExpenses.Clear();
        checkBoxFromSafeExpenses.Checked = true;
        checkBoxPhotoSendExpenses.Checked = false;
        comboBoxPhotoSendExpenses.SelectedIndex = 0;
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