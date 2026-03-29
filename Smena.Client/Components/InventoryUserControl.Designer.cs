namespace Smena.Client.Components
{
    partial class InventoryUserControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        // Dispose is overridden in InventoryUserControl.cs

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            buttonSendInventory = new MaterialSkin.Controls.MaterialButton();
            textBoxAmountInventory = new MaterialSkin.Controls.MaterialTextBox();
            listBoxNameInventory = new ListBox();
            SuspendLayout();
            // 
            // buttonSendInventory
            // 
            buttonSendInventory.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendInventory.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonSendInventory.Depth = 0;
            buttonSendInventory.HighEmphasis = true;
            buttonSendInventory.Icon = null;
            buttonSendInventory.Location = new Point(22, 496);
            buttonSendInventory.Margin = new Padding(4, 6, 4, 6);
            buttonSendInventory.MouseState = MaterialSkin.MouseState.HOVER;
            buttonSendInventory.Name = "buttonSendInventory";
            buttonSendInventory.NoAccentTextColor = Color.Empty;
            buttonSendInventory.Size = new Size(170, 36);
            buttonSendInventory.TabIndex = 88;
            buttonSendInventory.Text = "Отправить инвент";
            buttonSendInventory.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonSendInventory.UseAccentColor = true;
            buttonSendInventory.UseVisualStyleBackColor = true;
            // 
            // textBoxAmountInventory
            // 
            textBoxAmountInventory.AnimateReadOnly = false;
            textBoxAmountInventory.BorderStyle = BorderStyle.None;
            textBoxAmountInventory.Depth = 0;
            textBoxAmountInventory.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxAmountInventory.Hint = "Сумма инвента";
            textBoxAmountInventory.LeadingIcon = null;
            textBoxAmountInventory.Location = new Point(22, 14);
            textBoxAmountInventory.MaxLength = 50;
            textBoxAmountInventory.MouseState = MaterialSkin.MouseState.OUT;
            textBoxAmountInventory.Multiline = false;
            textBoxAmountInventory.Name = "textBoxAmountInventory";
            textBoxAmountInventory.Size = new Size(309, 50);
            textBoxAmountInventory.TabIndex = 87;
            textBoxAmountInventory.Text = "";
            textBoxAmountInventory.TrailingIcon = null;
            // 
            // listBoxNameInventory
            // 
            listBoxNameInventory.BackColor = Color.FromArgb(35, 25, 75);
            listBoxNameInventory.BorderStyle = BorderStyle.FixedSingle;
            listBoxNameInventory.Font = new Font("Segoe UI", 17.25F);
            listBoxNameInventory.ForeColor = Color.White;
            listBoxNameInventory.FormattingEnabled = true;
            listBoxNameInventory.HorizontalScrollbar = true;
            listBoxNameInventory.Location = new Point(22, 70);
            listBoxNameInventory.Name = "listBoxNameInventory";
            listBoxNameInventory.SelectionMode = SelectionMode.MultiExtended;
            listBoxNameInventory.Size = new Size(309, 374);
            listBoxNameInventory.TabIndex = 86;
            // 
            // InventoryUserControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(buttonSendInventory);
            Controls.Add(textBoxAmountInventory);
            Controls.Add(listBoxNameInventory);
            Name = "InventoryUserControl";
            Size = new Size(367, 570);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialButton buttonSendInventory;
        private MaterialSkin.Controls.MaterialTextBox textBoxAmountInventory;
        private ListBox listBoxNameInventory;
    }
}
