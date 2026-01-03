using MaterialSkin.Controls;

namespace Smena.Client.Components
{
    partial class EmployeeHoursBlock
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            comboBoxEmployee = new MaterialComboBox();
            textBoxHours = new MaterialTextBox();
            removeButton = new Button();
            SuspendLayout();
            // 
            // comboBoxEmployee
            // 
            comboBoxEmployee.AutoResize = false;
            comboBoxEmployee.BackColor = Color.Gainsboro;
            comboBoxEmployee.Depth = 0;
            comboBoxEmployee.DrawMode = DrawMode.OwnerDrawVariable;
            comboBoxEmployee.DropDownHeight = 174;
            comboBoxEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxEmployee.DropDownWidth = 121;
            comboBoxEmployee.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboBoxEmployee.ForeColor = Color.White;
            comboBoxEmployee.FormattingEnabled = true;
            comboBoxEmployee.Hint = "Имя";
            comboBoxEmployee.IntegralHeight = false;
            comboBoxEmployee.ItemHeight = 43;
            comboBoxEmployee.Location = new Point(3, 4);
            comboBoxEmployee.MaxDropDownItems = 4;
            comboBoxEmployee.MouseState = MaterialSkin.MouseState.OUT;
            comboBoxEmployee.Name = "comboBoxEmployee";
            comboBoxEmployee.Size = new Size(241, 49);
            comboBoxEmployee.StartIndex = 0;
            comboBoxEmployee.TabIndex = 0;
            // 
            // textBoxHours
            // 
            textBoxHours.AnimateReadOnly = false;
            textBoxHours.BorderStyle = BorderStyle.None;
            textBoxHours.Depth = 0;
            textBoxHours.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxHours.Hint = "Часы";
            textBoxHours.LeadingIcon = null;
            textBoxHours.Location = new Point(250, 4);
            textBoxHours.MaxLength = 50;
            textBoxHours.MouseState = MaterialSkin.MouseState.OUT;
            textBoxHours.Multiline = false;
            textBoxHours.Name = "textBoxHours";
            textBoxHours.Size = new Size(100, 50);
            textBoxHours.TabIndex = 1;
            textBoxHours.Text = "";
            textBoxHours.TrailingIcon = null;
            // 
            // removeButton
            // 
            removeButton.Location = new Point(3, 58);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(241, 23);
            removeButton.TabIndex = 2;
            removeButton.Text = "удалить строку";
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += ButtonRemove_Click;
            // 
            // EmployeeHoursBlock
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(removeButton);
            Controls.Add(textBoxHours);
            Controls.Add(comboBoxEmployee);
            Name = "EmployeeHoursBlock";
            Size = new Size(357, 84);
            ResumeLayout(false);
        }

        #endregion

        private MaterialComboBox comboBoxEmployee;
        private MaterialTextBox textBoxHours;
        private Button removeButton;
    }
}
