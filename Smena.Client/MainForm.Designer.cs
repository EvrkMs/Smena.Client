using MaterialSkin.Controls;

namespace Smena.Client
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainTabSelector = new MaterialTabSelector();
            mainTabControl = new MaterialTabControl();
            raportPage = new TabPage();
            addEmployeeHoursBlockButton = new Button();
            employeePage = new TabPage();
            employeeListView = new ListView();
            employeeAddButton = new MaterialButton();
            mainTabControl.SuspendLayout();
            raportPage.SuspendLayout();
            employeePage.SuspendLayout();
            SuspendLayout();
            // 
            // mainTabSelector
            // 
            mainTabSelector.BaseTabControl = mainTabControl;
            mainTabSelector.CharacterCasing = MaterialTabSelector.CustomCharacterCasing.Normal;
            mainTabSelector.Depth = 0;
            mainTabSelector.Dock = DockStyle.Bottom;
            mainTabSelector.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            mainTabSelector.Location = new Point(3, 630);
            mainTabSelector.MouseState = MaterialSkin.MouseState.HOVER;
            mainTabSelector.Name = "mainTabSelector";
            mainTabSelector.Size = new Size(1258, 48);
            mainTabSelector.TabIndex = 0;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(raportPage);
            mainTabControl.Controls.Add(employeePage);
            mainTabControl.Depth = 0;
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Location = new Point(3, 64);
            mainTabControl.MouseState = MaterialSkin.MouseState.HOVER;
            mainTabControl.Multiline = true;
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(1258, 566);
            mainTabControl.TabIndex = 1;
            // 
            // raportPage
            // 
            raportPage.BackColor = Color.Fuchsia;
            raportPage.Controls.Add(addEmployeeHoursBlockButton);
            raportPage.Location = new Point(4, 24);
            raportPage.Name = "raportPage";
            raportPage.Size = new Size(1250, 538);
            raportPage.TabIndex = 0;
            raportPage.Text = "Закрытие смены";
            // 
            // addEmployeeHoursBlockButton
            // 
            addEmployeeHoursBlockButton.Location = new Point(3, 3);
            addEmployeeHoursBlockButton.Name = "addEmployeeHoursBlockButton";
            addEmployeeHoursBlockButton.Size = new Size(241, 23);
            addEmployeeHoursBlockButton.TabIndex = 0;
            addEmployeeHoursBlockButton.Text = "добавить строку сотрудника";
            addEmployeeHoursBlockButton.UseVisualStyleBackColor = true;
            addEmployeeHoursBlockButton.Click += AddEmployeeAndHours_Click;
            // 
            // employeePage
            // 
            employeePage.Controls.Add(employeeListView);
            employeePage.Controls.Add(employeeAddButton);
            employeePage.Location = new Point(4, 24);
            employeePage.Name = "employeePage";
            employeePage.Size = new Size(1250, 538);
            employeePage.TabIndex = 1;
            employeePage.Text = "Работа с сотрудниками";
            employeePage.UseVisualStyleBackColor = true;
            // 
            // employeeListView
            // 
            employeeListView.BackColor = Color.Indigo;
            employeeListView.Dock = DockStyle.Top;
            employeeListView.ForeColor = SystemColors.MenuBar;
            employeeListView.Location = new Point(0, 0);
            employeeListView.Name = "employeeListView";
            employeeListView.Size = new Size(1250, 493);
            employeeListView.TabIndex = 2;
            employeeListView.UseCompatibleStateImageBehavior = false;
            // 
            // employeeAddButton
            // 
            employeeAddButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            employeeAddButton.Density = MaterialButton.MaterialButtonDensity.Default;
            employeeAddButton.Depth = 0;
            employeeAddButton.Dock = DockStyle.Bottom;
            employeeAddButton.HighEmphasis = true;
            employeeAddButton.Icon = null;
            employeeAddButton.Location = new Point(0, 502);
            employeeAddButton.Margin = new Padding(4, 6, 4, 6);
            employeeAddButton.MouseState = MaterialSkin.MouseState.HOVER;
            employeeAddButton.Name = "employeeAddButton";
            employeeAddButton.NoAccentTextColor = Color.Empty;
            employeeAddButton.Size = new Size(1250, 36);
            employeeAddButton.TabIndex = 1;
            employeeAddButton.Text = "Добавить";
            employeeAddButton.Type = MaterialButton.MaterialButtonType.Contained;
            employeeAddButton.UseAccentColor = false;
            employeeAddButton.UseVisualStyleBackColor = true;
            employeeAddButton.Click += EmployeeAddButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 681);
            Controls.Add(mainTabControl);
            Controls.Add(mainTabSelector);
            Name = "MainForm";
            mainTabControl.ResumeLayout(false);
            raportPage.ResumeLayout(false);
            employeePage.ResumeLayout(false);
            employeePage.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialTabSelector mainTabSelector;
        private MaterialTabControl mainTabControl;
        private TabPage raportPage;
        private TabPage employeePage;
        private MaterialButton employeeAddButton;
        private ListView employeeListView;
        private Button addEmployeeHoursBlockButton;
    }
}
