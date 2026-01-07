namespace Smena.Client.Components
{
    partial class ExpenseUserControl
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
            buttonSendExpenses = new MaterialSkin.Controls.MaterialButton();
            comboBoxPhotoSendExpenses = new MaterialSkin.Controls.MaterialComboBox();
            checkBoxFromSafeExpenses = new MaterialSkin.Controls.MaterialCheckbox();
            checkBoxPhotoSendExpenses = new MaterialSkin.Controls.MaterialCheckbox();
            textBoxCommentExpenses = new MaterialSkin.Controls.MaterialTextBox();
            textBoxAmountExpenses = new MaterialSkin.Controls.MaterialTextBox();
            SuspendLayout();
            // 
            // buttonSendExpenses
            // 
            buttonSendExpenses.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendExpenses.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonSendExpenses.Depth = 0;
            buttonSendExpenses.HighEmphasis = true;
            buttonSendExpenses.Icon = null;
            buttonSendExpenses.Location = new Point(17, 498);
            buttonSendExpenses.Margin = new Padding(4, 6, 4, 6);
            buttonSendExpenses.MouseState = MaterialSkin.MouseState.HOVER;
            buttonSendExpenses.Name = "buttonSendExpenses";
            buttonSendExpenses.NoAccentTextColor = Color.Empty;
            buttonSendExpenses.Size = new Size(108, 36);
            buttonSendExpenses.TabIndex = 85;
            buttonSendExpenses.Text = "Отправить";
            buttonSendExpenses.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonSendExpenses.UseAccentColor = false;
            buttonSendExpenses.UseVisualStyleBackColor = true;
            // 
            // comboBoxPhotoSendExpenses
            // 
            comboBoxPhotoSendExpenses.AutoResize = false;
            comboBoxPhotoSendExpenses.BackColor = Color.White;
            comboBoxPhotoSendExpenses.Depth = 0;
            comboBoxPhotoSendExpenses.DrawMode = DrawMode.OwnerDrawVariable;
            comboBoxPhotoSendExpenses.DropDownHeight = 174;
            comboBoxPhotoSendExpenses.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPhotoSendExpenses.DropDownWidth = 121;
            comboBoxPhotoSendExpenses.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboBoxPhotoSendExpenses.ForeColor = Color.FromArgb(222, 0, 0, 0);
            comboBoxPhotoSendExpenses.FormattingEnabled = true;
            comboBoxPhotoSendExpenses.Hint = "Имя";
            comboBoxPhotoSendExpenses.IntegralHeight = false;
            comboBoxPhotoSendExpenses.ItemHeight = 43;
            comboBoxPhotoSendExpenses.Location = new Point(17, 184);
            comboBoxPhotoSendExpenses.MaxDropDownItems = 4;
            comboBoxPhotoSendExpenses.MouseState = MaterialSkin.MouseState.OUT;
            comboBoxPhotoSendExpenses.Name = "comboBoxPhotoSendExpenses";
            comboBoxPhotoSendExpenses.Size = new Size(264, 49);
            comboBoxPhotoSendExpenses.StartIndex = 0;
            comboBoxPhotoSendExpenses.TabIndex = 84;
            comboBoxPhotoSendExpenses.Visible = false;
            // 
            // checkBoxFromSafeExpenses
            // 
            checkBoxFromSafeExpenses.AutoSize = true;
            checkBoxFromSafeExpenses.Checked = true;
            checkBoxFromSafeExpenses.CheckState = CheckState.Checked;
            checkBoxFromSafeExpenses.Depth = 0;
            checkBoxFromSafeExpenses.Location = new Point(17, 292);
            checkBoxFromSafeExpenses.Margin = new Padding(0);
            checkBoxFromSafeExpenses.MouseLocation = new Point(-1, -1);
            checkBoxFromSafeExpenses.MouseState = MaterialSkin.MouseState.HOVER;
            checkBoxFromSafeExpenses.Name = "checkBoxFromSafeExpenses";
            checkBoxFromSafeExpenses.ReadOnly = false;
            checkBoxFromSafeExpenses.Ripple = true;
            checkBoxFromSafeExpenses.Size = new Size(104, 37);
            checkBoxFromSafeExpenses.TabIndex = 83;
            checkBoxFromSafeExpenses.Text = "Из сейфа";
            checkBoxFromSafeExpenses.UseVisualStyleBackColor = true;
            // 
            // checkBoxPhotoSendExpenses
            // 
            checkBoxPhotoSendExpenses.AutoSize = true;
            checkBoxPhotoSendExpenses.Depth = 0;
            checkBoxPhotoSendExpenses.Location = new Point(17, 144);
            checkBoxPhotoSendExpenses.Margin = new Padding(0);
            checkBoxPhotoSendExpenses.MouseLocation = new Point(-1, -1);
            checkBoxPhotoSendExpenses.MouseState = MaterialSkin.MouseState.HOVER;
            checkBoxPhotoSendExpenses.Name = "checkBoxPhotoSendExpenses";
            checkBoxPhotoSendExpenses.ReadOnly = false;
            checkBoxPhotoSendExpenses.Ripple = true;
            checkBoxPhotoSendExpenses.Size = new Size(150, 37);
            checkBoxPhotoSendExpenses.TabIndex = 82;
            checkBoxPhotoSendExpenses.Text = "Отправка фото";
            checkBoxPhotoSendExpenses.UseVisualStyleBackColor = true;
            // 
            // textBoxCommentExpenses
            // 
            textBoxCommentExpenses.AnimateReadOnly = false;
            textBoxCommentExpenses.BorderStyle = BorderStyle.None;
            textBoxCommentExpenses.Depth = 0;
            textBoxCommentExpenses.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxCommentExpenses.Hint = "Комментарий";
            textBoxCommentExpenses.LeadingIcon = null;
            textBoxCommentExpenses.Location = new Point(449, 22);
            textBoxCommentExpenses.MaxLength = 50;
            textBoxCommentExpenses.MouseState = MaterialSkin.MouseState.OUT;
            textBoxCommentExpenses.Multiline = false;
            textBoxCommentExpenses.Name = "textBoxCommentExpenses";
            textBoxCommentExpenses.Size = new Size(673, 50);
            textBoxCommentExpenses.TabIndex = 81;
            textBoxCommentExpenses.Text = "";
            textBoxCommentExpenses.TrailingIcon = null;
            // 
            // textBoxAmountExpenses
            // 
            textBoxAmountExpenses.AnimateReadOnly = false;
            textBoxAmountExpenses.BorderStyle = BorderStyle.None;
            textBoxAmountExpenses.Depth = 0;
            textBoxAmountExpenses.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxAmountExpenses.Hint = "Сумма";
            textBoxAmountExpenses.LeadingIcon = null;
            textBoxAmountExpenses.Location = new Point(17, 23);
            textBoxAmountExpenses.MaxLength = 50;
            textBoxAmountExpenses.MouseState = MaterialSkin.MouseState.OUT;
            textBoxAmountExpenses.Multiline = false;
            textBoxAmountExpenses.Name = "textBoxAmountExpenses";
            textBoxAmountExpenses.Size = new Size(312, 50);
            textBoxAmountExpenses.TabIndex = 80;
            textBoxAmountExpenses.Text = "";
            textBoxAmountExpenses.TrailingIcon = null;
            // 
            // ExpenseUserControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(buttonSendExpenses);
            Controls.Add(comboBoxPhotoSendExpenses);
            Controls.Add(checkBoxFromSafeExpenses);
            Controls.Add(checkBoxPhotoSendExpenses);
            Controls.Add(textBoxCommentExpenses);
            Controls.Add(textBoxAmountExpenses);
            Name = "ExpenseUserControl";
            Size = new Size(1147, 629);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialButton buttonSendExpenses;
        private MaterialSkin.Controls.MaterialComboBox comboBoxPhotoSendExpenses;
        private MaterialSkin.Controls.MaterialCheckbox checkBoxFromSafeExpenses;
        private MaterialSkin.Controls.MaterialCheckbox checkBoxPhotoSendExpenses;
        private MaterialSkin.Controls.MaterialTextBox textBoxCommentExpenses;
        private MaterialSkin.Controls.MaterialTextBox textBoxAmountExpenses;
    }
}
