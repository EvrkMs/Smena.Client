using Grpc.Core;
using Grpc.Net.Client;
using Host.Grpc.Services;

namespace Smena.Client;

public partial class MainForm : Form
{
    private readonly GrpcChannel _channel;

    public MainForm()
    {
        InitializeComponent();

        // Создаём канал один раз
        _channel = new GrpcService("http://localhost:5000").Channel;

        // Загружаем сотрудников при старте формы
        _ = LoadEmployeesAsync();
    }

    private async Task LoadEmployeesAsync()
    {
        var client = new EmployeeService.EmployeeServiceClient(_channel);

        // Выбираем streaming метод
        var call = client.EmployeesListStream(new GetEmployeesRequest());

        // Читаем стрим асинхронно
        await foreach (var emp in call.ResponseStream.ReadAllAsync())
        {
            // Обновление UI через Invoke
            EmployeeListBox.Invoke(() =>
            {
                EmployeeListBox.Items.Add($"{emp.Id} - {emp.Name}");
            });
        }
    }

    private async void button1_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textBox1.Text))
        {
            MessageBox.Show("Вы забыли ввести имя");
            return;
        }

        var req = new GrpcEmployee
        {
            Id = Guid.NewGuid().ToString(), // генерируем уникальный Guid
            Name = textBox1.Text
        };

        var client = new EmployeeService.EmployeeServiceClient(_channel);

        // Вызов EmployeeAdd
        var response = await client.EmployeeAddAsync(req);

        if (response != null)
        {
            MessageBox.Show(response.Value ? "Сотрудник добавлен" : "Ошибка добавления");
        }
    }
}
