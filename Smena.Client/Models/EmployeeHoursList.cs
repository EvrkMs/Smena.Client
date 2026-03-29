using Host.Grpc.Services.Employee;

namespace Smena.Client.Models;

public class EmployeeHoursList
{
    public GrpcEmployee? Employee { get; set; }
    public int Hours { get; set; }
    public int Minus { get; set; }
    public int HourlyRate => Employee?.HourlyRate ?? 0;
    public int Salary => (Hours * HourlyRate) - Minus;

    public string SalaryInfo => ToString();

    public override string ToString() =>
        Employee?.Name != null ? $"{Employee.Name} - {Salary} руб." : "";
}
