using MaterialSkin.Controls;

namespace Smena.Client.Components;

partial class StockcountUserControl
{
    private System.ComponentModel.IContainer components = null;

    // Search bar
    private Panel                                 panelSearch;
    private Label                                 labelSearchHint;
    private MaterialTextBox                       textBoxSearch;

    // Selected item info + fact entry
    private Panel                                 panelSelected;
    private Label                                 labelSelName;
    private Label                                 labelSelMeta;
    private Label                                 labelSelStock;
    private MaterialTextBox                       textBoxFact;
    private Label                                 labelFactHint;
    private MaterialButton                        buttonAdd;
    private Button                                buttonClearSelected;

    // Action bar
    private Panel                                 panelActions;
    private MaterialButton                        buttonAddFromStock;
    private MaterialButton                        buttonCalculate;
    private MaterialButton                        buttonClearList;
    private Label                                 labelStatus;

    // Items DataGridView
    private DataGridView                          dataGridViewItems;

    // Result panel (bottom, initially hidden)
    private Panel                                 panelResult;
    private Panel                                 panelResultHeader;
    private Label                                 labelResultTitle;
    private Button                                buttonCloseResult;
    private MaterialButton                        buttonCopyText;
    private MaterialButton                        buttonCopyImage;
    private ListView                              resultListView;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        // ── Search panel ──────────────────────────────────────────
        labelSearchHint = new Label
        {
            Text      = "Поиск:",
            ForeColor = Color.FromArgb(160, 170, 200),
            Font      = new Font("Segoe UI", 9f),
            AutoSize  = true,
            Location  = new Point(12, 26),
        };

        textBoxSearch = new MaterialTextBox
        {
            Location       = new Point(64, 5),
            Size           = new Size(500, 48),
            Hint           = "Введите название или артикул...",
            Depth          = 0,
            UseAccent      = true,
            MouseState     = MaterialSkin.MouseState.OUT,
        };
        textBoxSearch.TextChanged += textBoxSearch_TextChanged;
        textBoxSearch.KeyDown     += textBoxSearch_KeyDown;

        panelSearch = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 58,
            Padding   = new Padding(6, 0, 8, 0),
        };
        panelSearch.Controls.AddRange([labelSearchHint, textBoxSearch]);

        // ── Selected item panel ───────────────────────────────────
        labelSelName = new Label
        {
            Location  = new Point(14, 8),
            Size      = new Size(560, 20),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Text      = string.Empty,
        };

        labelSelMeta = new Label
        {
            Location  = new Point(14, 28),
            Size      = new Size(560, 16),
            ForeColor = Color.FromArgb(140, 160, 190),
            Font      = new Font("Segoe UI", 8.5f),
            Text      = string.Empty,
        };

        labelSelStock = new Label
        {
            Location  = new Point(14, 44),
            Size      = new Size(400, 16),
            ForeColor = Color.FromArgb(96, 165, 250),
            Font      = new Font("Segoe UI", 8.5f),
            Text      = string.Empty,
        };

        labelFactHint = new Label
        {
            Text      = "Факт:",
            ForeColor = Color.FromArgb(160, 170, 200),
            Font      = new Font("Segoe UI", 9f),
            AutoSize  = true,
            Location  = new Point(14, 72),
        };

        textBoxFact = new MaterialTextBox
        {
            Location   = new Point(60, 54),
            Size       = new Size(130, 48),
            Hint       = "0",
            Depth      = 0,
            UseAccent  = true,
            MouseState = MaterialSkin.MouseState.OUT,
        };
        textBoxFact.KeyDown += textBoxFact_KeyDown;

        buttonAdd = new MaterialButton
        {
            Text            = "Добавить",
            Location        = new Point(200, 68),
            AutoSize        = false,
            Size            = new Size(120, 36),
            Depth           = 0,
            HighEmphasis    = true,
            Type            = MaterialButton.MaterialButtonType.Contained,
            UseAccentColor  = false,
            MouseState      = MaterialSkin.MouseState.HOVER,
        };
        buttonAdd.Click += buttonAdd_Click;

        buttonClearSelected = new Button
        {
            Text      = "✕",
            Location  = new Point(666, 8),
            Size      = new Size(28, 28),
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(140, 145, 165),
            Font      = new Font("Segoe UI", 11f),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
        };
        buttonClearSelected.FlatAppearance.BorderSize = 0;
        buttonClearSelected.Click += buttonClearSelected_Click;

        panelSelected = new Panel
        {
            Dock        = DockStyle.Top,
            Height      = 108,
            Visible     = false,
        };
        panelSelected.Paint += (s, e) =>
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(99, 102, 241)),
                0, 0, 4, ((Panel)s!).Height);
        panelSelected.Controls.AddRange([
            labelSelName, labelSelMeta, labelSelStock,
            labelFactHint, textBoxFact, buttonAdd, buttonClearSelected
        ]);

        // ── Action bar ────────────────────────────────────────────
        buttonAddFromStock = new MaterialButton
        {
            Text           = "Из остатков МС",
            Dock           = DockStyle.Left,
            AutoSize       = false,
            Width          = 180,
            Depth          = 0,
            HighEmphasis   = true,
            Type           = MaterialButton.MaterialButtonType.Contained,
            UseAccentColor = false,
            MouseState     = MaterialSkin.MouseState.HOVER,
        };
        buttonAddFromStock.Click += buttonAddFromStock_Click;

        buttonCalculate = new MaterialButton
        {
            Text           = "Рассчитать",
            Dock           = DockStyle.Left,
            AutoSize       = false,
            Width          = 150,
            Depth          = 0,
            HighEmphasis   = true,
            Type           = MaterialButton.MaterialButtonType.Contained,
            UseAccentColor = true,
            MouseState     = MaterialSkin.MouseState.HOVER,
            Enabled        = false,
        };
        buttonCalculate.Click += buttonCalculate_Click;

        buttonClearList = new MaterialButton
        {
            Text           = "Очистить",
            Dock           = DockStyle.Left,
            AutoSize       = false,
            Width          = 120,
            Depth          = 0,
            HighEmphasis   = false,
            Type           = MaterialButton.MaterialButtonType.Text,
            UseAccentColor = false,
            MouseState     = MaterialSkin.MouseState.HOVER,
        };
        buttonClearList.Click += buttonClearList_Click;

        labelStatus = new Label
        {
            Dock      = DockStyle.Fill,
            ForeColor = Color.FromArgb(120, 135, 160),
            Font      = new Font("Segoe UI", 8.5f),
            Text      = "Список пуст",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(10, 0, 0, 0),
        };

        panelActions = new Panel
        {
            Dock    = DockStyle.Top,
            Height  = 52,
            Padding = new Padding(4, 8, 4, 8),
        };
        panelActions.Controls.AddRange([
            labelStatus, buttonClearList, buttonCalculate, buttonAddFromStock
        ]);

        // ── Items DataGridView ────────────────────────────────────
        dataGridViewItems = new DataGridView { Dock = DockStyle.Fill };
        dataGridViewItems.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Наименование", Name = "ColName",   FillWeight = 100, ReadOnly = true  },
            new DataGridViewTextBoxColumn { HeaderText = "Артикул",      Name = "ColArt",    FillWeight = 25,  ReadOnly = true  },
            new DataGridViewTextBoxColumn { HeaderText = "Папка",        Name = "ColFolder", FillWeight = 35,  ReadOnly = true  },
            new DataGridViewTextBoxColumn { HeaderText = "Остаток МС",   Name = "ColStock",  FillWeight = 20,  ReadOnly = true  },
            new DataGridViewTextBoxColumn { HeaderText = "Факт",         Name = "ColFact",   FillWeight = 20,  ReadOnly = false },
            new DataGridViewButtonColumn  { HeaderText = "",             Name = "ColDel",    FillWeight = 7,   Text = "✕",  UseColumnTextForButtonValue = true }
        );
        dataGridViewItems.CellEndEdit += dataGridViewItems_CellEndEdit;
        dataGridViewItems.CellClick   += dataGridViewItems_CellClick;

        // ── Result panel ──────────────────────────────────────────
        labelResultTitle = new Label
        {
            Dock      = DockStyle.Fill,
            ForeColor = Color.FromArgb(96, 165, 250),
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(8, 0, 0, 0),
        };

        buttonCopyText = new MaterialButton
        {
            Text           = "Текст",
            Dock           = DockStyle.Right,
            AutoSize       = false,
            Width          = 90,
            Depth          = 0,
            HighEmphasis   = false,
            Type           = MaterialButton.MaterialButtonType.Text,
            UseAccentColor = true,
            MouseState     = MaterialSkin.MouseState.HOVER,
        };
        buttonCopyText.Click += buttonCopyText_Click;

        buttonCopyImage = new MaterialButton
        {
            Text           = "Фото",
            Dock           = DockStyle.Right,
            AutoSize       = false,
            Width          = 80,
            Depth          = 0,
            HighEmphasis   = false,
            Type           = MaterialButton.MaterialButtonType.Text,
            UseAccentColor = true,
            MouseState     = MaterialSkin.MouseState.HOVER,
        };
        buttonCopyImage.Click += buttonCopyImage_Click;

        buttonCloseResult = new Button
        {
            Text      = "✕",
            Dock      = DockStyle.Right,
            Width     = 42,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(140, 145, 165),
            Font      = new Font("Segoe UI", 11f),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
        };
        buttonCloseResult.FlatAppearance.BorderSize = 0;
        buttonCloseResult.Click += (s, e) => panelResult.Visible = false;

        panelResultHeader = new Panel
        {
            Dock   = DockStyle.Top,
            Height = 38,
        };
        panelResultHeader.Controls.AddRange([
            buttonCloseResult, buttonCopyImage, buttonCopyText, labelResultTitle
        ]);

        resultListView = new ListView
        {
            Dock                            = DockStyle.Fill,
            View                            = View.Details,
            FullRowSelect                   = true,
            GridLines                       = false,
            BackColor                       = Color.FromArgb(245, 246, 252),
            ForeColor                       = Color.FromArgb(30, 35, 60),
            Font                            = new Font("Segoe UI", 9.5f),
            BorderStyle                     = BorderStyle.None,
            UseCompatibleStateImageBehavior = false,
            MultiSelect                     = false,
            HeaderStyle                     = ColumnHeaderStyle.Nonclickable,
        };
        resultListView.Columns.AddRange([
            new ColumnHeader { Text = "Наименование", Width = 300 },
            new ColumnHeader { Text = "МС",           Width = 80  },
            new ColumnHeader { Text = "Факт",         Width = 80  },
            new ColumnHeader { Text = "Расхождение",  Width = 130 },
        ]);

        panelResult = new Panel
        {
            Dock    = DockStyle.Bottom,
            Height  = 260,
            Visible = false,
        };
        panelResult.Controls.AddRange([resultListView, panelResultHeader]);

        // ── Assemble ─────────────────────────────────────────────
        Controls.AddRange([panelResult, dataGridViewItems, panelActions, panelSelected, panelSearch]);
    }
}
