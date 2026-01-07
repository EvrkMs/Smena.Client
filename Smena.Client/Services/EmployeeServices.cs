using Grpc.Core;
using Grpc.Net.Client;
using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Requests;
using System.ComponentModel;

namespace Smena.Client.Services;

public class EmployeeService
{
    private readonly GrpcEmployeeService.GrpcEmployeeServiceClient Client;
    public BindingList<GrpcEmployee> Employees { get; private set; } = [];


    public EmployeeService(GrpcChannel _channel)
    {
        Client = new(_channel);

        _ = LoadOrReloadListAsync();
    }

    public async Task LoadOrReloadListAsync()
    {
        try
        {
            var response = await Client.EmployeesListAsync(new EmptyRequest());

            if (response?.Employees == null) return;

            Employees.Clear();
            foreach (var employee in response.Employees)
            {
                Employees.Add(employee);
            }
            Employees.ResetBindings();
        }
        catch (RpcException ex)
        {
            MessageBox.Show($"gRPC: {ex.StatusCode} - {ex.Message}");
        }
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
            await LoadOrReloadListAsync();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return false;
        }
    }
}
