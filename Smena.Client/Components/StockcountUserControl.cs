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

    private sealed record ResultRow(
        string Name, string Article, string Folder,
        double Stock, double Fact, double Diff, double Pct);

    private WarehouseService? _warehouseService;
    private readonly List<Row>  _rows      = [];
    private List<ResultRow>     _negatives = [];

    // Dropdown via ToolStripDropDown
    private readonly ToolStripDropDown _dropDown = new() { AutoClose = true };
    private readonly ListBox          _dropList  = new();

    // Floating TextBox for inline fact editing in the list
    private readonly TextBox _factEditBox   = new();
    private          int     _editingRowIdx = -1;

    // Currently selected item from dropdown (before Add is pressed)
    private GrpcWarehouseItem? _selected;

    // Debounce for search
    private CancellationTokenSource? _searchCts;

    // ── Construction ──────────────────────────────────────────────
    public StockcountUserControl()
    {
        InitializeComponent();
        if (DesignMode) return;
        BuildDropdown();
        BuildFactEditBox();
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
        itemsListView.BeginUpdate();
        itemsListView.Items.Clear();
        foreach (var r in _rows)
        {
            var item = new ListViewItem(r.Name);
            item.SubItems.Add(FormatQty(r.Stock));
            item.SubItems.Add(r.Fact.HasValue ? FormatQty(r.Fact.Value) : string.Empty);
            item.Tag = r.Key;
            itemsListView.Items.Add(item);
        }
        itemsListView.EndUpdate();
        ResizeItemsColumns();
        UpdateStatus();
    }

    private void ResizeItemsColumns()
    {
        if (itemsListView.Columns.Count < 3) return;
        var fill = itemsListView.ClientSize.Width
                   - itemsListView.Columns[1].Width
                   - itemsListView.Columns[2].Width - 6;
        if (fill > 80) itemsListView.Columns[0].Width = fill;
    }

    private void UpdateStatus()
    {
        var withFact = _rows.Count(r => r.Fact.HasValue);
        labelStatus.Text = $"В списке: {_rows.Count} поз. · с фактом: {withFact}";
        buttonCalculate.Enabled = _rows.Count > 0;
    }

    // ── Редактирование факта (двойной клик → плавающий TextBox) ──
    private void BuildFactEditBox()
    {
        _factEditBox.Visible     = false;
        _factEditBox.BorderStyle = BorderStyle.FixedSingle;
        _factEditBox.Font        = new Font("Segoe UI", 10f);
        _factEditBox.TextAlign   = HorizontalAlignment.Center;
        _factEditBox.KeyDown    += FactEditBox_KeyDown;
        _factEditBox.LostFocus  += FactEditBox_LostFocus;
        itemsListView.Controls.Add(_factEditBox);
    }

    private void itemsListView_DoubleClick(object? sender, EventArgs e)
    {
        if (itemsListView.SelectedItems.Count == 0) return;
        ShowFactEditBox(itemsListView.SelectedIndices[0]);
    }

    private void ShowFactEditBox(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= itemsListView.Items.Count) return;
        var sub    = itemsListView.Items[rowIndex].SubItems[2];
        var bounds = sub.Bounds;
        if (bounds.Width < 20) return;
        _editingRowIdx = rowIndex;
        _factEditBox.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height + 2);
        _factEditBox.Text = sub.Text;
        _factEditBox.Visible = true;
        _factEditBox.Focus();
        _factEditBox.SelectAll();
    }

    private void CommitFactEdit()
    {
        _factEditBox.Visible = false;
        if (_editingRowIdx < 0 || _editingRowIdx >= itemsListView.Items.Count) return;

        var item = itemsListView.Items[_editingRowIdx];
        var key  = item.Tag as string;
        _editingRowIdx = -1;
        if (key is null) return;

        var modelRow = _rows.Find(r => r.Key == key);
        if (modelRow is null) return;

        var raw = _factEditBox.Text.Trim();
        if (string.IsNullOrEmpty(raw))
            modelRow.Fact = null;
        else if (TryParseFact(raw, out var d))
            modelRow.Fact = d;

        item.SubItems[2].Text = modelRow.Fact.HasValue ? FormatQty(modelRow.Fact.Value) : string.Empty;
        UpdateStatus();
    }

    private void FactEditBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)  { CommitFactEdit(); e.Handled = true; }
        if (e.KeyCode == Keys.Escape) { _factEditBox.Visible = false; _editingRowIdx = -1; }
    }

    private void FactEditBox_LostFocus(object? sender, EventArgs e) => CommitFactEdit();

    // ── Удаление через ПКМ ────────────────────────────────────────
    private void menuItemDelete_Click(object? sender, EventArgs e)
    {
        if (itemsListView.SelectedItems.Count == 0) return;
        var key = itemsListView.SelectedItems[0].Tag as string;
        if (key is null) return;
        _rows.RemoveAll(r => r.Key == key);
        RefreshGrid();
        panelResult.Visible = false;
    }

    // ── Calculate ─────────────────────────────────────────────────
    private void buttonCalculate_Click(object? sender, EventArgs e)
    {
        CommitFactEdit(); // сохранить незаконченное редактирование
        _negatives = _rows
            .Where(r => r.Fact.HasValue)
            .Select(r =>
            {
                double diff = r.Fact!.Value - r.Stock;
                double pct  = r.Stock > 0 ? diff / r.Stock * 100 : 0;
                return new ResultRow(r.Name, r.Article, r.Folder, r.Stock, r.Fact.Value, diff, pct);
            })
            .Where(r => r.Diff < 0)
            .OrderBy(r => r.Diff)
            .ToList();

        PopulateResultListView();

        labelResultTitle.Text = _negatives.Count == 0
            ? "Расхождений нет — отлично! ✓"
            : $"Минусовые расхождения: {_negatives.Count} поз.";

        panelResult.Visible = true;
        panelResult.BringToFront();
    }

    // ── Очистить список ──────────────────────────────────────────
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
            sb.AppendLine($"{r.Name}  →  {diffStr}  (МС: {FormatQty(r.Stock)}  Факт: {FormatQty(r.Fact)})");
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
        int[] colPcts = [55, 14, 14, 17];

        var rows = _negatives
            .Select(r => new string[]
            {
                r.Name,
                FormatQty(r.Stock), FormatQty(r.Fact),
                $"{FormatQty(r.Diff)} ({r.Pct:F1}%)"
            })
            .ToList();

        string[] headers = ["Наименование", "МС", "Факт", "Расхождение"];

        const int totalW = 900;
        int totalH = hdrH + rows.Count * rowH + 12;
        int[] colW = colPcts.Select(p => p * (totalW - 2 * padX) / 100).ToArray();

        var bmp = new Bitmap(totalW, totalH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Color.FromArgb(245, 246, 252));

        // ── Header ──
        g.FillRectangle(new SolidBrush(Color.FromArgb(60, 50, 160)), 0, 0, totalW, hdrH);

        var sf = new StringFormat
        {
            Alignment     = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming      = StringTrimming.EllipsisCharacter,
            FormatFlags   = StringFormatFlags.NoWrap
        };
        var hdrFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
        var hdrBrush = new SolidBrush(Color.White);

        int x = padX;
        for (int c = 0; c < headers.Length; c++)
        {
            g.DrawString(headers[c], hdrFont, hdrBrush, new RectangleF(x, 0, colW[c], hdrH), sf);
            x += colW[c];
        }

        // ── Rows ──
        var bodyFont  = new Font("Segoe UI", 9f);
        var diffFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
        var textBrush = new SolidBrush(Color.FromArgb(30, 35, 60));
        var altBrush  = new SolidBrush(Color.FromArgb(232, 234, 248));
        var linePen   = new Pen(Color.FromArgb(210, 215, 240));

        for (int ri = 0; ri < rows.Count; ri++)
        {
            int y = hdrH + ri * rowH;
            if (ri % 2 == 1) g.FillRectangle(altBrush, 0, y, totalW, rowH);

            x = padX;
            for (int c = 0; c < rows[ri].Length; c++)
            {
                bool isDiff = c == 3;
                var  brush  = isDiff
                    ? new SolidBrush(DiffColor(_negatives[ri].Pct))
                    : textBrush;
                g.DrawString(rows[ri][c], isDiff ? diffFont : bodyFont,
                    brush, new RectangleF(x, y, colW[c], rowH), sf);
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

    // ── Result ListView helpers ───────────────────────────────────
    private void PopulateResultListView()
    {
        resultListView.BeginUpdate();
        resultListView.Items.Clear();

        foreach (var r in _negatives)
        {
            var item = new ListViewItem(r.Name) { UseItemStyleForSubItems = false };
            item.SubItems.Add(FormatQty(r.Stock));
            item.SubItems.Add(FormatQty(r.Fact));
            var diffSub = new ListViewItem.ListViewSubItem(item,
                $"{FormatQty(r.Diff)} ({r.Pct:F1}%)")
            {
                ForeColor = DiffColor(r.Pct),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            };
            item.SubItems.Add(diffSub);
            resultListView.Items.Add(item);
        }

        if (resultListView.Columns.Count > 0)
            resultListView.Columns[0].Width = -2;

        resultListView.EndUpdate();
    }

    private static Color DiffColor(double pct) => pct switch
    {
        > -10 => Color.FromArgb(160, 100,   0),
        > -30 => Color.FromArgb(190,  70,   0),
        _     => Color.FromArgb(200,  25,  25),
    };
}
