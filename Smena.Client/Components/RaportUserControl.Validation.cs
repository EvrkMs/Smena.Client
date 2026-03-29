using Host.Grpc.Services.Employee;
using MaterialSkin.Controls;
using Smena.Client.Models;
using Smena.Client.Services;

namespace Smena.Client.Components;

public partial class RaportUserControl
{
    private void ValidateTotalHours(object? sender, EventArgs e)
    {
        if (isValidatingHours || sender is not MaterialTextBox changedBox) return;

        const int MaxTotalHours = ShiftConstants.MaxHoursPerShift;

        int totalHours = 0;
        for (int i = 0; i < hoursTextBoxes.Count; i++)
        {
            if (!IsRowActive(i)) continue;

            if (int.TryParse(hoursTextBoxes[i].Text, out var hours))
            {
                totalHours += hours;
            }
        }

        if (totalHours > MaxTotalHours)
        {
            isValidatingHours = true;
            try
            {
                if (previousHoursValues.TryGetValue(changedBox, out var prevValue))
                {
                    changedBox.Text = prevValue;
                }
                else
                {
                    changedBox.Clear();
                }

                MessageBox.Show(
                    $"Суммарное количество часов не может превышать {MaxTotalHours} часов!",
                    "Ограничение по часам",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            finally
            {
                isValidatingHours = false;
            }
        }
        else
        {
            previousHoursValues[changedBox] = changedBox.Text;
        }
    }

    private async void ValidateMinusInputAsync(object? sender, EventArgs e)
    {
        if (isValidatingMinus || isNormalizingInput || sender is not MaterialTextBox minusBox)
            return;

        var index = minusTextBoxes.IndexOf(minusBox);
        if (index < 0 || employeeService == null)
            return;

        if (!IsRowActive(index) ||
            comboBoxes[index].SelectedItem is not GrpcEmployee employee ||
            employee == nullComboBox)
            return;

        if (!int.TryParse(minusBox.Text, out var minus) || minus <= 0)
            return;

        var salaryCheck = await GetCurrentSalaryAsync(employee.Id);
        if (!salaryCheck.Success || salaryCheck.CurrentSalary >= 0)
            return;

        var hours = int.TryParse(hoursTextBoxes[index].Text, out var parsedHours) ? parsedHours : 0;
        var effectiveHourlyRate = GetEffectiveHourlyRate(employee);
        var maxMinus = hours * effectiveHourlyRate;
        if (minus <= maxMinus)
            return;

        isValidatingMinus = true;
        try
        {
            minusBox.Text = maxMinus.ToString();
            minusBox.SelectionStart = minusBox.TextLength;

            MessageBox.Show(
                $"Для сотрудника {employee.Name} текущая ЗП отрицательная ({salaryCheck.CurrentSalary} руб.).\n" +
                $"Минус ограничен {maxMinus} руб. (часы {hours} * ставка {effectiveHourlyRate}).",
                "Ограничение минуса",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        finally
        {
            isValidatingMinus = false;
        }
    }

    private bool ConfirmSafeDiscrepancy(ReportData report, bool isSendAction)
    {
        if (report.SafeDiscrepancy == 0)
            return true;

        var discrepancyTitle = GetSafeDiscrepancyCaption(report.SafeDiscrepancy);
        var discrepancyAmount = Math.Abs(report.SafeDiscrepancy);
        var message =
            $"Актуальное значение сейфа обновлено: {report.ProgramSafe} руб.\n" +
            $"Указано по факту: {report.FactSafe} руб.\n" +
            $"{discrepancyTitle}: {discrepancyAmount} руб.";

        if (!isSendAction)
        {
            MessageBox.Show(
                message,
                "Расхождение по сейфу",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return true;
        }

        var result = MessageBox.Show(
            $"{message}\n\nПродолжить отправку отчёта?",
            "Расхождение по сейфу",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        return result == DialogResult.Yes;
    }

    private bool ValidateMinusTotals(ReportData report, bool showMessage)
    {
        var totalMinusCash = Math.Abs(report.CashDiscrepancy);
        var totalMinusSafe = report.SafeDiscrepancy < 0 ? -report.SafeDiscrepancy : 0;
        var totalMinusEntered = employeesData.Sum(e => e.Minus);
        var totalMinusExpected = totalMinusCash + totalMinusSafe;

        if (totalMinusEntered == totalMinusExpected)
            return true;

        if (showMessage)
        {
            MessageBox.Show(
                $"Сумма минусов должна быть равна {totalMinusExpected} руб.\n" +
                $"Указано минусов: {totalMinusEntered} руб.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        return false;
    }

    private async Task<bool> ValidateEmployeeMinusRulesAsync(bool showMessage)
    {
        if (employeeService == null)
            return false;

        employeeSalaryCache.Clear();

        foreach (var emp in employeesData)
        {
            if (emp.Employee == null || string.IsNullOrWhiteSpace(emp.Employee.Id))
                continue;

            var salaryCheck = await GetCurrentSalaryAsync(emp.Employee.Id);
            if (!salaryCheck.Success)
            {
                if (showMessage)
                {
                    MessageBox.Show(
                        salaryCheck.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                return false;
            }

            if (salaryCheck.CurrentSalary >= 0)
                continue;

            var effectiveHourlyRate = GetEffectiveHourlyRate(emp.Employee);
            var maxMinus = emp.Hours * effectiveHourlyRate;
            if (emp.Minus <= maxMinus)
                continue;

            if (showMessage)
            {
                MessageBox.Show(
                    $"Для сотрудника {emp.Employee.Name} при отрицательной ЗП ({salaryCheck.CurrentSalary} руб.) " +
                    $"минус не может превышать {maxMinus} руб. (часы {emp.Hours} * ставка {effectiveHourlyRate}).",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            return false;
        }

        return true;
    }

    private async Task<(bool Success, int CurrentSalary, string Message)> GetCurrentSalaryAsync(string employeeId)
    {
        if (employeeSalaryCache.TryGetValue(employeeId, out var cachedSalary))
            return (true, cachedSalary, string.Empty);

        if (employeeService == null)
            return (false, 0, "Сервис сотрудников недоступен.");

        var salaryCheck = await employeeService.GetCurrentSalaryAsync(employeeId);
        if (salaryCheck.Success)
        {
            employeeSalaryCache[employeeId] = salaryCheck.CurrentSalary;
        }

        return salaryCheck;
    }

    private int GetEffectiveHourlyRate(GrpcEmployee? employee)
    {
        if (employee == null)
            return 0;

        if (employeeService != null)
        {
            var latest = employeeService.Employees.FirstOrDefault(e => e.Id == employee.Id);
            if (latest != null)
                return latest.HourlyRate;
        }

        return employee.HourlyRate;
    }
}
