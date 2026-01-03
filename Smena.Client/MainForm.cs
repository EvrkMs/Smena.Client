using Grpc.Net.Client;
using Host.Grpc.Services;
using Host.Grpc.Services.Employee;
using MaterialSkin.Controls;
using Smena.Client.ModelForm;
using Smena.Client.Services;
using System.ComponentModel;

namespace Smena.Client;

public partial class MainForm : MaterialForm
{
    private readonly GrpcChannel _channel;
    private readonly EmployeeService _employeeService;
    private readonly SafeService _safeService;
    private FlowLayoutPanel _employeeHoursPanel;
    public MainForm()
    {
        InitializeComponent();

        // Создаём канал один раз
        _channel = new GrpcService("http://localhost:5000").Channel;
        _employeeService = new EmployeeService(_channel);
        _safeService = new SafeService(_channel);

        _employeeService.Employees.ListChanged += Employees_ListChanged;

        _employeeService.LoadOrReloadList();
    }

    private void Employees_ListChanged(object? sender, ListChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => Employees_ListChanged(sender, e));
            return;
        }

        employeeListView.Items.Clear(); // можно оптимизировать

        foreach (var emp in _employeeService.Employees)
        {
            employeeListView.Items.Add(emp.Name);
        }
    }

    private void EmployeeAddButton_Click(object sender, EventArgs e)
    {
        var addForm = new AddEmployee(_employeeService);
        var result = addForm.ShowDialog();

        if (result == DialogResult.OK && addForm.Success == true)
        {
            MessageBox.Show("Сотрудник успешно добавлен");
        }
        else if (result == DialogResult.Cancel)
        {
            MessageBox.Show("Добавление отменено");
        }
    }

    private void AddEmployeeAndHours_Click(object sender, EventArgs e)
    {

    }
}
