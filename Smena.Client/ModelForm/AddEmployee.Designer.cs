namespace Smena.Client.ModelForm
{
    partial class AddEmployee
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            acceptButton = new MaterialSkin.Controls.MaterialButton();
            cancelButton = new MaterialSkin.Controls.MaterialButton();
            nameTextBox = new MaterialSkin.Controls.MaterialTextBox();
            SuspendLayout();
            // 
            // acceptButton
            // 
            acceptButton.AutoSize = false;
            acceptButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            acceptButton.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            acceptButton.Depth = 0;
            acceptButton.HighEmphasis = true;
            acceptButton.Icon = null;
            acceptButton.Location = new Point(23, 142);
            acceptButton.Margin = new Padding(4, 6, 4, 6);
            acceptButton.MouseState = MaterialSkin.MouseState.HOVER;
            acceptButton.Name = "acceptButton";
            acceptButton.NoAccentTextColor = Color.Empty;
            acceptButton.Size = new Size(268, 37);
            acceptButton.TabIndex = 1;
            acceptButton.Text = "Добавить";
            acceptButton.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            acceptButton.UseAccentColor = false;
            acceptButton.UseVisualStyleBackColor = true;
            acceptButton.Click += AcceptButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.AutoSize = false;
            cancelButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cancelButton.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            cancelButton.Depth = 0;
            cancelButton.HighEmphasis = true;
            cancelButton.Icon = null;
            cancelButton.Location = new Point(23, 191);
            cancelButton.Margin = new Padding(4, 6, 4, 6);
            cancelButton.MouseState = MaterialSkin.MouseState.HOVER;
            cancelButton.Name = "cancelButton";
            cancelButton.NoAccentTextColor = Color.Empty;
            cancelButton.Size = new Size(268, 37);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Отмена";
            cancelButton.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            cancelButton.UseAccentColor = false;
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // nameTextBox
            // 
            nameTextBox.AnimateReadOnly = false;
            nameTextBox.BorderStyle = BorderStyle.None;
            nameTextBox.Depth = 0;
            nameTextBox.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            nameTextBox.LeadingIcon = null;
            nameTextBox.Location = new Point(23, 83);
            nameTextBox.MaxLength = 50;
            nameTextBox.MouseState = MaterialSkin.MouseState.OUT;
            nameTextBox.Multiline = false;
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(268, 50);
            nameTextBox.TabIndex = 3;
            nameTextBox.Text = "";
            nameTextBox.TrailingIcon = null;
            // 
            // AddEmployee
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(311, 266);
            Controls.Add(nameTextBox);
            Controls.Add(cancelButton);
            Controls.Add(acceptButton);
            Name = "AddEmployee";
            Text = "AddEmployee";
            ResumeLayout(false);
        }

        #endregion
        private MaterialSkin.Controls.MaterialButton acceptButton;
        private MaterialSkin.Controls.MaterialButton cancelButton;
        private MaterialSkin.Controls.MaterialTextBox nameTextBox;
    }
}