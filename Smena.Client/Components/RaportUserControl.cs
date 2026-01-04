using Host.Grpc.Services.Employee;
using MaterialSkin.Controls;
using Smena.Client.Services;

namespace Smena.Client.Components
{
    public partial class RaportUserControl : UserControl
    {
        private EmployeeService employeeService;
        private SafeService safeService;
        private readonly List<ComboBox> boxesName;
        public RaportUserControl()
        {

            InitializeComponent();
            boxesName =
                [
                    comboBoxFirstNameRaport,
                    comboBoxSecondNameRaport,
                    comboBoxThirdNameRaport,
                ];
        }

        public void Initialize(EmployeeService employeeService, SafeService safeService)
        {
            this.employeeService = employeeService;
            this.safeService = safeService;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            employeeService.Employees.ListChanged += Event_ListEmployeeChange;

            foreach (var comboBox in boxesName)
            {
                comboBox.SelectedIndexChanged += OnComboBoxSelectedIndexChanged;
                comboBox.SelectedIndexChanged += ForceComboRedraw;
            }
        }
        private void ForceComboRedraw(object? sender, EventArgs e)
        {
            if (sender is Control c)
            {
                c.Invalidate();
                c.Refresh();   // на всякий случай
            }
        }
        public void UnsubscribeFromEvents()
        {
            employeeService.Employees.ListChanged -= Event_ListEmployeeChange;

            foreach (var comboBox in boxesName)
            {
                comboBox.SelectedIndexChanged -= OnComboBoxSelectedIndexChanged;
            }
        }

        private async void Event_ListEmployeeChange(object? sender, EventArgs e)
        {
            var baseEmployeeList = employeeService.Employees.ToList();
            foreach(var comboBox in boxesName)
            {
                comboBox.DataSource = null;

                comboBox.DisplayMember = nameof(GrpcEmployee.Name);
                comboBox.ValueMember = nameof(GrpcEmployee.Id);

                var list = new List<GrpcEmployee>() { MainForm.NullComboBox };
                list.AddRange(baseEmployeeList);

                comboBox.DataSource = list;
            }
        }

        private void OnComboBoxSelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is not MaterialComboBox comboBox)
                return;

            bool hasValidSelection = comboBox.SelectedItem != MainForm.NullComboBox &&
                                   comboBox.SelectedItem != null;

            switch (comboBox)
            {
                case var _ when comboBox == comboBoxFirstNameRaport:
                    UpdateSecondNameVisibility(hasValidSelection);
                    break;

                case var _ when comboBox == comboBoxSecondNameRaport:
                    UpdateThirdNameAndHoursVisibility(hasValidSelection);
                    break;

                case var _ when comboBox == comboBoxThirdNameRaport:
                    UpdateThirdNameHoursVisibility(hasValidSelection);
                    break;
            }
        }

        private void UpdateSecondNameVisibility(bool isVisible)
        {
            comboBoxSecondNameRaport.Visible = isVisible;
        }

        private void UpdateThirdNameAndHoursVisibility(bool isVisible)
        {
            textBoxMinusFirstNameRaport.Visible = isVisible;
            textBoxHoursSecondNameRaport.Visible = isVisible;
            textBoxMinusSecondNameRaport.Visible = isVisible;
            comboBoxThirdNameRaport.Visible = isVisible;
        }

        private void UpdateThirdNameHoursVisibility(bool isVisible)
        {
            textBoxHoursThirdNameRaport.Visible = isVisible;
            textBoxMinusThirdNameRaport.Visible = isVisible;
        }
    }
}
