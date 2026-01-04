using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace Smena.Client
{
    partial class MainForm : MaterialForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// true if managed resources should be disposed; otherwise, false.
        /// </param>
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
            materialTabSelector1 = new MaterialTabSelector();
            materialTabControl1 = new MaterialTabControl();
            tabRaport = new TabPage();
            raportUserControl = new Smena.Client.Components.RaportUserControl();
            tabAdvance = new TabPage();
            checkBoxExtractSalaryFromSafe = new MaterialCheckbox();
            comboBoxExtractSalaryName = new MaterialComboBox();
            buttonSendExtractSalary = new MaterialButton();
            checkBoxSalaryAdvance = new MaterialCheckbox();
            checkBoxAdvanceExtract = new MaterialCheckbox();
            textBoxSalaryExtractAmount = new MaterialTextBox();
            tabExpenses = new TabPage();
            buttonSendExpenses = new MaterialButton();
            comboBoxPhotoSendExpenses = new MaterialComboBox();
            checkBoxFromSafeExpenses = new MaterialCheckbox();
            checkBoxPhotoSendExpenses = new MaterialCheckbox();
            textBoxCommentExpenses = new MaterialTextBox();
            textBoxAmountExpenses = new MaterialTextBox();
            tabPlusSafe = new TabPage();
            buttonSendPlusSafe = new MaterialButton();
            textBoxCommentPlusAmount = new MaterialTextBox();
            textBoxAmountPlusSafe = new MaterialTextBox();
            tabInventory = new TabPage();
            buttonSendInventory = new MaterialButton();
            textBoxAmountInventory = new MaterialTextBox();
            listBoxNameInventory = new ListBox();
            materialTabControl1.SuspendLayout();
            tabRaport.SuspendLayout();
            tabAdvance.SuspendLayout();
            tabExpenses.SuspendLayout();
            tabPlusSafe.SuspendLayout();
            tabInventory.SuspendLayout();
            SuspendLayout();
            // 
            // materialTabSelector1
            // 
            materialTabSelector1.BaseTabControl = materialTabControl1;
            materialTabSelector1.CharacterCasing = MaterialTabSelector.CustomCharacterCasing.Normal;
            materialTabSelector1.Depth = 0;
            materialTabSelector1.Dock = DockStyle.Bottom;
            materialTabSelector1.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            materialTabSelector1.Location = new Point(3, 808);
            materialTabSelector1.MouseState = MouseState.HOVER;
            materialTabSelector1.Name = "materialTabSelector1";
            materialTabSelector1.Size = new Size(1290, 48);
            materialTabSelector1.TabIndex = 3;
            materialTabSelector1.Text = "materialTabSelector1";
            // 
            // materialTabControl1
            // 
            materialTabControl1.Controls.Add(tabRaport);
            materialTabControl1.Controls.Add(tabAdvance);
            materialTabControl1.Controls.Add(tabExpenses);
            materialTabControl1.Controls.Add(tabPlusSafe);
            materialTabControl1.Controls.Add(tabInventory);
            materialTabControl1.Depth = 0;
            materialTabControl1.Dock = DockStyle.Fill;
            materialTabControl1.Location = new Point(3, 64);
            materialTabControl1.MouseState = MouseState.HOVER;
            materialTabControl1.Multiline = true;
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new Size(1290, 744);
            materialTabControl1.TabIndex = 2;
            // 
            // tabRaport
            // 
            tabRaport.BackColor = Color.FromArgb(64, 0, 64);
            tabRaport.Controls.Add(raportUserControl);
            tabRaport.Font = new Font("Segoe UI", 11F);
            tabRaport.Location = new Point(4, 24);
            tabRaport.Name = "tabRaport";
            tabRaport.Padding = new Padding(3);
            tabRaport.Size = new Size(1282, 716);
            tabRaport.TabIndex = 0;
            tabRaport.Text = "Отчёт за смену";
            // 
            // raportUserControl
            // 
            raportUserControl.Dock = DockStyle.Fill;
            raportUserControl.Location = new Point(3, 3);
            raportUserControl.Name = "raportUserControl";
            raportUserControl.Size = new Size(1276, 710);
            raportUserControl.TabIndex = 0;
            // 
            // tabAdvance
            // 
            tabAdvance.BackColor = Color.FromArgb(64, 0, 64);
            tabAdvance.Controls.Add(checkBoxExtractSalaryFromSafe);
            tabAdvance.Controls.Add(comboBoxExtractSalaryName);
            tabAdvance.Controls.Add(buttonSendExtractSalary);
            tabAdvance.Controls.Add(checkBoxSalaryAdvance);
            tabAdvance.Controls.Add(checkBoxAdvanceExtract);
            tabAdvance.Controls.Add(textBoxSalaryExtractAmount);
            tabAdvance.Location = new Point(4, 24);
            tabAdvance.Name = "tabAdvance";
            tabAdvance.Size = new Size(1282, 716);
            tabAdvance.TabIndex = 1;
            tabAdvance.Text = "Аванс";
            // 
            // checkBoxExtractSalaryFromSafe
            // 
            checkBoxExtractSalaryFromSafe.AutoSize = true;
            checkBoxExtractSalaryFromSafe.Depth = 0;
            checkBoxExtractSalaryFromSafe.Location = new Point(34, 107);
            checkBoxExtractSalaryFromSafe.Margin = new Padding(0);
            checkBoxExtractSalaryFromSafe.MouseLocation = new Point(-1, -1);
            checkBoxExtractSalaryFromSafe.MouseState = MouseState.HOVER;
            checkBoxExtractSalaryFromSafe.Name = "checkBoxExtractSalaryFromSafe";
            checkBoxExtractSalaryFromSafe.ReadOnly = false;
            checkBoxExtractSalaryFromSafe.Ripple = true;
            checkBoxExtractSalaryFromSafe.Size = new Size(63, 37);
            checkBoxExtractSalaryFromSafe.TabIndex = 6;
            checkBoxExtractSalaryFromSafe.Text = "Б/Н";
            checkBoxExtractSalaryFromSafe.UseVisualStyleBackColor = true;
            // 
            // comboBoxExtractSalaryName
            // 
            comboBoxExtractSalaryName.AutoResize = false;
            comboBoxExtractSalaryName.BackColor = Color.White;
            comboBoxExtractSalaryName.Depth = 0;
            comboBoxExtractSalaryName.DrawMode = DrawMode.OwnerDrawVariable;
            comboBoxExtractSalaryName.DropDownHeight = 174;
            comboBoxExtractSalaryName.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxExtractSalaryName.DropDownWidth = 121;
            comboBoxExtractSalaryName.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboBoxExtractSalaryName.ForeColor = Color.FromArgb(222, 0, 0, 0);
            comboBoxExtractSalaryName.FormattingEnabled = true;
            comboBoxExtractSalaryName.Hint = "Имя";
            comboBoxExtractSalaryName.IntegralHeight = false;
            comboBoxExtractSalaryName.ItemHeight = 43;
            comboBoxExtractSalaryName.Location = new Point(34, 35);
            comboBoxExtractSalaryName.MaxDropDownItems = 4;
            comboBoxExtractSalaryName.MouseState = MouseState.OUT;
            comboBoxExtractSalaryName.Name = "comboBoxExtractSalaryName";
            comboBoxExtractSalaryName.Size = new Size(256, 49);
            comboBoxExtractSalaryName.StartIndex = 0;
            comboBoxExtractSalaryName.TabIndex = 5;
            // 
            // buttonSendExtractSalary
            // 
            buttonSendExtractSalary.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendExtractSalary.Density = MaterialButton.MaterialButtonDensity.Default;
            buttonSendExtractSalary.Depth = 0;
            buttonSendExtractSalary.HighEmphasis = true;
            buttonSendExtractSalary.Icon = null;
            buttonSendExtractSalary.Location = new Point(363, 621);
            buttonSendExtractSalary.Margin = new Padding(4, 6, 4, 6);
            buttonSendExtractSalary.MouseState = MouseState.HOVER;
            buttonSendExtractSalary.Name = "buttonSendExtractSalary";
            buttonSendExtractSalary.NoAccentTextColor = Color.Empty;
            buttonSendExtractSalary.Size = new Size(108, 36);
            buttonSendExtractSalary.TabIndex = 4;
            buttonSendExtractSalary.Text = "Отправить";
            buttonSendExtractSalary.Type = MaterialButton.MaterialButtonType.Contained;
            buttonSendExtractSalary.UseAccentColor = false;
            buttonSendExtractSalary.UseVisualStyleBackColor = true;
            // 
            // checkBoxSalaryAdvance
            // 
            checkBoxSalaryAdvance.AutoSize = true;
            checkBoxSalaryAdvance.Depth = 0;
            checkBoxSalaryAdvance.Location = new Point(465, 107);
            checkBoxSalaryAdvance.Margin = new Padding(0);
            checkBoxSalaryAdvance.MouseLocation = new Point(-1, -1);
            checkBoxSalaryAdvance.MouseState = MouseState.HOVER;
            checkBoxSalaryAdvance.Name = "checkBoxSalaryAdvance";
            checkBoxSalaryAdvance.ReadOnly = false;
            checkBoxSalaryAdvance.Ripple = true;
            checkBoxSalaryAdvance.Size = new Size(56, 37);
            checkBoxSalaryAdvance.TabIndex = 3;
            checkBoxSalaryAdvance.Text = "ЗП";
            checkBoxSalaryAdvance.UseVisualStyleBackColor = true;
            // 
            // checkBoxAdvanceExtract
            // 
            checkBoxAdvanceExtract.AutoSize = true;
            checkBoxAdvanceExtract.Checked = true;
            checkBoxAdvanceExtract.CheckState = CheckState.Checked;
            checkBoxAdvanceExtract.Depth = 0;
            checkBoxAdvanceExtract.ForeColor = SystemColors.ControlText;
            checkBoxAdvanceExtract.Location = new Point(363, 107);
            checkBoxAdvanceExtract.Margin = new Padding(0);
            checkBoxAdvanceExtract.MouseLocation = new Point(-1, -1);
            checkBoxAdvanceExtract.MouseState = MouseState.HOVER;
            checkBoxAdvanceExtract.Name = "checkBoxAdvanceExtract";
            checkBoxAdvanceExtract.ReadOnly = false;
            checkBoxAdvanceExtract.Ripple = true;
            checkBoxAdvanceExtract.Size = new Size(80, 37);
            checkBoxAdvanceExtract.TabIndex = 2;
            checkBoxAdvanceExtract.Text = "Аванс";
            checkBoxAdvanceExtract.UseVisualStyleBackColor = true;
            // 
            // textBoxSalaryExtractAmount
            // 
            textBoxSalaryExtractAmount.AnimateReadOnly = false;
            textBoxSalaryExtractAmount.BackgroundImageLayout = ImageLayout.None;
            textBoxSalaryExtractAmount.BorderStyle = BorderStyle.None;
            textBoxSalaryExtractAmount.Depth = 0;
            textBoxSalaryExtractAmount.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxSalaryExtractAmount.Hint = "Сумма";
            textBoxSalaryExtractAmount.LeadingIcon = null;
            textBoxSalaryExtractAmount.Location = new Point(363, 34);
            textBoxSalaryExtractAmount.MaxLength = 32767;
            textBoxSalaryExtractAmount.MouseState = MouseState.OUT;
            textBoxSalaryExtractAmount.Multiline = false;
            textBoxSalaryExtractAmount.Name = "textBoxSalaryExtractAmount";
            textBoxSalaryExtractAmount.RightToLeft = RightToLeft.No;
            textBoxSalaryExtractAmount.Size = new Size(250, 50);
            textBoxSalaryExtractAmount.TabIndex = 0;
            textBoxSalaryExtractAmount.TabStop = false;
            textBoxSalaryExtractAmount.Text = "";
            textBoxSalaryExtractAmount.TrailingIcon = null;
            // 
            // tabExpenses
            // 
            tabExpenses.BackColor = Color.FromArgb(64, 0, 64);
            tabExpenses.Controls.Add(buttonSendExpenses);
            tabExpenses.Controls.Add(comboBoxPhotoSendExpenses);
            tabExpenses.Controls.Add(checkBoxFromSafeExpenses);
            tabExpenses.Controls.Add(checkBoxPhotoSendExpenses);
            tabExpenses.Controls.Add(textBoxCommentExpenses);
            tabExpenses.Controls.Add(textBoxAmountExpenses);
            tabExpenses.ForeColor = SystemColors.ControlText;
            tabExpenses.Location = new Point(4, 24);
            tabExpenses.Name = "tabExpenses";
            tabExpenses.Size = new Size(1282, 716);
            tabExpenses.TabIndex = 2;
            tabExpenses.Text = "Расходы";
            // 
            // buttonSendExpenses
            // 
            buttonSendExpenses.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendExpenses.Density = MaterialButton.MaterialButtonDensity.Default;
            buttonSendExpenses.Depth = 0;
            buttonSendExpenses.HighEmphasis = true;
            buttonSendExpenses.Icon = null;
            buttonSendExpenses.Location = new Point(35, 680);
            buttonSendExpenses.Margin = new Padding(4, 6, 4, 6);
            buttonSendExpenses.MouseState = MouseState.HOVER;
            buttonSendExpenses.Name = "buttonSendExpenses";
            buttonSendExpenses.NoAccentTextColor = Color.Empty;
            buttonSendExpenses.Size = new Size(108, 36);
            buttonSendExpenses.TabIndex = 79;
            buttonSendExpenses.Text = "Отправить";
            buttonSendExpenses.Type = MaterialButton.MaterialButtonType.Contained;
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
            comboBoxPhotoSendExpenses.Location = new Point(25, 198);
            comboBoxPhotoSendExpenses.MaxDropDownItems = 4;
            comboBoxPhotoSendExpenses.MouseState = MouseState.OUT;
            comboBoxPhotoSendExpenses.Name = "comboBoxPhotoSendExpenses";
            comboBoxPhotoSendExpenses.Size = new Size(264, 49);
            comboBoxPhotoSendExpenses.StartIndex = 0;
            comboBoxPhotoSendExpenses.TabIndex = 5;
            comboBoxPhotoSendExpenses.Visible = false;
            // 
            // checkBoxFromSafeExpenses
            // 
            checkBoxFromSafeExpenses.AutoSize = true;
            checkBoxFromSafeExpenses.Checked = true;
            checkBoxFromSafeExpenses.CheckState = CheckState.Checked;
            checkBoxFromSafeExpenses.Depth = 0;
            checkBoxFromSafeExpenses.Location = new Point(25, 306);
            checkBoxFromSafeExpenses.Margin = new Padding(0);
            checkBoxFromSafeExpenses.MouseLocation = new Point(-1, -1);
            checkBoxFromSafeExpenses.MouseState = MouseState.HOVER;
            checkBoxFromSafeExpenses.Name = "checkBoxFromSafeExpenses";
            checkBoxFromSafeExpenses.ReadOnly = false;
            checkBoxFromSafeExpenses.Ripple = true;
            checkBoxFromSafeExpenses.Size = new Size(104, 37);
            checkBoxFromSafeExpenses.TabIndex = 4;
            checkBoxFromSafeExpenses.Text = "Из сейфа";
            checkBoxFromSafeExpenses.UseVisualStyleBackColor = true;
            // 
            // checkBoxPhotoSendExpenses
            // 
            checkBoxPhotoSendExpenses.AutoSize = true;
            checkBoxPhotoSendExpenses.Depth = 0;
            checkBoxPhotoSendExpenses.Location = new Point(25, 158);
            checkBoxPhotoSendExpenses.Margin = new Padding(0);
            checkBoxPhotoSendExpenses.MouseLocation = new Point(-1, -1);
            checkBoxPhotoSendExpenses.MouseState = MouseState.HOVER;
            checkBoxPhotoSendExpenses.Name = "checkBoxPhotoSendExpenses";
            checkBoxPhotoSendExpenses.ReadOnly = false;
            checkBoxPhotoSendExpenses.Ripple = true;
            checkBoxPhotoSendExpenses.Size = new Size(150, 37);
            checkBoxPhotoSendExpenses.TabIndex = 2;
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
            textBoxCommentExpenses.Location = new Point(457, 36);
            textBoxCommentExpenses.MaxLength = 50;
            textBoxCommentExpenses.MouseState = MouseState.OUT;
            textBoxCommentExpenses.Multiline = false;
            textBoxCommentExpenses.Name = "textBoxCommentExpenses";
            textBoxCommentExpenses.Size = new Size(673, 50);
            textBoxCommentExpenses.TabIndex = 1;
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
            textBoxAmountExpenses.Location = new Point(25, 37);
            textBoxAmountExpenses.MaxLength = 50;
            textBoxAmountExpenses.MouseState = MouseState.OUT;
            textBoxAmountExpenses.Multiline = false;
            textBoxAmountExpenses.Name = "textBoxAmountExpenses";
            textBoxAmountExpenses.Size = new Size(312, 50);
            textBoxAmountExpenses.TabIndex = 0;
            textBoxAmountExpenses.Text = "";
            textBoxAmountExpenses.TrailingIcon = null;
            // 
            // tabPlusSafe
            // 
            tabPlusSafe.BackColor = Color.FromArgb(64, 0, 64);
            tabPlusSafe.Controls.Add(buttonSendPlusSafe);
            tabPlusSafe.Controls.Add(textBoxCommentPlusAmount);
            tabPlusSafe.Controls.Add(textBoxAmountPlusSafe);
            tabPlusSafe.Location = new Point(4, 24);
            tabPlusSafe.Name = "tabPlusSafe";
            tabPlusSafe.Size = new Size(1282, 716);
            tabPlusSafe.TabIndex = 3;
            tabPlusSafe.Text = "Плюс к сейфу";
            // 
            // buttonSendPlusSafe
            // 
            buttonSendPlusSafe.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendPlusSafe.Density = MaterialButton.MaterialButtonDensity.Default;
            buttonSendPlusSafe.Depth = 0;
            buttonSendPlusSafe.HighEmphasis = true;
            buttonSendPlusSafe.Icon = null;
            buttonSendPlusSafe.Location = new Point(61, 662);
            buttonSendPlusSafe.Margin = new Padding(4, 6, 4, 6);
            buttonSendPlusSafe.MouseState = MouseState.HOVER;
            buttonSendPlusSafe.Name = "buttonSendPlusSafe";
            buttonSendPlusSafe.NoAccentTextColor = Color.Empty;
            buttonSendPlusSafe.Size = new Size(108, 36);
            buttonSendPlusSafe.TabIndex = 2;
            buttonSendPlusSafe.Text = "Отправить";
            buttonSendPlusSafe.Type = MaterialButton.MaterialButtonType.Contained;
            buttonSendPlusSafe.UseAccentColor = false;
            buttonSendPlusSafe.UseVisualStyleBackColor = true;
            // 
            // textBoxCommentPlusAmount
            // 
            textBoxCommentPlusAmount.AnimateReadOnly = false;
            textBoxCommentPlusAmount.BorderStyle = BorderStyle.None;
            textBoxCommentPlusAmount.Depth = 0;
            textBoxCommentPlusAmount.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxCommentPlusAmount.Hint = "Комментарий";
            textBoxCommentPlusAmount.LeadingIcon = null;
            textBoxCommentPlusAmount.Location = new Point(367, 21);
            textBoxCommentPlusAmount.MaxLength = 50;
            textBoxCommentPlusAmount.MouseState = MouseState.OUT;
            textBoxCommentPlusAmount.Multiline = false;
            textBoxCommentPlusAmount.Name = "textBoxCommentPlusAmount";
            textBoxCommentPlusAmount.Size = new Size(543, 50);
            textBoxCommentPlusAmount.TabIndex = 1;
            textBoxCommentPlusAmount.Text = "";
            textBoxCommentPlusAmount.TrailingIcon = null;
            // 
            // textBoxAmountPlusSafe
            // 
            textBoxAmountPlusSafe.AnimateReadOnly = false;
            textBoxAmountPlusSafe.BorderStyle = BorderStyle.None;
            textBoxAmountPlusSafe.Depth = 0;
            textBoxAmountPlusSafe.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxAmountPlusSafe.Hint = "Сумма";
            textBoxAmountPlusSafe.LeadingIcon = null;
            textBoxAmountPlusSafe.Location = new Point(45, 21);
            textBoxAmountPlusSafe.MaxLength = 50;
            textBoxAmountPlusSafe.MouseState = MouseState.OUT;
            textBoxAmountPlusSafe.Multiline = false;
            textBoxAmountPlusSafe.Name = "textBoxAmountPlusSafe";
            textBoxAmountPlusSafe.Size = new Size(289, 50);
            textBoxAmountPlusSafe.TabIndex = 0;
            textBoxAmountPlusSafe.Text = "";
            textBoxAmountPlusSafe.TrailingIcon = null;
            // 
            // tabInventory
            // 
            tabInventory.BackColor = Color.FromArgb(64, 0, 64);
            tabInventory.Controls.Add(buttonSendInventory);
            tabInventory.Controls.Add(textBoxAmountInventory);
            tabInventory.Controls.Add(listBoxNameInventory);
            tabInventory.Location = new Point(4, 24);
            tabInventory.Name = "tabInventory";
            tabInventory.Size = new Size(1282, 716);
            tabInventory.TabIndex = 4;
            tabInventory.Text = "Инвентаризация";
            // 
            // buttonSendInventory
            // 
            buttonSendInventory.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonSendInventory.Density = MaterialButton.MaterialButtonDensity.Default;
            buttonSendInventory.Depth = 0;
            buttonSendInventory.HighEmphasis = true;
            buttonSendInventory.Icon = null;
            buttonSendInventory.Location = new Point(26, 506);
            buttonSendInventory.Margin = new Padding(4, 6, 4, 6);
            buttonSendInventory.MouseState = MouseState.HOVER;
            buttonSendInventory.Name = "buttonSendInventory";
            buttonSendInventory.NoAccentTextColor = Color.Empty;
            buttonSendInventory.Size = new Size(170, 36);
            buttonSendInventory.TabIndex = 85;
            buttonSendInventory.Text = "Отправить инвент";
            buttonSendInventory.Type = MaterialButton.MaterialButtonType.Contained;
            buttonSendInventory.UseAccentColor = false;
            buttonSendInventory.UseVisualStyleBackColor = true;
            // 
            // textBoxAmountInventory
            // 
            textBoxAmountInventory.AnimateReadOnly = false;
            textBoxAmountInventory.BorderStyle = BorderStyle.None;
            textBoxAmountInventory.Depth = 0;
            textBoxAmountInventory.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textBoxAmountInventory.Hint = "Сумма инвента";
            textBoxAmountInventory.LeadingIcon = null;
            textBoxAmountInventory.Location = new Point(26, 24);
            textBoxAmountInventory.MaxLength = 50;
            textBoxAmountInventory.MouseState = MouseState.OUT;
            textBoxAmountInventory.Multiline = false;
            textBoxAmountInventory.Name = "textBoxAmountInventory";
            textBoxAmountInventory.Size = new Size(309, 50);
            textBoxAmountInventory.TabIndex = 84;
            textBoxAmountInventory.Text = "";
            textBoxAmountInventory.TrailingIcon = null;
            // 
            // listBoxNameInventory
            // 
            listBoxNameInventory.BackColor = Color.FromArgb(40, 40, 40);
            listBoxNameInventory.BorderStyle = BorderStyle.FixedSingle;
            listBoxNameInventory.Font = new Font("Segoe UI", 17.25F);
            listBoxNameInventory.ForeColor = Color.White;
            listBoxNameInventory.FormattingEnabled = true;
            listBoxNameInventory.HorizontalScrollbar = true;
            listBoxNameInventory.Location = new Point(26, 80);
            listBoxNameInventory.Name = "listBoxNameInventory";
            listBoxNameInventory.SelectionMode = SelectionMode.MultiExtended;
            listBoxNameInventory.Size = new Size(309, 374);
            listBoxNameInventory.TabIndex = 83;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1296, 859);
            Controls.Add(materialTabControl1);
            Controls.Add(materialTabSelector1);
            MinimumSize = new Size(1296, 859);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            materialTabControl1.ResumeLayout(false);
            tabRaport.ResumeLayout(false);
            tabAdvance.ResumeLayout(false);
            tabAdvance.PerformLayout();
            tabExpenses.ResumeLayout(false);
            tabExpenses.PerformLayout();
            tabPlusSafe.ResumeLayout(false);
            tabPlusSafe.PerformLayout();
            tabInventory.ResumeLayout(false);
            tabInventory.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private MaterialTabSelector materialTabSelector1;
        private MaterialTabControl materialTabControl1;
        private TabPage tabRaport;
        private TabPage tabAdvance;
        private MaterialCheckbox checkBoxExtractSalaryFromSafe;
        private MaterialComboBox comboBoxExtractSalaryName;
        private MaterialButton buttonSendExtractSalary;
        private MaterialCheckbox checkBoxSalaryAdvance;
        private MaterialCheckbox checkBoxAdvanceExtract;
        private MaterialTextBox textBoxSalaryExtractAmount;
        private TabPage tabExpenses;
        private MaterialButton buttonSendExpenses;
        private MaterialComboBox comboBoxPhotoSendExpenses;
        private MaterialCheckbox checkBoxFromSafeExpenses;
        private MaterialCheckbox checkBoxPhotoSendExpenses;
        private MaterialTextBox textBoxCommentExpenses;
        private MaterialTextBox textBoxAmountExpenses;
        private TabPage tabPlusSafe;
        private MaterialButton buttonSendPlusSafe;
        private MaterialTextBox textBoxCommentPlusAmount;
        private MaterialTextBox textBoxAmountPlusSafe;
        private TabPage tabInventory;
        private MaterialButton buttonSendInventory;
        private MaterialTextBox textBoxAmountInventory;
        private ListBox listBoxNameInventory;

        private MaterialSkinManager materialSkinManager;
        private Components.RaportUserControl raportUserControl;
    }
}
