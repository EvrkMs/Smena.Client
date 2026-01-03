using MaterialSkin.Controls;

namespace Smena.Client.Components;

public partial class EmployeeHoursBlock : UserControl
{
    public MaterialComboBox EmployeeComboBox => comboBoxEmployee;
    public MaterialTextBox HoursTextBox => textBoxHours;

    public event EventHandler? RemoveRequested;

    public EmployeeHoursBlock()
    {
        InitializeComponent();
    }

    private void ButtonRemove_Click(object sender, EventArgs e)
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}
