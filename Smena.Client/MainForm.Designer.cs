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
            advanceUserControl1 = new Smena.Client.Components.AdvanceUserControl();
            tabExpenses = new TabPage();
            expenseUserControl1 = new Smena.Client.Components.ExpenseUserControl();
            tabPlusSafe = new TabPage();
            comingUserControl1 = new Smena.Client.Components.ComingUserControl();
            tabStockcount = new TabPage();
            stockcountUserControl1 = new Smena.Client.Components.StockcountUserControl();
            materialTabControl1.SuspendLayout();
            tabRaport.SuspendLayout();
            tabAdvance.SuspendLayout();
            tabExpenses.SuspendLayout();
            tabPlusSafe.SuspendLayout();
            tabStockcount.SuspendLayout();
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
            materialTabControl1.Controls.Add(tabStockcount);
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
            tabRaport.BackColor = Color.FromArgb(30, 18, 80);
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
            tabAdvance.BackColor = Color.FromArgb(30, 18, 80);
            tabAdvance.Controls.Add(advanceUserControl1);
            tabAdvance.Location = new Point(4, 24);
            tabAdvance.Name = "tabAdvance";
            tabAdvance.Size = new Size(1282, 716);
            tabAdvance.TabIndex = 1;
            tabAdvance.Text = "Аванс";
            // 
            // advanceUserControl1
            // 
            advanceUserControl1.BackColor = Color.Transparent;
            advanceUserControl1.Dock = DockStyle.Fill;
            advanceUserControl1.Location = new Point(0, 0);
            advanceUserControl1.Name = "advanceUserControl1";
            advanceUserControl1.Size = new Size(1282, 716);
            advanceUserControl1.TabIndex = 0;
            // 
            // tabExpenses
            // 
            tabExpenses.BackColor = Color.FromArgb(30, 18, 80);
            tabExpenses.Controls.Add(expenseUserControl1);
            tabExpenses.ForeColor = SystemColors.ControlText;
            tabExpenses.Location = new Point(4, 24);
            tabExpenses.Name = "tabExpenses";
            tabExpenses.Size = new Size(1282, 716);
            tabExpenses.TabIndex = 2;
            tabExpenses.Text = "Расходы";
            // 
            // expenseUserControl1
            // 
            expenseUserControl1.BackColor = Color.Transparent;
            expenseUserControl1.Dock = DockStyle.Fill;
            expenseUserControl1.Location = new Point(0, 0);
            expenseUserControl1.Name = "expenseUserControl1";
            expenseUserControl1.Size = new Size(1282, 716);
            expenseUserControl1.TabIndex = 0;
            // 
            // tabPlusSafe
            // 
            tabPlusSafe.BackColor = Color.FromArgb(30, 18, 80);
            tabPlusSafe.Controls.Add(comingUserControl1);
            tabPlusSafe.Location = new Point(4, 24);
            tabPlusSafe.Name = "tabPlusSafe";
            tabPlusSafe.Size = new Size(1282, 716);
            tabPlusSafe.TabIndex = 3;
            tabPlusSafe.Text = "Плюс к сейфу";
            // 
            // comingUserControl1
            // 
            comingUserControl1.BackColor = Color.Transparent;
            comingUserControl1.Dock = DockStyle.Fill;
            comingUserControl1.Location = new Point(0, 0);
            comingUserControl1.Name = "comingUserControl1";
            comingUserControl1.Size = new Size(1282, 716);
            comingUserControl1.TabIndex = 0;
            // 
            // tabStockcount
            // 
            tabStockcount.BackColor = Color.FromArgb(18, 22, 36);
            tabStockcount.Controls.Add(stockcountUserControl1);
            tabStockcount.Location = new Point(4, 24);
            tabStockcount.Name = "tabStockcount";
            tabStockcount.Size = new Size(1282, 716);
            tabStockcount.TabIndex = 4;
            tabStockcount.Text = "Пересчёт";
            // 
            // stockcountUserControl1
            // 
            stockcountUserControl1.BackColor = Color.FromArgb(18, 22, 36);
            stockcountUserControl1.Dock = DockStyle.Fill;
            stockcountUserControl1.Location = new Point(0, 0);
            stockcountUserControl1.Name = "stockcountUserControl1";
            stockcountUserControl1.Size = new Size(1282, 716);
            stockcountUserControl1.TabIndex = 0;
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
            tabExpenses.ResumeLayout(false);
            tabPlusSafe.ResumeLayout(false);
            tabStockcount.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private MaterialTabSelector materialTabSelector1;
        private MaterialTabControl materialTabControl1;
        private TabPage tabRaport;
        private TabPage tabAdvance;
        private TabPage tabExpenses;
        private TabPage tabPlusSafe;
        private TabPage tabStockcount;

        private MaterialSkinManager materialSkinManager;
        private Components.RaportUserControl raportUserControl;
        private Components.AdvanceUserControl advanceUserControl1;
        private Components.ExpenseUserControl expenseUserControl1;
        private Components.ComingUserControl comingUserControl1;
        private Components.StockcountUserControl stockcountUserControl1;
    }
}
