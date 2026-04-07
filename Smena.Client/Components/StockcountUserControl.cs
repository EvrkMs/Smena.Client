using Host.Grpc.Services.Warehouse;
using Smena.Client.Services;

namespace Smena.Client.Components;

/// <summary>
/// Вкладка пересчёта товаров.
/// Поиск позиций → ввод факта → расчёт минусовых расхождений.
/// </summary>
public partial class StockcountUserControl : UserControl
{
    // ── Model ─────────────────────────────────────────────────────
    private sealed record Row(string Name, string Article, string Folder, double Stock)
    {
        public double? Fact { get; set; }
        public string Key => Name + "||" + Article;
    }

    private WarehouseService? _warehouseService;
    private readonly List<Row>        _rows      = [];
    private List<ResultCanvas.ResultRow> _negatives = [];

    // Dropdown via ToolStripDropDown
    private readonly ToolStripDropDown _dropDown = new() { AutoClose = true };
    private readonly ListBox          _dropList  = new();

    // Currently selected item from dropdown (before Add is pressed)
    private GrpcWarehouseItem? _selected;

    // Debounce for search
    private CancellationTokenSource? _searchCts;

    // ── Construction ──────────────────────────────────────────────
    public StockcountUserControl()
    {
        InitializeComponent();
        BuildDropdown();
        StyleGrids();
    }

    public void Initialize(WarehouseService warehouseService)
        => _warehouseService = warehouseService;

    // ── Dropdown setup ────────────────────────────────────────────
    private void BuildDropdown()
    {
        _dropList.BorderStyle  = BorderStyle.None;
        _dropList.BackColor    = Color.FromArgb(28, 32, 52);
        _dropList.ForeColor    = Color.FromArgb(220, 220, 235);
        _dropList.Font         = new Font("Segoe UI", 10f);
        _dropList.ItemHeight   = 30;
        _dropList.IntegralHeight = true;
        _dropList.Width        = 520;
        _dropList.DrawMode     = DrawMode.OwnerDrawFixed;
        _dropList.DrawItem    += DropList_DrawItem;
        _dropList.MouseClick  += DropList_MouseClick;
        _dropList.KeyDown     += DropList_KeyDown;

        var host = new ToolStripControlHost(_dropList)
        {
            Padding = Padding.Empty,
            Margin  = Padding.Empty,
            AutoSize = false,
            Size = new Size(_dropList.Width, 0)
        };

        _dropDown.Padding = Padding.Empty;
        _dropDown.Items.Add(host);
    }

    private void DropList_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _dropList.Items.Count) return;
        var item = (GrpcWarehouseItem)_dropList.Items[e.Index]!;

        var bg = (e.State & DrawItemState.Selected) != 0
            ? Color.FromArgb(55, 35, 120)
            : (e.Index % 2 == 0 ? Color.FromArgb(28, 32, 52) : Color.FromArgb(22, 26, 42));

        e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

        var nameFont = new Font("Segoe UI", 10f, FontStyle.Regular);
        var metaFont = new Font("Segoe UI", 8f);
        var nameRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 2, e.Bounds.Width - 130, 16);
        var metaRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 18, e.Bounds.Width - 130, 12);
        var stockRect = new Rectangle(e.Bounds.Right - 120, e.Bounds.Y + 4, 112, 20);

        e.Graphics.DrawString(item.Name, nameFont, Brushes.White, nameRect, StringFormat.GenericDefault);
        var meta = new[] { item.Article, item.Folder }.Where(s => !string.IsNullOrEmpty(s));
        e.Graphics.DrawString(string.Join(" · ", meta), metaFont, new SolidBrush(Color.FromArgb(140, 160, 190)), metaRect);
        e.Graphics.DrawString($"ост: {FormatQty(item.Stock)}", metaFont,
            new SolidBrush(Color.FromArgb(96, 165, 250)), stockRect,
            new StringFormat { Alignment = StringAlignment.Far });
    }

    private void DropList_MouseClick(object? sender, MouseEventArgs e)
    {
        var idx = _dropList.IndexFromPoint(e.Location);
        if (idx >= 0) SelectDropdownItem((GrpcWarehouseItem)_dropList.Items[idx]!);
    }

    private void DropList_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && _dropList.SelectedIndex >= 0)
            SelectDropdownItem((GrpcWarehouseItem)_dropList.Items[_dropList.SelectedIndex]!);
        else if (e.KeyCode == Keys.Escape)
            _dropDown.Close();
    }

    // ── Search ────────────────────────────────────────────────────
    private async void textBoxSearch_TextChanged(object? sender, EventArgs e)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct  = _searchCts.Token;
        var q   = textBoxSearch.Text.Trim();

        if (q.Length < 2) { _dropDown.Close(); return; }

        try
        {
            await Task.Delay(200, ct);
            if (ct.IsCancellationRequested || _warehouseService is null) return;

            var hits = await _warehouseService.SearchItemsAsync(q, 15, ct);
            if (ct.IsCancellationRequested) return;

            if (!IsHandleCreated) return;
            Invoke(() => ShowDropdown(hits));
        }
        catch (OperationCanceledException) { }
    }

    private void ShowDropdown(List<GrpcWarehouseItem> hits)
    {
        _dropDown.Close();
        _dropList.Items.Clear();

        if (hits.Count == 0) return;

        foreach (var h in hits) _dropList.Items.Add(h);
        _dropList.Height = Math.Min(hits.Count * 30, 240);
        ((ToolStripControlHost)_dropDown.Items[0]!).Size = new Size(_dropList.Width, _dropList.Height);
        _dropDown.Width = _dropList.Width;

        var pt = textBoxSearch.PointToScreen(new Point(0, textBoxSearch.Height));
        _dropDown.Show(pt);
    }

    private void textBoxSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Down && _dropDown.Visible)
        {
            _dropList.Focus();
            if (_dropList.Items.Count > 0) _dropList.SelectedIndex = 0;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            _dropDown.Close();
        }
    }

    private void SelectDropdownItem(GrpcWarehouseItem item)
    {
        _dropDown.Close();
        _selected = item;
        textBoxSearch.Text = string.Empty;

        labelSelName.Text   = item.Name;
        labelSelMeta.Text   = string.Join(" · ",
            new[] { item.Article, item.Folder }.Where(s => !string.IsNullOrEmpty(s)));
        labelSelStock.Text  = $"Остаток МС: {FormatQty(item.Stock)}";
        panelSelected.Visible = true;
        textBoxFact.Text    = string.Empty;
        textBoxFact.Focus();
    }

    private void buttonClearSelected_Click(object? sender, EventArgs e)
    {
        _selected = null;
        panelSelected.Visible = false;
        textBoxSearch.Text = string.Empty;
        textBoxSearch.Focus();
    }

    // ── Add single ────────────────────────────────────────────────
    private void buttonAdd_Click(object? sender, EventArgs e)
    {
        if (_selected is null) return;

        double? fact = null;
        if (!string.IsNullOrWhiteSpace(textBoxFact.Text))
        {
            if (!TryParseFact(textBoxFact.Text, out var f))
            {
                MessageBox.Show("Введите корректное число для факта.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFact.Focus();
                return;
            }
            fact = f;
        }

        AddOrUpdateRow(new Row(_selected.Name, _selected.Article, _selected.Folder, _selected.Stock)
        {
            Fact = fact
        });

        _selected = null;
        panelSelected.Visible = false;
        textBoxSearch.Text = string.Empty;
        textBoxSearch.Focus();
    }

    private void textBoxFact_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) { buttonAdd_Click(null, EventArgs.Empty); e.Handled = true; }
    }

    // ── Add from остатки ──────────────────────────────────────────
    private async void buttonAddFromStock_Click(object? sender, EventArgs e)
    {
        if (_warehouseService is null) return;

        var confirm = MessageBox.Show(
            "Все позиции склада, которых ещё нет в списке, будут добавлены с Фактом = 0.\n" +
            "После загрузки вы сможете изменить любое значение.\n\nПродолжить?",
            "Добавить из остатков МС",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        buttonAddFromStock.Enabled = false;
        labelStatus.Text = "Загружаю позиции со склада…";

        try
        {
            var (items, refreshedUtc, error) = await _warehouseService.GetAllItemsAsync();

            if (!string.IsNullOrWhiteSpace(error))
            {
                MessageBox.Show("Ошибка загрузки: " + error, "МойСклад", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var existing = new HashSet<string>(_rows.Select(r => r.Key));
            var added = 0;
            foreach (var it in items)
            {
                var key = it.Name + "||" + it.Article;
                if (!existing.Contains(key))
                {
                    _rows.Add(new Row(it.Name, it.Article, it.Folder, it.Stock) { Fact = 0 });
                    added++;
                }
            }

            RefreshGrid();

            var refreshInfo = string.Empty;
            if (DateTime.TryParse(refreshedUtc, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                refreshInfo = $" · кэш {dt.ToLocalTime():HH:mm}";

            labelStatus.Text = $"Добавлено {added} позиций{refreshInfo}. Всего в списке: {_rows.Count}.";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка: " + ex.Message, "МойСклад", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            buttonAddFromStock.Enabled = true;
        }
    }

    // ── Grid management ───────────────────────────────────────────
    private void AddOrUpdateRow(Row row)
    {
        var idx = _rows.FindIndex(r => r.Key == row.Key);
        if (idx >= 0)
            _rows[idx] = row;
        else
            _rows.Add(row);

        RefreshGrid();
    }

    private void RefreshGrid()
    {
        dataGridViewItems.Rows.Clear();
        foreach (var r in _rows)
        {
            var ri = dataGridViewItems.Rows.Add(
                r.Name, r.Article, r.Folder,
                FormatQty(r.Stock),
                r.Fact.HasValue ? FormatQty(r.Fact.Value) : string.Empty,
                "✕");
            dataGridViewItems.Rows[ri].Tag = r.Key;
        }
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var withFact = _rows.Count(r => r.Fact.HasValue);
        labelStatus.Text = $"В списке: {_rows.Count} поз. · с фактом: {withFact}";
        buttonCalculate.Enabled = _rows.Count > 0;
    }

    // ── Fact cell edit ────────────────────────────────────────────
    private void dataGridViewItems_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex != ColIdxFact) return;

        var row = dataGridViewItems.Rows[e.RowIndex];
        var key = row.Tag as string;
        if (key is null) return;
        var modelRow = _rows.Find(r => r.Key == key);
        if (modelRow is null) return;

        var raw = row.Cells[ColIdxFact].Value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            modelRow.Fact = null;
        else if (TryParseFact(raw, out var d))
            modelRow.Fact = d;

        UpdateStatus();
    }

    // ── Delete row ────────────────────────────────────────────────
    private void dataGridViewItems_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex != ColIdxDelete || e.RowIndex < 0) return;
        var key = dataGridViewItems.Rows[e.RowIndex].Tag as string;
        if (key is null) return;
        _rows.RemoveAll(r => r.Key == key);
        RefreshGrid();
        panelResult.Visible = false;
    }

    // ── Calculate ─────────────────────────────────────────────────
    private void buttonCalculate_Click(object? sender, EventArgs e)
    {
        _negatives = _rows
            .Where(r => r.Fact.HasValue)
            .Select(r =>
            {
                double diff = r.Fact!.Value - r.Stock;
                double pct  = r.Stock > 0 ? diff / r.Stock * 100 : 0;
                return new ResultCanvas.ResultRow(r.Name, r.Article, r.Folder, r.Stock, r.Fact.Value, diff, pct);
            })
            .Where(r => r.Diff < 0)
            .OrderBy(r => r.Diff)
            .ToList();

        resultCanvas.SetRows(_negatives);

        labelResultTitle.Text = _negatives.Count == 0
            ? "Расхождений нет — отлично! ✓"
            : $"Минусовые расхождения: {_negatives.Count} поз.";

        panelResult.Visible = true;
        panelResult.BringToFront();
    }

    // ── Clear list ────────────────────────────────────────────────
    private void buttonClearList_Click(object? sender, EventArgs e)
    {
        if (_rows.Count == 0) return;
        if (MessageBox.Show("Очистить весь список?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _rows.Clear();
        RefreshGrid();
        panelResult.Visible = false;
    }

    // ── Style ─────────────────────────────────────────────────────
    private const int ColIdxFact   = 4;
    private const int ColIdxDelete = 5;

    private void StyleGrids()
    {
        StyleGrid(dataGridViewItems, editable: true);

        // Факт — тёмно-голубой фон (подсказка что редактируемо)
        dataGridViewItems.Columns[ColIdxFact].DefaultCellStyle.BackColor = Color.FromArgb(20, 40, 70);
        // Кнопка удаления
        dataGridViewItems.Columns[ColIdxDelete].DefaultCellStyle.ForeColor = Color.FromArgb(248, 113, 113);
        dataGridViewItems.Columns[ColIdxDelete].DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        dataGridViewItems.Columns[ColIdxDelete].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private static void StyleGrid(DataGridView dgv, bool editable)
    {
        dgv.BackgroundColor              = Color.FromArgb(18, 22, 36);
        dgv.GridColor                    = Color.FromArgb(45, 55, 72);
        dgv.BorderStyle                  = BorderStyle.None;
        dgv.RowHeadersVisible            = false;
        dgv.AllowUserToAddRows           = false;
        dgv.AllowUserToDeleteRows        = false;
        dgv.AllowUserToResizeRows        = false;
        dgv.ReadOnly                     = !editable;
        dgv.SelectionMode                = DataGridViewSelectionMode.FullRowSelect;
        dgv.AutoSizeColumnsMode          = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.ColumnHeadersHeightSizeMode  = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.ColumnHeadersHeight          = 34;
        dgv.EnableHeadersVisualStyles    = false;

        var hs = dgv.ColumnHeadersDefaultCellStyle;
        hs.BackColor = Color.FromArgb(30, 18, 80);
        hs.ForeColor = Color.FromArgb(200, 200, 220);
        hs.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
        hs.SelectionBackColor = hs.BackColor;

        var rs = dgv.DefaultCellStyle;
        rs.BackColor          = Color.FromArgb(18, 22, 36);
        rs.ForeColor          = Color.FromArgb(220, 220, 235);
        rs.SelectionBackColor = Color.FromArgb(55, 35, 120);
        rs.SelectionForeColor = Color.White;
        rs.Font               = new Font("Segoe UI", 9f);

        var alt = dgv.AlternatingRowsDefaultCellStyle;
        alt.BackColor          = Color.FromArgb(24, 28, 46);
        alt.SelectionBackColor = Color.FromArgb(55, 35, 120);
    }

    // ── Copy ──────────────────────────────────────────────────────
    private void buttonCopyText_Click(object? sender, EventArgs e)
    {
        if (_negatives.Count == 0) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Пересчёт склада  {DateTime.Now:dd.MM.yyyy HH:mm}");
        sb.AppendLine(new string('─', 50));
        foreach (var r in _negatives)
        {
            var diffStr = $"{FormatQty(r.Diff)} ({r.Pct:F1}%)";
            sb.AppendLine($"{r.Name}  →  {diffStr}");
            if (!string.IsNullOrEmpty(r.Article))
                sb.AppendLine($"   {r.Article}  МС: {FormatQty(r.Stock)}  Факт: {FormatQty(r.Fact)}");
        }
        Clipboard.SetText(sb.ToString().TrimEnd());
        FlashButton(buttonCopyText, "✓ Скопировано", "Текст");
    }

    private void buttonCopyImage_Click(object? sender, EventArgs e)
    {
        if (_negatives.Count == 0) return;
        using var bmp = RenderResultTable();
        Clipboard.SetImage(bmp);
        FlashButton(buttonCopyImage, "✓", "Фото");
    }

    private void FlashButton(MaterialSkin.Controls.MaterialButton btn, string flash, string orig)
    {
        btn.Text = flash;
        Task.Delay(2000).ContinueWith(_ =>
        {
            if (IsHandleCreated) Invoke(() => btn.Text = orig);
        });
    }

    private Bitmap RenderResultTable()
    {
        const int rowH = 34, hdrH = 42, padX = 14;
        int[] colPcts = [42, 10, 14, 10, 10, 14];

        var rows = _negatives
            .Select(r => new string[]
            {
                r.Name, r.Article, r.Folder,
                FormatQty(r.Stock), FormatQty(r.Fact),
                $"{FormatQty(r.Diff)} ({r.Pct:F1}%)"
            })
            .ToList();

        string[] headers = ["Наименование", "Артикул", "Папка", "Остаток МС", "Факт", "Расхождение"];

        const int totalW = 1040;
        int totalH = hdrH + rows.Count * rowH + 16;
        int[] colW = colPcts.Select(p => p * (totalW - 2 * padX) / 100).ToArray();

        var bmp = new Bitmap(totalW, totalH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode            = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint        = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Color.FromArgb(18, 22, 36));

        // ── Header ──
        g.FillRectangle(new SolidBrush(Color.FromArgb(30, 18, 80)), 0, 0, totalW, hdrH);
        g.DrawLine(new Pen(Color.FromArgb(70, 55, 140)), 0, hdrH, totalW, hdrH);

        var sf = new StringFormat
        {
            Alignment     = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming      = StringTrimming.EllipsisCharacter,
            FormatFlags   = StringFormatFlags.NoWrap
        };
        var hdrFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
        var hdrBrush = new SolidBrush(Color.FromArgb(196, 181, 253));

        int x = padX;
        for (int c = 0; c < headers.Length; c++)
        {
            g.DrawString(headers[c], hdrFont, hdrBrush, new RectangleF(x, 0, colW[c], hdrH), sf);
            x += colW[c];
        }

        // ── Rows ──
        var bodyFont  = new Font("Segoe UI", 9f);
        var diffFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
        var textBrush = new SolidBrush(Color.FromArgb(220, 220, 235));
        var diffBrush = new SolidBrush(Color.FromArgb(248, 113, 113));
        var altBrush  = new SolidBrush(Color.FromArgb(22, 26, 44));
        var linePen   = new Pen(Color.FromArgb(35, 45, 65));

        for (int ri = 0; ri < rows.Count; ri++)
        {
            int y = hdrH + ri * rowH;
            if (ri % 2 == 1) g.FillRectangle(altBrush, 0, y, totalW, rowH);

            x = padX;
            for (int c = 0; c < rows[ri].Length; c++)
            {
                bool isDiff = c == 5;
                g.DrawString(rows[ri][c], isDiff ? diffFont : bodyFont,
                    isDiff ? diffBrush : textBrush,
                    new RectangleF(x, y, colW[c], rowH), sf);
                x += colW[c];
            }
            g.DrawLine(linePen, 0, y + rowH - 1, totalW, y + rowH - 1);
        }

        return bmp;
    }

    // ── Helpers ───────────────────────────────────────────────────
    private static bool TryParseFact(string s, out double value)
        => double.TryParse(s.Replace(',', '.'),
               System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture,
               out value) && value >= 0;

    private static string FormatQty(double v)
        => v == Math.Floor(v)
            ? ((long)v).ToString()
            : v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    // ── ResultCanvas ─────────────────────────────────────────────
    internal sealed class ResultCanvas : Panel
    {
        public sealed record ResultRow(
            string Name, string Article, string Folder,
            double Stock, double Fact, double Diff, double Pct);

        private const int RowH = 58;
        private const int HdrH = 38;

        private static readonly Color BgRow0  = Color.FromArgb(20, 24, 40);
        private static readonly Color BgRow1  = Color.FromArgb(25, 30, 50);
        private static readonly Color BgHdr   = Color.FromArgb(30, 18, 80);
        private static readonly Color ClrName = Color.FromArgb(225, 225, 240);
        private static readonly Color ClrMeta = Color.FromArgb(120, 135, 165);
        private static readonly Color ClrBlue = Color.FromArgb(96, 165, 250);
        private static readonly Color ClrDiv  = Color.FromArgb(38, 44, 66);

        private static Color DiffColor(double pct) => pct switch
        {
            > -10  => Color.FromArgb(251, 191,  36),
            > -30  => Color.FromArgb(249, 115,  22),
            _      => Color.FromArgb(248,  68,  68),
        };

        private List<ResultRow> _rows = [];

        public ResultCanvas()
        {
            DoubleBuffered = true;
            AutoScroll     = true;
        }

        public void SetRows(IReadOnlyList<ResultRow> rows)
        {
            _rows = [..rows];
            AutoScrollMinSize = new Size(1, HdrH + _rows.Count * RowH);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g  = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            int oy = AutoScrollPosition.Y;
            int w  = ClientSize.Width;

            // Header
            int hY = oy;
            g.FillRectangle(new SolidBrush(BgHdr), 0, hY, w, HdrH);
            var hdrFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            var hdrBr   = new SolidBrush(Color.FromArgb(196, 181, 253));
            var sfL = new StringFormat { Alignment = StringAlignment.Near,  LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            var sfR = new StringFormat { Alignment = StringAlignment.Far ,  LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };
            int rightW = Math.Min(340, w / 3);
            g.DrawString("Наименование / Артикул", hdrFont, hdrBr, new RectangleF(18, hY, w - rightW - 18, HdrH), sfL);
            g.DrawString("МС  |  Факт", hdrFont, hdrBr, new RectangleF(w - rightW, hY, rightW / 2, HdrH), sfL);
            g.DrawString("Расхождение", hdrFont, hdrBr, new RectangleF(w - rightW / 2, hY, rightW / 2 - 10, HdrH), sfR);

            // Rows
            var nameFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            var metaFont = new Font("Segoe UI", 8f);
            var diffFont = new Font("Segoe UI", 10f, FontStyle.Bold);

            for (int i = 0; i < _rows.Count; i++)
            {
                var r  = _rows[i];
                int rY = oy + HdrH + i * RowH;
                var bg = i % 2 == 0 ? BgRow0 : BgRow1;
                g.FillRectangle(new SolidBrush(bg), 0, rY, w, RowH);

                // Accent bar
                var accent = DiffColor(r.Pct);
                g.FillRectangle(new SolidBrush(accent), 0, rY + 6, 4, RowH - 12);

                // Name
                g.DrawString(r.Name, nameFont, new SolidBrush(ClrName),
                    new RectangleF(14, rY + 6, w - rightW - 18, 20), sfL);

                // Meta
                var meta = string.Join("  ·  ",
                    new[] { r.Article, r.Folder }.Where(s => !string.IsNullOrEmpty(s)));
                g.DrawString(meta, metaFont, new SolidBrush(ClrMeta),
                    new RectangleF(14, rY + 28, w - rightW - 18, 16), sfL);

                // Stock + Fact
                var stockFact = $"МС: {Fmt(r.Stock)}    Факт: {Fmt(r.Fact)}";
                g.DrawString(stockFact, metaFont, new SolidBrush(ClrBlue),
                    new RectangleF(w - rightW, rY + 6, rightW / 2 - 6, RowH), sfL);

                // Diff
                var diffStr = $"{Fmt(r.Diff)}";
                var pctStr  = $" ({r.Pct:F1}%)";
                var diffBr  = new SolidBrush(DiffColor(r.Pct));
                g.DrawString(diffStr + pctStr, diffFont, diffBr,
                    new RectangleF(w - rightW / 2, rY + 12, rightW / 2 - 10, 24), sfR);

                // Divider
                g.DrawLine(new Pen(ClrDiv), 0, rY + RowH - 1, w, rY + RowH - 1);
            }

            if (_rows.Count == 0)
            {
                var emptyFont = new Font("Segoe UI", 10f);
                g.DrawString("Нет минусовых расхождений ✓",
                    emptyFont, new SolidBrush(Color.FromArgb(80, 200, 120)),
                    new RectangleF(0, oy + HdrH + 16, w, 30),
                    new StringFormat { Alignment = StringAlignment.Center });
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        private static string Fmt(double v)
            => v == Math.Floor(v)
                ? ((long)v).ToString()
                : v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }
}
