using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Inventory;
using MaterialSkin.Controls;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client.Components;

public partial class InventoryUserControl : UserControl
{
    private EmployeeService? employeeService;
    private InventoryService? inventoryService;

    public InventoryUserControl()
    {
        InitializeComponent();
    }

    public void Initialize(EmployeeService employeeService, InventoryService inventoryService)
    {
        ArgumentNullException.ThrowIfNull(employeeService);
        ArgumentNullException.ThrowIfNull(inventoryService);

        this.employeeService = employeeService;
        this.inventoryService = inventoryService;

        SubscribeToEvents();
    }

    public void UnsubscribeFromEvents()
    {
        if (employeeService != null)
        {
            employeeService.Employees.ListChanged -= Event_ListEmployeeChange;
        }

        buttonSendInventory.Click -= OnSendClick;
        textBoxAmountInventory.KeyPress -= FilterNumericInput;
        textBoxAmountInventory.TextChanged -= OnAmountChanged;
    }

    private void SubscribeToEvents()
    {
        if (employeeService != null)
        {
            employeeService.Employees.ListChanged += Event_ListEmployeeChange;
            LoadEmployees();
        }

        buttonSendInventory.Click += OnSendClick;
        textBoxAmountInventory.KeyPress += FilterNumericInput;
        textBoxAmountInventory.TextChanged += OnAmountChanged;
        listBoxNameInventory.SelectedIndexChanged += OnAmountChanged;
    }

    private void OnAmountChanged(object? sender, EventArgs e)
    {
        // Обновляем текст кнопки с показом суммы на человека
        if (int.TryParse(textBoxAmountInventory.Text, out var totalAmount) &&
            totalAmount > 0 &&
            listBoxNameInventory.SelectedItems.Count > 0)
        {
            var perEmployee = totalAmount / listBoxNameInventory.SelectedItems.Count;
            buttonSendInventory.Text = $"Отправить ({perEmployee} руб. на человека)";
        }
        else
        {
            buttonSendInventory.Text = "Отправить инвент";
        }
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
        var selectedIndices = listBoxNameInventory.SelectedIndices.Cast<int>().ToList();

        listBoxNameInventory.Items.Clear();
        listBoxNameInventory.DisplayMember = nameof(GrpcEmployee.Name);

        foreach (var employee in employees)
        {
            listBoxNameInventory.Items.Add(employee);
        }

        // Восстанавливаем выбранные элементы
        foreach (var index in selectedIndices)
        {
            if (index < listBoxNameInventory.Items.Count)
            {
                listBoxNameInventory.SetSelected(index, true);
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
        if (!int.TryParse(textBoxAmountInventory.Text, out var totalAmount) || totalAmount <= 0)
        {
            MessageBox.Show(
                "Введите корректную общую сумму инвентаризации!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (listBoxNameInventory.SelectedItems.Count == 0)
        {
            MessageBox.Show(
                "Выберите хотя бы одного сотрудника!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var employeesCount = listBoxNameInventory.SelectedItems.Count;
        var amountPerEmployee = totalAmount / employeesCount;

        // Проверяем, что сумма делится нацело
        if (totalAmount % employeesCount != 0)
        {
            var remainder = totalAmount % employeesCount;
            MessageBox.Show(
                $"Сумма {totalAmount} не делится нацело на {employeesCount} сотрудников. " +
                $"Остаток: {remainder} руб.\n\n" +
                $"Измените сумму или количество сотрудников.",
                "Внимание",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        var request = new GrpcInventoryRequest
        {
            TotalAmount = totalAmount
        };

        var selectedEmployees = new List<string>();

        foreach (GrpcEmployee employee in listBoxNameInventory.SelectedItems)
        {
            request.Employees.Add(new EmployeeInventory
            {
                EmployeeId = employee.Id
            });
            selectedEmployees.Add(employee.Name);
        }

        var confirmMessage = $"Инвентаризация на общую сумму {totalAmount} руб.\n" +
                           $"По {amountPerEmployee} руб. на каждого из {employeesCount} сотрудников:\n\n" +
                           string.Join(", ", selectedEmployees) + "\n\n" +
                           "Отправить?";

        var result = MessageBox.Show(
            confirmMessage,
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        buttonSendInventory.Enabled = false;

        try
        {
            var (success, message) = await inventoryService!.SendInventoryAsync(request);

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
            buttonSendInventory.Enabled = true;
        }
    }

    private void ClearFields()
    {
        textBoxAmountInventory.Clear();
        listBoxNameInventory.ClearSelected();
        buttonSendInventory.Text = "Отправить инвент";
    }
}