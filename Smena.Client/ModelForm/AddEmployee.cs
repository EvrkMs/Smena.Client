using MaterialSkin.Controls;
using Smena.Client.Services;

namespace Smena.Client.ModelForm
{
    public partial class AddEmployee : MaterialForm
    {
        private readonly EmployeeService Service;
        public bool? Success { get; private set; } = null;
        public AddEmployee(EmployeeService service)
        {
            InitializeComponent();
            Service = service;
        }

        private async void AcceptButton_Click(object sender, EventArgs e)
        {
            var name = nameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Вы не ввели имя");
                return;
            }

            Success = await Service.AddEmployeeAsync(new Host.Grpc.Services.Employee.GrpcEmployee { Name = name });

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Success = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
