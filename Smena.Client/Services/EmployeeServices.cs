using Google.Protobuf.WellKnownTypes;
using Host.Grpc.Services.Employee;
using Smena.Client.Helpers;
using System.ComponentModel;

namespace Smena.Client.Services;

public class EmployeeService(GrpcService grpcService)
{
    private readonly GrpcEmployeeService.GrpcEmployeeServiceClient _client = new(grpcService.CallInvoker);
    public BindingList<GrpcEmployee> Employees { get; } = [];

    /// <summary>
    /// Ошибки загрузки НЕ глотаются (раньше catch молчал, и при недоступном сервере
    /// UI показывал "Список пуст" без единой ошибки — офлайн был неотличим от пустой
    /// базы). Исключение уходит через мост в JS как rejected Promise: Engine покажет
    /// тост, а App включит бейдж "нет связи" и полинг.
    /// </summary>
    public async Task LoadOrReloadListAsync(CancellationToken ct = default)
    {
        var response = await _client.EmployeesListAsync(new Empty(), cancellationToken: ct);
        if (response?.Employees == null) return;

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

    public async Task<(bool Success, string Message)> AddEmployeeAsync(
        GrpcEmployee employee,
        CancellationToken ct = default)
    {
        try
        {
            var res = await _client.EmployeeAddAsync(employee, cancellationToken: ct);
            if (res == null || !res.Success)
            {
                return (false, res?.Message ?? "Server returned empty response");
            }

            try
            {
                await LoadOrReloadListAsync(ct);
            }
            catch (Exception ex)
            {
                // Добавление уже прошло на сервере — не превращаем сбой обновления
                // списка в ложную ошибку добавления; UI перезапросит список сам.
                ErrorLog.Write("EmployeeList reload after add", ex);
            }
            return (true, res.Message ?? string.Empty);
        }
        catch (Exception ex)
        {
            ErrorLog.Write("EmployeeAdd", ex);
            return (false, ex.Message);
        }
    }

    public Task<(bool Success, int CurrentSalary, string Message)> GetCurrentSalaryAsync(
        string employeeId,
        CancellationToken ct = default)
        => GrpcCallHelper.CallAsync(
            () => _client.EmployeeCurrentSalaryAsync(
                new GrpcEmployeeSalaryRequest { EmployeeId = employeeId },
                cancellationToken: ct).ResponseAsync,
            response => response == null
                ? (false, 0, "Server returned empty response.")
                : (true, (int)response.CurrentSalary, string.Empty),
            error => (false, 0, error));
}
