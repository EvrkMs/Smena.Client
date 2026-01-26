using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Host.Grpc.Common;
using Host.Grpc.Services.Employee;
using System.ComponentModel;

namespace Smena.Client.Services;

public class EmployeeService
{
    private readonly GrpcEmployeeService.GrpcEmployeeServiceClient _client;
    public BindingList<GrpcEmployee> Employees { get; } = [];

    public EmployeeService(GrpcService grpcService)
    {
        _client = new(grpcService.CallInvoker);
        _ = LoadOrReloadListAsync();
    }

    public async Task LoadOrReloadListAsync()
    {
        try
        {
            var response = await _client.EmployeesListAsync(new Empty());
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
            var res = await _client.EmployeeAddAsync(employee);
            if (res == null || !res.Success)
            {
                MessageBox.Show(res?.Message ?? "Server returned empty response");
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
