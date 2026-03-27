using MaterialSkin.Controls;

namespace Smena.Client.Helpers;

/// <summary>
/// Shared UI helpers extracted from individual UserControls to eliminate duplication.
/// </summary>
internal static class InputHelper
{
    /// <summary>
    /// Restricts a MaterialTextBox to digits only, with a configurable max length.
    /// Attach to <see cref="Control.KeyPress"/>.
    /// </summary>
    public static void FilterNumericInput(object? sender, KeyPressEventArgs e, int maxLength = 6)
    {
        if (sender is not MaterialTextBox textBox) return;

        if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
        {
            e.Handled = true;
            return;
        }

        if (!char.IsControl(e.KeyChar))
        {
            int newLength = textBox.TextLength - textBox.SelectionLength + 1;
            if (newLength > maxLength)
            {
                e.Handled = true;
            }
        }
    }

    /// <summary>Numeric filter with max 6 digits (amounts).</summary>
    public static void FilterAmountInput(object? sender, KeyPressEventArgs e)
        => FilterNumericInput(sender, e, 6);

    /// <summary>Numeric filter with max 2 digits (hours).</summary>
    public static void FilterHoursInput(object? sender, KeyPressEventArgs e)
        => FilterNumericInput(sender, e, 2);
}
