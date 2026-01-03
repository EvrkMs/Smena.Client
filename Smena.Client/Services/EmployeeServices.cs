using Grpc.Core;
using Grpc.Net.Client;
using Host.Grpc.Services.Employee;
using System.ComponentModel;

namespace Smena.Client.Services;

public class EmployeeService(GrpcChannel _channel)
{
    private readonly GrpcEmployeeService.GrpcEmployeeServiceClient Client = new(_channel);

    public BindingList<GrpcEmployee> Employees { get; private set; } = [];

    public async void LoadOrReloadList()
    {
        var response = await Client.EmployeesListAsync(new GrpcRequest());

        if (response == null)
        {
            MessageBox.Show("Ошибка: ответ вернулся null");
            return;
        }

        Employees.Clear();
        foreach (var employee in response.Employees)
        {
            Employees.Add(employee);
        }

        Employees.ResetBindings();
    }

    public async Task<bool> AddEmployeeAsync(GrpcEmployee employee)
    {
        try
        {
            var res = await Client.EmployeeAddAsync(employee);
            if (res == null)
            {
                MessageBox.Show("Ошибка: создание сотрудника вернуло null");
                return false;
            }
            LoadOrReloadList();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return false;
        }
    }
}
