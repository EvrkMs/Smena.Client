using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Expense;
using MaterialSkin.Controls;
using Smena.Client.Helpers;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class ExpenseUserControl : UserControl
{
    private EmployeeService? employeeService;
    private ExpenseService? expenseService;
    private PhotoService? photoService;
    private FormCacheService? formCache;
    private readonly GrpcEmployee nullComboBox = new() { Name = "Нет" };
    private bool isRestoringCache;

    private const string CachePrefix = "Expense.";

    public ExpenseUserControl()
    {
        InitializeComponent();
        SetupDefaultStates();
    }

    public void Initialize(EmployeeService employeeService, ExpenseService expenseService, PhotoService photoService, FormCacheService? formCache = null)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(expenseService);
        ArgumentNullException.ThrowIfNull(photoService);

        this.employeeService = employeeService;
        this.expenseService = expenseService;
        this.photoService = photoService;
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

        buttonSendExpenses.Click -= OnSendClick;
        textBoxAmountExpenses.KeyPress -= InputHelper.FilterAmountInput;
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
        textBoxAmountExpenses.KeyPress += InputHelper.FilterAmountInput;
        checkBoxPhotoSendExpenses.CheckedChanged += OnPhotoCheckChanged;

        textBoxAmountExpenses.TextChanged += (_, _) => SaveFieldToCache();
        textBoxCommentExpenses.TextChanged += (_, _) => SaveFieldToCache();
        checkBoxFromSafeExpenses.CheckedChanged += (_, _) => SaveFieldToCache();
        checkBoxPhotoSendExpenses.CheckedChanged += (_, _) => SaveFieldToCache();
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
        formCache?.ClearPrefix(CachePrefix);
    }

    private void SaveFieldToCache()
    {
        if (isRestoringCache || formCache == null) return;

        formCache.Set($"{CachePrefix}Amount", textBoxAmountExpenses.Text);
        formCache.Set($"{CachePrefix}Comment", textBoxCommentExpenses.Text);
        formCache.Set($"{CachePrefix}FromSafe", checkBoxFromSafeExpenses.Checked ? "1" : "0");
        formCache.Set($"{CachePrefix}PhotoSend", checkBoxPhotoSendExpenses.Checked ? "1" : "0");
    }

    private void RestoreCachedValues()
    {
        if (formCache == null) return;

        isRestoringCache = true;
        try
        {
            var amount = formCache.Get($"{CachePrefix}Amount");
            if (!string.IsNullOrEmpty(amount)) textBoxAmountExpenses.Text = amount;

            var comment = formCache.Get($"{CachePrefix}Comment");
            if (!string.IsNullOrEmpty(comment)) textBoxCommentExpenses.Text = comment;

            var fromSafe = formCache.Get($"{CachePrefix}FromSafe");
            if (fromSafe != null) checkBoxFromSafeExpenses.Checked = fromSafe == "1";

            var photoSend = formCache.Get($"{CachePrefix}PhotoSend");
            if (photoSend != null) checkBoxPhotoSendExpenses.Checked = photoSend == "1";
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