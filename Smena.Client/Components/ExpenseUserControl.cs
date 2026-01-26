using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Expense;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class ExpenseUserControl : UserControl
{
    private EmployeeService? employeeService;
    private ExpenseService? expenseService;
    private PhotoService? photoService;
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };

    public ExpenseUserControl()
    {
        InitializeComponent();
        SetupDefaultStates();
    }

    public void Initialize(EmployeeService employeeService, ExpenseService expenseService, PhotoService photoService)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(expenseService);
        ArgumentNullException.ThrowIfNull(photoService);

        this.employeeService = employeeService;
        this.expenseService = expenseService;
        this.photoService = photoService;

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
        if (!int.TryParse(textBoxAmountExpenses.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show(
                "Введите корректную сумму расхода!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        GrpcEmployee? selectedEmployee = null;
        if (checkBoxPhotoSendExpenses.Checked)
        {
            if (comboBoxPhotoSendExpenses.SelectedItem is not GrpcEmployee employee || employee == nullComboBox)
            {
                MessageBox.Show(
                    "Выберите получателя фото!",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            selectedEmployee = employee;
        }

        var confirmMessage = $"Добавить расход {amount} руб." +
                           $"{(checkBoxFromSafeExpenses.Checked ? " (из сейфа)" : "")}?";

        if (!string.IsNullOrWhiteSpace(textBoxCommentExpenses.Text))
        {
            confirmMessage += $"\nКомментарий: {textBoxCommentExpenses.Text}";
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
        var originalText = buttonSendExpenses.Text;

        try
        {
            string? sessionKey = null;
            if (checkBoxPhotoSendExpenses.Checked && selectedEmployee != null)
            {
                var photoResult = await photoService!.RequestPhotosAsync(
                    selectedEmployee.Id,
                    message => SetSendStatus(message));

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

                sessionKey = photoResult.SessionKey;
            }

            bool fromSafe = checkBoxFromSafeExpenses.Checked;
            bool isNonCash = !fromSafe;

            var request = new GrpcExpenseAdd
            {
                Amount = amount,
                Comment = textBoxCommentExpenses.Text,
                FromSafe = fromSafe,
                IsNonCash = isNonCash,
                SendPhoto = checkBoxPhotoSendExpenses.Checked,
                PhotoSessionKey = sessionKey ?? string.Empty,
                SenderName = selectedEmployee?.Name ?? string.Empty
            };

            var res = await expenseService!.AddExpenseOperationAsync(request);

            if (res.Success)
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
            buttonSendExpenses.Text = originalText;
            buttonSendExpenses.Enabled = true;
        }
    }

    private void SetSendStatus(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetSendStatus(message));
            return;
        }

        buttonSendExpenses.Text = message;
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