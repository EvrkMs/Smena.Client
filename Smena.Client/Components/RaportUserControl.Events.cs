using Host.Grpc.Services.Employee;
using Host.Grpc.Services.Raport;
using MaterialSkin.Controls;
using Smena.Client.Helpers;
using Smena.Client.Models;
using Smena.Client.Services;

namespace Smena.Client.Components;

public partial class RaportUserControl
{
    private async void OnCalculateClick(object? sender, EventArgs e)
    {
        await RefreshSafeFromServerAsync();

        if (employeesData.Count == 0)
        {
            MessageBox.Show(
                "Выберите хотя бы одного сотрудника!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var report = GenerateReport();
        if (!ConfirmSafeDiscrepancy(report, isSendAction: false))
        {
            return;
        }

        if (!ValidateMinusTotals(report, showMessage: true))
        {
            return;
        }

        if (!await ValidateEmployeeMinusRulesAsync(showMessage: true))
        {
            return;
        }

        UpdateReportPreview(null, EventArgs.Empty);

        MessageBox.Show(
            "Расчёт выполнен успешно! Проверьте данные и нажмите 'Отправить' для отправки отчёта.",
            "Расчёт завершён",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private async void OnSendClick(object? sender, EventArgs e)
    {
        await RefreshSafeFromServerAsync();

        if (employeesData.Count == 0)
        {
            MessageBox.Show(
                "Выберите хотя бы одного сотрудника!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var report = GenerateReport();
        if (!ConfirmSafeDiscrepancy(report, isSendAction: true))
        {
            return;
        }

        var totalMinusCash = Math.Abs(report.CashDiscrepancy);
        var totalMinusSafe = report.SafeDiscrepancy < 0 ? -report.SafeDiscrepancy : 0;
        var totalMinusEntered = employeesData.Sum(e => e.Minus);

        if (!ValidateMinusTotals(report, showMessage: true))
        {
            return;
        }

        if (!await ValidateEmployeeMinusRulesAsync(showMessage: true))
        {
            return;
        }

        var confirmMessage = $"Отчёт за смену:\n\n" +
                           $"Выручка: {report.Revenue} руб.\n" +
                           $"ЗП сотрудников: {report.TotalSalary} руб.\n" +
                           $"Итог: {report.Total} руб.\n\n";

        if (totalMinusCash > 0 || totalMinusSafe > 0)
        {
            confirmMessage += $"Минус по кассе: {totalMinusCash} руб.\n" +
                            $"Минус по сейфу: {totalMinusSafe} руб.\n" +
                            $"Всего минусов: {totalMinusCash + totalMinusSafe} руб.\n" +
                            $"Указано минусов: {totalMinusEntered} руб.\n\n";
        }

        confirmMessage += "Отправить отчёт?";

        var result = MessageBox.Show(
            confirmMessage,
            "Подтверждение отправки",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        listBoxSendInformation.Items.Clear();
        string? photoSessionKey = null;
        var firstEmployee = employeesData.FirstOrDefault()?.Employee;
        if (firstEmployee == null)
        {
            MessageBox.Show(
                "Выберите первого сотрудника для получения фото.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        buttonSend.Enabled = false;
        buttonСalculate.Enabled = false;

        try
        {
            var photoResult = await photoService!.RequestPhotosAsync(
                firstEmployee.Id,
                message => AddSendInfo(message));

            if (!photoResult.Success || string.IsNullOrWhiteSpace(photoResult.SessionKey))
            {
                MessageBox.Show(
                    photoResult.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            photoSessionKey = photoResult.SessionKey;

            var request = new GrpcRaportRequest
            {
                FactCash = report.FactCash,
                FactNonCash = report.FactNonCash,
                ProgramCash = report.ProgramCash,
                ProgramNonCash = report.ProgramNonCash,
                FactSafe = report.FactSafe,
                WhyMinus = textBoxWhyMinus.Text,
                SendPhoto = true,
                PhotoSessionKey = photoSessionKey ?? string.Empty
            };

            foreach (var emp in report.Employees)
            {
                request.Employees.Add(new EmployeeRaportSalary
                {
                    EmployeeId = emp.Employee?.Id ?? "",
                    Hours = emp.Hours,
                    Minus = emp.Minus,
                });
            }

            var (success, message) = await raportService!.SendRaportAsync(request);

            if (success)
            {
                MessageBox.Show(
                    message,
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ClearAllFields();
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
            buttonSend.Enabled = true;
            buttonСalculate.Enabled = true;
        }
    }

    private void Event_ListEmployeeChange(object? sender, System.ComponentModel.ListChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => Event_ListEmployeeChange(sender, e));
            return;
        }

        employeeSalaryCache.Clear();

        var employees = employeeService?.Employees.ToList() ?? [];
        var wasEnabled = _cacheEnabled;
        _cacheEnabled = false;
        try
        {
            EmployeeComboHelper.ReloadAll(comboBoxes, employees, nullComboBox);
        }
        finally
        {
            _cacheEnabled = wasEnabled;
        }
        if (wasEnabled) RestoreCachedEmployees();
    }

    private void Event_SafeChanged(object? sender, long newSafe)
    {
        if (InvokeRequired)
        {
            Invoke(() => Event_SafeChanged(sender, newSafe));
            return;
        }

        UpdateReportPreview(sender, EventArgs.Empty);
    }

    private async void OnFactSafeChanged(object? sender, EventArgs e)
    {
        await RefreshSafeFromServerAsync();
    }

    private void DataSalaryRaport(object? sender, EventArgs e)
    {
        if (isUpdating) return;

        isUpdating = true;
        try
        {
            var changedControl = sender as Control;
            int employeeIndex = GetEmployeeIndex(changedControl);

            if (employeeIndex >= 0)
            {
                UpdateEmployeeData(employeeIndex);
            }
            else
            {
                RebuildEmployeesList();
            }
        }
        finally
        {
            isUpdating = false;
        }
    }

    private void UpdateReportPreview(object? sender, EventArgs e)
    {
        if (isUpdating) return;

        RebuildEmployeesList();
        var report = GenerateReport();
        var preview = FormatReportPreview(report);

        listBoxRaport.Items.Clear();
        foreach (var line in preview.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                listBoxRaport.Items.Add(line);
            }
        }
    }

    private void AddSendInfo(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => AddSendInfo(message));
            return;
        }

        listBoxSendInformation.Items.Add(message);
    }

    private async Task RefreshSafeFromServerAsync()
    {
        if (safeService == null) return;

        try
        {
            await safeService.RefreshCurrentSafeAsync();
        }
        catch
        {
            // keep the last known safe value if refresh fails
        }
    }

    private void FilterNumericInputRaport(object? sender, KeyPressEventArgs e)
    {
        if (sender is not MaterialTextBox textBox) return;
        bool isHoursField = hoursTextBoxes.Contains(textBox);
        InputHelper.FilterNumericInput(sender, e, isHoursField ? ShiftConstants.MaxHoursDigits : ShiftConstants.MaxAmountDigits);
    }

    private void NormalizeNumericInput(object? sender, EventArgs e)
    {
        if (isNormalizingInput || sender is not MaterialTextBox textBox) return;

        var text = textBox.Text ?? string.Empty;
        var digitsOnly = new string(text.Where(char.IsDigit).ToArray());

        if (digitsOnly == text) return;

        isNormalizingInput = true;
        try
        {
            textBox.Text = digitsOnly;
            textBox.SelectionStart = digitsOnly.Length;
        }
        finally
        {
            isNormalizingInput = false;
        }
    }

    private string FormatReportPreview(ReportData report)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(report.Date.ToString("dd.MM.yyyy HH:mm"));
        sb.AppendLine();
        sb.AppendLine($"Нал: {report.FactCash}");
        sb.AppendLine($"Б/Н: {report.FactNonCash}");
        sb.AppendLine($"Выручка: {report.Revenue}");
        sb.AppendLine($"Итог: {report.Total}");
        sb.AppendLine();
        sb.AppendLine($"Минус по кассе: {report.CashDiscrepancy}");
        sb.AppendLine($"{GetSafeDiscrepancyCaption(report.SafeDiscrepancy)}: {Math.Abs(report.SafeDiscrepancy)}");
        sb.AppendLine();
        sb.AppendLine($"Факт сейфа: {report.FactSafe}");
        sb.AppendLine($"Теперь сейфа: {report.NewSafe}");
        sb.AppendLine();
        sb.AppendLine("==програмные данные==");
        sb.AppendLine($"Нал: {report.ProgramCash}");
        sb.AppendLine($"Безнал: {report.ProgramNonCash}");
        sb.AppendLine($"Сейф: {report.ProgramSafe}");

        if (report.TotalSalary > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Всего ЗП: {report.TotalSalary} руб.");
        }

        return sb.ToString();
    }

    private static string GetSafeDiscrepancyCaption(int safeDiscrepancy) =>
        safeDiscrepancy switch
        {
            > 0 => "Плюс по сейфу",
            < 0 => "Минус по сейфу",
            _ => "Расхождение по сейфу"
        };
}
