namespace Smena.Client.Components
{
    partial class ComingUserControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            buttonSendPlusSafe = new MaterialSkin.Controls.MaterialButton();
            textBoxCommentPlusAmount = new MaterialSkin.Controls.MaterialTextBox();
            textBoxAmountPlusSafe = new MaterialSkin.Controls.MaterialTextBox();
            SuspendLayout();
            // 
            // buttonSendPlusSafe
            // 
            buttonSendPlusSafe.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendPlusSafe.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonSendPlusSafe.Depth = 0;
            buttonSendPlusSafe.HighEmphasis = true;
            buttonSendPlusSafe.Icon = null;
            buttonSendPlusSafe.Location = new Point(38, 458);
            buttonSendPlusSafe.Margin = new Padding(4, 6, 4, 6);
            buttonSendPlusSafe.MouseState = MaterialSkin.MouseState.HOVER;
            buttonSendPlusSafe.Name = "buttonSendPlusSafe";
            buttonSendPlusSafe.NoAccentTextColor = Color.Empty;
            buttonSendPlusSafe.Size = new Size(108, 36);
            buttonSendPlusSafe.TabIndex = 5;
            buttonSendPlusSafe.Text = "Отправить";
            buttonSendPlusSafe.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonSendPlusSafe.UseAccentColor = true;
            buttonSendPlusSafe.UseVisualStyleBackColor = true;
            // 
            // textBoxCommentPlusAmount
            // 
            textBoxCommentPlusAmount.AnimateReadOnly = false;
            textBoxCommentPlusAmount.BorderStyle = BorderStyle.None;
            textBoxCommentPlusAmount.Depth = 0;
            textBoxCommentPlusAmount.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxCommentPlusAmount.Hint = "Комментарий";
            textBoxCommentPlusAmount.LeadingIcon = null;
            textBoxCommentPlusAmount.Location = new Point(342, 22);
            textBoxCommentPlusAmount.MaxLength = 50;
            textBoxCommentPlusAmount.MouseState = MaterialSkin.MouseState.OUT;
            textBoxCommentPlusAmount.Multiline = false;
            textBoxCommentPlusAmount.Name = "textBoxCommentPlusAmount";
            textBoxCommentPlusAmount.Size = new Size(543, 50);
            textBoxCommentPlusAmount.TabIndex = 4;
            textBoxCommentPlusAmount.Text = "";
            textBoxCommentPlusAmount.TrailingIcon = null;
            // 
            // textBoxAmountPlusSafe
            // 
            textBoxAmountPlusSafe.AnimateReadOnly = false;
            textBoxAmountPlusSafe.BorderStyle = BorderStyle.None;
            textBoxAmountPlusSafe.Depth = 0;
            textBoxAmountPlusSafe.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxAmountPlusSafe.Hint = "Сумма";
            textBoxAmountPlusSafe.LeadingIcon = null;
            textBoxAmountPlusSafe.Location = new Point(20, 22);
            textBoxAmountPlusSafe.MaxLength = 50;
            textBoxAmountPlusSafe.MouseState = MaterialSkin.MouseState.OUT;
            textBoxAmountPlusSafe.Multiline = false;
            textBoxAmountPlusSafe.Name = "textBoxAmountPlusSafe";
            textBoxAmountPlusSafe.Size = new Size(289, 50);
            textBoxAmountPlusSafe.TabIndex = 3;
            textBoxAmountPlusSafe.Text = "";
            textBoxAmountPlusSafe.TrailingIcon = null;
            // 
            // ComingUserControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(buttonSendPlusSafe);
            Controls.Add(textBoxCommentPlusAmount);
            Controls.Add(textBoxAmountPlusSafe);
            Name = "ComingUserControl";
            Size = new Size(898, 515);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialButton buttonSendPlusSafe;
        private MaterialSkin.Controls.MaterialTextBox textBoxCommentPlusAmount;
        private MaterialSkin.Controls.MaterialTextBox textBoxAmountPlusSafe;
    }
}
