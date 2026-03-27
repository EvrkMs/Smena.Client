using Host.Grpc.Services.Safe;
using MaterialSkin.Controls;
using Smena.Client.Services;

namespace Smena.Client.Components;

public partial class ComingUserControl : UserControl
{
    private SafeService? _safeService;
    private FormCacheService? formCache;
    private bool isRestoringCache;

    private const string CachePrefix = "Coming.";

    public ComingUserControl()
    {
        InitializeComponent();
    }

    public void Initialize(SafeService safeService, FormCacheService? formCache = null)
    {
        ArgumentNullException.ThrowIfNull(safeService);

        _safeService = safeService;
        this.formCache = formCache;
        SubscribeToEvents();
        RestoreCachedValues();
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

        textBoxAmountPlusSafe.TextChanged += (_, _) => SaveFieldToCache();
        textBoxCommentPlusAmount.TextChanged += (_, _) => SaveFieldToCache();
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
        formCache?.ClearPrefix(CachePrefix);
    }

    private void SaveFieldToCache()
    {
        if (isRestoringCache || formCache == null) return;

        formCache.Set($"{CachePrefix}Amount", textBoxAmountPlusSafe.Text);
        formCache.Set($"{CachePrefix}Comment", textBoxCommentPlusAmount.Text);
    }

    private void RestoreCachedValues()
    {
        if (formCache == null) return;

        isRestoringCache = true;
        try
        {
            var amount = formCache.Get($"{CachePrefix}Amount");
            if (!string.IsNullOrEmpty(amount)) textBoxAmountPlusSafe.Text = amount;

            var comment = formCache.Get($"{CachePrefix}Comment");
            if (!string.IsNullOrEmpty(comment)) textBoxCommentPlusAmount.Text = comment;
        }
        finally
        {
            isRestoringCache = false;
        }
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