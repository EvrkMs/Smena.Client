using Host.Grpc.Services.Employee;

namespace Smena.Client.Helpers;

internal static class EmployeeComboHelper
{
    /// <summary>
    /// Reloads a ComboBox with the latest employee list, prepending a "none" entry.
    /// Optionally preserves the current selection by employee ID.
    /// </summary>
    public static void Reload(
        ComboBox comboBox,
        IReadOnlyList<GrpcEmployee> employees,
        GrpcEmployee nullEntry,
        bool preserveSelection = true)
    {
        var selectedId = preserveSelection
            ? (comboBox.SelectedItem as GrpcEmployee)?.Id
            : null;

        comboBox.DataSource = null;
        comboBox.DisplayMember = nameof(GrpcEmployee.Name);
        comboBox.ValueMember = nameof(GrpcEmployee.Id);

        List<GrpcEmployee> list = [nullEntry, .. employees];
        comboBox.DataSource = list;

        if (selectedId == null) return;

        var employee = employees.FirstOrDefault(e => e.Id == selectedId);
        if (employee != null)
        {
            comboBox.SelectedItem = employee;
        }
    }

    /// <summary>
    /// Reloads multiple ComboBoxes with the same employee list.
    /// </summary>
    public static void ReloadAll(
        IReadOnlyList<ComboBox> comboBoxes,
        IReadOnlyList<GrpcEmployee> employees,
        GrpcEmployee nullEntry)
    {
        foreach (var comboBox in comboBoxes)
        {
            Reload(comboBox, employees, nullEntry);
        }
    }
}
