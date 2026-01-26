using Host.Grpc.Services.Safe;
using MaterialSkin.Controls;
using Smena.Client.Services;

namespace Smena.Client.Components;

public partial class ComingUserControl : UserControl
{
    private SafeService? _safeService;

    public ComingUserControl()
    {
        InitializeComponent();
    }

    public void Initialize(SafeService safeService)
    {
        ArgumentNullException.ThrowIfNull(safeService);

        _safeService = safeService;
        SubscribeToEvents();
    }

    public void UnsubscribeFromEvents()
    {
        buttonSendPlusSafe.Click -= OnSendClick;
        textBoxAmountPlusSafe.KeyPress -= FilterNumericInput;
    }

    private void SubscribeToEvents()
    {
        buttonSendPlusSafe.Click += OnSendClick;
        textBoxAmountPlusSafe.KeyPress += FilterNumericInput;
    }

    private void FilterNumericInput(object? sender, KeyPressEventArgs e)
    {
        if (sender is not MaterialTextBox textBox) return;

        if (textBox == textBoxCommentPlusAmount)
            return;

        if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
        {
            e.Handled = true;
            return;
        }

        if (!char.IsControl(e.KeyChar))
        {
            int newLength = textBox.TextLength - textBox.SelectionLength + 1;
            if (newLength > 6)
            {
                e.Handled = true;
            }
        }
    }

    private async void OnSendClick(object? sender, EventArgs e)
    {
        if (!int.TryParse(textBoxAmountPlusSafe.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show(
                "Введите корректную сумму прихода!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var request = new SafeOperationAdd
        {
            Amount = amount,
            Comment = textBoxCommentPlusAmount.Text,
            Type = SafeOperationTypeGrpc.Coming
        };

        var confirmMessage = $"Добавить приход {request.Amount} руб. в сейф?";

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            confirmMessage += $"\nКомментарий: {request.Comment}";
        }

        var result = MessageBox.Show(
            confirmMessage,
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        buttonSendPlusSafe.Enabled = false;

        try
        {
            var respons = await _safeService!.AddOperationSafeAsync(request);

            if (respons.Success)
            {
                MessageBox.Show(
                    respons.Message,
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ClearFields();
            }
            else
            {
                MessageBox.Show(
                    respons.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        finally
        {
            buttonSendPlusSafe.Enabled = true;
        }
    }

    private void ClearFields()
    {
        textBoxAmountPlusSafe.Clear();
        textBoxCommentPlusAmount.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnsubscribeFromEvents();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}