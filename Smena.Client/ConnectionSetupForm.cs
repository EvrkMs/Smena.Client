using System.Text.Json;
using System.Text.Json.Serialization;
using Smena.Client.Helpers;

namespace Smena.Client;

/// <summary>
/// Первый запуск: адрес сервера не найден ни в appsettings.json, ни в переменных
/// окружения. Показываем форму и сохраняем введённое в
/// %LOCALAPPDATA%\Smena.Client\appsettings.json. Рядом с exe не пишем сознательно:
/// в Program Files без прав администратора запись запрещена, а конфиг с API-ключом
/// в профиле пользователя — ровно то место, где ему и положено лежать.
/// </summary>
internal sealed class ConnectionSetupForm : Form
{
    private readonly TextBox addressBox;
    private readonly TextBox apiKeyBox;
    private readonly TextBox pathPrefixBox;
    private readonly Label errorLabel;

    public string AddressValue => addressBox.Text.Trim();
    public string ApiKeyValue => apiKeyBox.Text.Trim();
    public string PathPrefixValue => pathPrefixBox.Text.Trim();

    /// <summary>
    /// Показывает форму; при подтверждении пишет конфиг в <paramref name="savePath"/>
    /// и возвращает введённые значения. null — пользователь закрыл форму (выход).
    /// </summary>
    public static (string Address, string ApiKey, string? PathPrefix)? Prompt(
        string savePath, string? initialAddress, string? initialApiKey, string? initialPathPrefix)
    {
        using var form = new ConnectionSetupForm(initialAddress, initialApiKey, initialPathPrefix);
        if (form.ShowDialog() != DialogResult.OK)
        {
            return null;
        }

        var pathPrefix = string.IsNullOrWhiteSpace(form.PathPrefixValue) ? null : form.PathPrefixValue;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            File.WriteAllText(savePath, JsonSerializer.Serialize(
                new { Grpc = new { Address = form.AddressValue, ApiKey = form.ApiKeyValue, PathPrefix = pathPrefix } },
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }));
        }
        catch (Exception ex)
        {
            // Не удалось сохранить — работаем на введённых значениях этот запуск,
            // при следующем старте форма появится снова.
            ErrorLog.Write("ConnectionSetup save", ex);
            MessageBox.Show(
                $"Не удалось сохранить настройки:\n{ex.Message}\n\n" +
                "Приложение продолжит работу, но при следующем запуске параметры придётся ввести заново.",
                "Настройка подключения",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        return (form.AddressValue, form.ApiKeyValue, pathPrefix);
    }

    private ConnectionSetupForm(string? initialAddress, string? initialApiKey, string? initialPathPrefix)
    {
        Text = "Smena.Client — подключение";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(470, 352);
        BackColor = ColorTranslator.FromHtml("#12161C");
        ForeColor = ColorTranslator.FromHtml("#E7EBF0");
        Font = new Font("Segoe UI", 9.5f);

        Controls.Add(new Label
        {
            Text = "Настройка подключения",
            ForeColor = ColorTranslator.FromHtml("#C08B3E"),
            Font = new Font("Segoe UI", 14f),
            Location = new Point(24, 18),
            AutoSize = true
        });
        Controls.Add(new Label
        {
            Text = "Параметры сервера не найдены. Заполните поля — они сохранятся в\n" +
                   @"%LOCALAPPDATA%\Smena.Client\appsettings.json.",
            ForeColor = ColorTranslator.FromHtml("#8892A0"),
            Location = new Point(24, 52),
            Size = new Size(420, 34)
        });

        addressBox = AddField("Адрес сервера (например, https://example.com:5001)", 96, initialAddress);
        apiKeyBox = AddField("API-ключ (заголовок x-api-key; можно оставить пустым)", 152, initialApiKey);
        pathPrefixBox = AddField("Path prefix gRPC (необязательно, например /grpc)", 208, initialPathPrefix);

        errorLabel = new Label
        {
            ForeColor = ColorTranslator.FromHtml("#C0604A"),
            Location = new Point(24, 262),
            Size = new Size(420, 32)
        };
        Controls.Add(errorLabel);

        var saveButton = new Button
        {
            Text = "Сохранить",
            Location = new Point(228, 300),
            Size = new Size(120, 34),
            BackColor = ColorTranslator.FromHtml("#C08B3E"),
            ForeColor = ColorTranslator.FromHtml("#191307"),
            FlatStyle = FlatStyle.Flat
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) =>
        {
            if (!Uri.TryCreate(AddressValue, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                errorLabel.Text = "Адрес должен быть абсолютным URL со схемой http:// или https://.";
                return;
            }
            DialogResult = DialogResult.OK;
        };
        Controls.Add(saveButton);

        var exitButton = new Button
        {
            Text = "Выход",
            Location = new Point(358, 300),
            Size = new Size(88, 34),
            BackColor = ColorTranslator.FromHtml("#1A212B"),
            ForeColor = ColorTranslator.FromHtml("#8892A0"),
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel
        };
        exitButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#2A3340");
        Controls.Add(exitButton);

        AcceptButton = saveButton;
        CancelButton = exitButton;
    }

    private TextBox AddField(string label, int top, string? initialValue)
    {
        Controls.Add(new Label
        {
            Text = label,
            ForeColor = ColorTranslator.FromHtml("#8892A0"),
            Location = new Point(24, top),
            AutoSize = true
        });
        var box = new TextBox
        {
            Location = new Point(24, top + 20),
            Size = new Size(422, 26),
            BackColor = ColorTranslator.FromHtml("#1A212B"),
            ForeColor = ColorTranslator.FromHtml("#E7EBF0"),
            BorderStyle = BorderStyle.FixedSingle,
            Text = initialValue ?? string.Empty
        };
        Controls.Add(box);
        return box;
    }
}
