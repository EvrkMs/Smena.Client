namespace Smena.Client.Models;

public class ReportData
{
    public DateTime Date { get; set; }
    public int FactCash { get; set; }
    public int FactNonCash { get; set; }
    public int Revenue { get; set; }
    public int Total { get; set; }
    public int CashDiscrepancy { get; set; }
    public int SafeDiscrepancy { get; set; }
    public int FactSafe { get; set; }
    public int NewSafe { get; set; }
    public int ProgramCash { get; set; }
    public int ProgramNonCash { get; set; }
    public long ProgramSafe { get; set; }
    public List<EmployeeHoursList> Employees { get; set; } = [];
    public int TotalSalary { get; set; }

    public const int InitialCash = ShiftConstants.InitialCashRegister;
}
