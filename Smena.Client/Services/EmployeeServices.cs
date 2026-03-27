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
    }

    /// <summary>
    /// Must be called from the UI thread (or via Invoke) to safely update Employees.
    /// </summary>
    public async Task LoadOrReloadListAsync()
    {
        try
        {
            var response = await _client.EmployeesListAsync(new Empty());
            if (response?.Employees == null) return;

            // Build the list off-thread, then swap on the current (UI) context.
            var items = response.Employees.ToList();

            Employees.RaiseListChangedEvents = false;
            try
            {
                Employees.Clear();
                foreach (var employee in items)
                {
                    Employees.Add(employee);
                }
            }
            finally
            {
                Employees.RaiseListChangedEvents = true;
                Employees.ResetBindings();
            }
        }
        catch (RpcException)
        {
            // Silently ignore on load — the list stays empty.
        }
    }

    public async Task<(bool Success, string Message)> AddEmployeeAsync(GrpcEmployee employee)
    {
        try
        {
            var res = await _client.EmployeeAddAsync(employee);
            if (res == null || !res.Success)
            {
                return (false, res?.Message ?? "Server returned empty response");
            }

            await LoadOrReloadListAsync();
            return (true, res.Message ?? string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, int CurrentSalary, string Message)> GetCurrentSalaryAsync(
        string employeeId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _client.EmployeeCurrentSalaryAsync(
                new GrpcEmployeeSalaryRequest { EmployeeId = employeeId },
                cancellationToken: ct);

            if (response == null)
            {
                return (false, 0, "Server returned empty response.");
            }

            return (true, (int)response.CurrentSalary, string.Empty);
        }
        catch (RpcException ex)
        {
            return (false, 0, $"gRPC: {ex.StatusCode} - {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            return (false, 0, ex.Message);
        }
    }
}
