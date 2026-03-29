using Host.Grpc.Services.Employee;
using Smena.Client.Services;

namespace Smena.Client.Components;

public partial class RaportUserControl
{
    private void SaveFieldToCache(object? sender, EventArgs e)
    {
        if (isRestoringCache || formCache == null) return;

        formCache.Set(CachePrefix + "FactCash", textBoxFactCash.Text);
        formCache.Set(CachePrefix + "FactNonCash", textBoxFactNonCash.Text);
        formCache.Set(CachePrefix + "ProgramCash", textBoxProgramCash.Text);
        formCache.Set(CachePrefix + "ProgramNonCash", textBoxProgramNonCash.Text);
        formCache.Set(CachePrefix + "Safe", textBoxSafe.Text);
        formCache.Set(CachePrefix + "WhyMinus", textBoxWhyMinus.Text);

        for (var i = 0; i < comboBoxes.Count; i++)
        {
            var emp = comboBoxes[i].SelectedItem as GrpcEmployee;
            formCache.Set(CachePrefix + $"Employee{i}", emp != null && emp != nullComboBox ? emp.Name : null);
            formCache.Set(CachePrefix + $"Hours{i}", hoursTextBoxes[i].Text);
            formCache.Set(CachePrefix + $"Minus{i}", minusTextBoxes[i].Text);
        }
    }

    private void RestoreCachedValues()
    {
        if (formCache == null) return;

        isRestoringCache = true;
        try
        {
            textBoxFactCash.Text = formCache.Get(CachePrefix + "FactCash") ?? string.Empty;
            textBoxFactNonCash.Text = formCache.Get(CachePrefix + "FactNonCash") ?? string.Empty;
            textBoxProgramCash.Text = formCache.Get(CachePrefix + "ProgramCash") ?? string.Empty;
            textBoxProgramNonCash.Text = formCache.Get(CachePrefix + "ProgramNonCash") ?? string.Empty;
            textBoxSafe.Text = formCache.Get(CachePrefix + "Safe") ?? string.Empty;
            textBoxWhyMinus.Text = formCache.Get(CachePrefix + "WhyMinus") ?? string.Empty;

            for (var i = 0; i < comboBoxes.Count; i++)
            {
                var cachedName = formCache.Get(CachePrefix + $"Employee{i}");
                if (!string.IsNullOrWhiteSpace(cachedName))
                {
                    for (var j = 0; j < comboBoxes[i].Items.Count; j++)
                    {
                        if (comboBoxes[i].Items[j] is GrpcEmployee emp &&
                            string.Equals(emp.Name, cachedName, StringComparison.Ordinal))
                        {
                            comboBoxes[i].SelectedIndex = j;
                            break;
                        }
                    }
                }

                hoursTextBoxes[i].Text = formCache.Get(CachePrefix + $"Hours{i}") ?? string.Empty;
                minusTextBoxes[i].Text = formCache.Get(CachePrefix + $"Minus{i}") ?? string.Empty;
            }
        }
        finally
        {
            isRestoringCache = false;
        }
    }

    private void ClearAllFields()
    {
        textBoxFactCash.Clear();
        textBoxFactNonCash.Clear();
        textBoxProgramCash.Clear();
        textBoxProgramNonCash.Clear();
        textBoxSafe.Clear();
        textBoxWhyMinus.Clear();

        foreach (var comboBox in comboBoxes)
        {
            comboBox.SelectedIndex = 0;
        }

        ClearAllTextBoxes();

        employeesData.Clear();
        employeeSalaryCache.Clear();
        listBoxRaport.Items.Clear();
        listBoxSendInformation.Items.Clear();

        formCache?.ClearPrefix(CachePrefix);
    }

    private void ClearAllTextBoxes()
    {
        foreach (var textBox in hoursTextBoxes.Concat(minusTextBoxes))
        {
            textBox.Clear();
        }

        foreach (var hoursBox in hoursTextBoxes)
        {
            previousHoursValues[hoursBox] = "0";
        }
    }

    private void ClearSecondAndThirdTextBoxes()
    {
        textBoxHoursSecondNameRaport.Clear();
        textBoxMinusSecondNameRaport.Clear();
        previousHoursValues[textBoxHoursSecondNameRaport] = "0";

        ClearThirdTextBoxes();
    }

    private void ClearThirdTextBoxes()
    {
        textBoxHoursThirdNameRaport.Clear();
        textBoxMinusThirdNameRaport.Clear();
        previousHoursValues[textBoxHoursThirdNameRaport] = "0";
    }
}
