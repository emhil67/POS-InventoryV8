using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AI_Inventory_Final
{
    /// <summary>
    /// Inventory dashboard: month filter, KPI cards (Balance / Stock In / Stock Out / Sales),
    /// bar chart (stock by product), pie (stock by category), donut (sales by category)
    /// and a line chart (sales by product). All charts are drawn with GDI+, no extra packages.
    /// </summary>
    public class InventoryDashboardForm : Form
    {
        private sealed class Slice
        {
            public string Label = "";
            public double Value;
        }

        /// <summary>A card-style panel that paints its own title and delegates the plot area to Painter.</summary>
        private sealed class ChartPanel : Panel
        {
            public string Title = "";
            public Action<Graphics, Rectangle> Painter;

            public ChartPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Surface;
                Margin = new Padding(6);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(Theme.Surface);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (var pen = new Pen(Theme.Border)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                using (var accent = new SolidBrush(Theme.Orange)) g.FillRectangle(accent, 0, 12, 4, 18);
                using (var f = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                using (var b = new SolidBrush(Theme.Text))
                    g.DrawString(Title, f, b, 16, 10);

                var plot = new Rectangle(12, 44, Math.Max(10, Width - 24), Math.Max(10, Height - 54));
                if (Painter != null) Painter(g, plot);
            }
        }

        private static readonly Color[] Palette =
        {
            Theme.Orange, Theme.Blue, Theme.Green, Theme.Pink,
            Color.FromArgb(124, 92, 255), Color.FromArgb(20, 184, 166), Theme.Amber, Theme.Red
        };

        private readonly CultureInfo _ci = CultureInfo.InvariantCulture;

        private ComboBox cmbYear;
        private Button btnRefresh;
        private Label lblPeriod;
        private Button[] _monthBtns = new Button[12];
        private Label lblBalance, lblStockIn, lblStockOut, lblSales;
        private Label capBalance, capStockIn, capStockOut, capSales;
        private ChartPanel chBar, chPie, chDonut, chLine;

        private int? _month = null;          // null = whole year
        private bool _loading = false;

        private List<Slice> _stockByProduct = new List<Slice>();
        private List<Slice> _stockByCategory = new List<Slice>();
        private List<Slice> _salesByCategory = new List<Slice>();
        private List<Slice> _salesByProduct = new List<Slice>();

        public InventoryDashboardForm()
        {
            Text = "Inventory Dashboard";
            Size = new Size(1180, 860);
            MinimumSize = new Size(1020, 760);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            BuildUi();
            Theme.Apply(this);
            StyleMonthButtons();

            Load += (s, e) => { LoadYears(); RefreshAll(); };
        }

        // =====================================================================
        // UI
        // =====================================================================
        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6,
                BackColor = Theme.Bg, Padding = new Padding(10)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildMonthCard(), 0, 1);
            root.Controls.Add(BuildKpiRow(), 0, 2);

            chBar = new ChartPanel { Dock = DockStyle.Fill, Title = "Balance in Stock by Product" };
            chBar.Painter = (g, r) => DrawBars(g, r, _stockByProduct, Theme.Blue);
            root.Controls.Add(chBar, 0, 3);

            var pies = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Bg, Margin = new Padding(0) };
            pies.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pies.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            chPie = new ChartPanel { Dock = DockStyle.Fill, Title = "Balance in Stock by Category" };
            chPie.Painter = (g, r) => DrawPie(g, r, _stockByCategory, false, "", v => v.ToString("N0", _ci));
            chDonut = new ChartPanel { Dock = DockStyle.Fill, Title = "Sales by Category" };
            chDonut.Painter = (g, r) => DrawPie(g, r, _salesByCategory, true, "₱" + _salesByCategory.Sum(x => x.Value).ToString("N0", _ci), v => "₱" + v.ToString("N0", _ci));
            pies.Controls.Add(chPie, 0, 0);
            pies.Controls.Add(chDonut, 1, 0);
            root.Controls.Add(pies, 0, 4);

            chLine = new ChartPanel { Dock = DockStyle.Fill, Title = "Total Sales by Product" };
            chLine.Painter = (g, r) => DrawLineChart(g, r, _salesByProduct, Theme.Orange);
            root.Controls.Add(chLine, 0, 5);

            Controls.Add(root);
        }

        private Control BuildHeader()
        {
            var hdr = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar, Margin = new Padding(6) };
            Theme.CardBorder(hdr, Theme.Border, Theme.Orange);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Sidebar };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var title = new Label
            {
                Text = "INVENTORY DASHBOARD", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Theme.Text, Padding = new Padding(18, 0, 0, 0)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Theme.Sidebar, Padding = new Padding(0, 14, 14, 0)
            };
            lblPeriod = new Label { AutoSize = true, ForeColor = Theme.Muted, Margin = new Padding(0, 6, 20, 0), Text = "" };
            var lblYear = new Label { AutoSize = true, ForeColor = Theme.Text, Margin = new Padding(0, 6, 6, 0), Text = "Year", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbYear = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90, Margin = new Padding(0, 3, 14, 0) };
            cmbYear.SelectedIndexChanged += (s, e) => { if (!_loading) RefreshAll(); };
            btnRefresh = new Button { Text = "⟳  Refresh", Size = new Size(104, 32), BackColor = Theme.Orange, ForeColor = Theme.Ink, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(0) };
            btnRefresh.Click += (s, e) => RefreshAll();
            flow.Controls.AddRange(new Control[] { lblPeriod, lblYear, cmbYear, btnRefresh });

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(flow, 1, 0);
            hdr.Controls.Add(layout);
            return hdr;
        }

        private Control BuildMonthCard()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Margin = new Padding(6), Padding = new Padding(14, 8, 14, 10) };
            Theme.CardBorder(card, Theme.Border);

            var top = new Panel { Dock = DockStyle.Top, Height = 26, BackColor = Theme.Surface };
            var title = new Label { Text = "Month", Dock = DockStyle.Left, AutoSize = true, ForeColor = Theme.Text, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
            var hint = new Label { Text = "Click a month to filter · click it again to show the whole year", Dock = DockStyle.Right, AutoSize = true, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 8.5f), TextAlign = ContentAlignment.MiddleRight };
            top.Controls.Add(hint);
            top.Controls.Add(title);

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 2, BackColor = Theme.Surface };
            for (int c = 0; c < 6; c++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            string[] names = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;
            for (int i = 0; i < 12; i++)
            {
                int month = i + 1;
                var b = new Button
                {
                    Text = names[i], Dock = DockStyle.Fill, Margin = new Padding(3), FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand, Tag = "keep", UseVisualStyleBackColor = false
                };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s, e) => { _month = (_month == month) ? (int?)null : month; StyleMonthButtons(); RefreshAll(); };
                _monthBtns[i] = b;
                grid.Controls.Add(b, i % 6, i / 6);
            }

            card.Controls.Add(grid);
            card.Controls.Add(top);
            return card;
        }

        private Control BuildKpiRow()
        {
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Theme.Bg, Margin = new Padding(0) };
            for (int c = 0; c < 4; c++) row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            row.Controls.Add(MakeKpi("Balance in Stock", Theme.Orange, out lblBalance, out capBalance), 0, 0);
            row.Controls.Add(MakeKpi("Stock In", Theme.Blue, out lblStockIn, out capStockIn), 1, 0);
            row.Controls.Add(MakeKpi("Stock Out", Theme.Green, out lblStockOut, out capStockOut), 2, 0);
            row.Controls.Add(MakeKpi("Total Sales", Theme.Pink, out lblSales, out capSales), 3, 0);
            return row;
        }

        private static Panel MakeKpi(string title, Color accent, out Label value, out Label caption)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Theme.CardTint, Margin = new Padding(6), Padding = new Padding(18, 10, 10, 8) };
            Theme.CardBorder(card, Theme.CardTintBorder, accent);

            var t = new Label { Text = title.ToUpperInvariant(), Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            caption = new Label { Dock = DockStyle.Bottom, Height = 20, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 8f), Text = "" };
            value = new Label { Dock = DockStyle.Fill, ForeColor = accent, Font = new Font("Segoe UI", 24, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Text = "0" };

            card.Controls.Add(value);
            card.Controls.Add(caption);
            card.Controls.Add(t);
            return card;
        }

        private void StyleMonthButtons()
        {
            for (int i = 0; i < 12; i++)
            {
                bool sel = _month == i + 1;
                Button b = _monthBtns[i];
                b.BackColor = sel ? Theme.Orange : Theme.SurfaceAlt;
                b.ForeColor = sel ? Theme.Ink : Theme.Text;
                b.FlatAppearance.MouseOverBackColor = sel ? Theme.Orange : Theme.Shift(Theme.SurfaceAlt, 14);
                b.FlatAppearance.MouseDownBackColor = sel ? Theme.Orange : Theme.Border;
                b.Font = new Font("Segoe UI", 9.5f, sel ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        // =====================================================================
        // DATA
        // =====================================================================
        private void LoadYears()
        {
            _loading = true;
            try
            {
                var years = new List<int>();
                using (var conn = new DatabaseHelper().GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT DISTINCT YEAR(TransactionDate) AS Y FROM StockTransactions WHERE TransactionDate IS NOT NULL ORDER BY Y DESC", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) years.Add(Convert.ToInt32(r.GetValue(0)));
                }
                int now = DateTime.Now.Year;
                if (!years.Contains(now)) years.Insert(0, now);
                years.Sort((a, b) => b.CompareTo(a));

                cmbYear.Items.Clear();
                foreach (int y in years) cmbYear.Items.Add(y);
                cmbYear.SelectedItem = now;
                if (cmbYear.SelectedIndex < 0 && cmbYear.Items.Count > 0) cmbYear.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load years: " + ex.Message, "Inventory Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (cmbYear.Items.Count == 0) { cmbYear.Items.Add(DateTime.Now.Year); cmbYear.SelectedIndex = 0; }
            }
            finally { _loading = false; }
        }

        private void RefreshAll()
        {
            if (cmbYear.SelectedItem == null) return;
            int year = (int)cmbYear.SelectedItem;
            string periodText = _month.HasValue
                ? _ci.DateTimeFormat.GetMonthName(_month.Value) + " " + year
                : "Whole year " + year;
            lblPeriod.Text = "Showing: " + periodText;

            string filter = " AND YEAR(t.TransactionDate) = @y" + (_month.HasValue ? " AND MONTH(t.TransactionDate) = @m" : "");

            try
            {
                using (var conn = new DatabaseHelper().GetConnection())
                {
                    conn.Open();

                    double Scalar(string sql)
                    {
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            AddParams(cmd, sql, year);
                            object o = cmd.ExecuteScalar();
                            return (o == null || o == DBNull.Value) ? 0 : Convert.ToDouble(o, _ci);
                        }
                    }

                    List<Slice> Rows(string sql)
                    {
                        var list = new List<Slice>();
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            AddParams(cmd, sql, year);
                            using (var r = cmd.ExecuteReader())
                                while (r.Read())
                                    list.Add(new Slice { Label = Convert.ToString(r.GetValue(0)) ?? "", Value = Convert.ToDouble(r.GetValue(1), _ci) });
                        }
                        return list;
                    }

                    double balance = Scalar("SELECT COALESCE(SUM(CurrentStock), 0) FROM Products");
                    double stockIn = Scalar("SELECT COALESCE(SUM(t.Quantity), 0) FROM StockTransactions t WHERE t.TransactionType = 'STOCK-IN'" + filter);
                    double stockOut = Scalar("SELECT COALESCE(SUM(t.Quantity), 0) FROM StockTransactions t WHERE t.TransactionType = 'STOCK-OUT'" + filter);
                    double sales = Scalar("SELECT COALESCE(SUM(t.FinalTotal), 0) FROM StockTransactions t WHERE t.TransactionType = 'STOCK-OUT'" + filter);

                    _stockByProduct = Rows("SELECT ProductName, CurrentStock FROM Products ORDER BY CurrentStock DESC, ProductName LIMIT 10");

                    _stockByCategory = Rows(
                        "SELECT c.CategoryName, COALESCE(SUM(p.CurrentStock), 0) AS V " +
                        "FROM Categories c LEFT JOIN Products p ON p.CategoryID = c.CategoryID " +
                        "GROUP BY c.CategoryID, c.CategoryName HAVING V > 0 ORDER BY V DESC");

                    _salesByCategory = Rows(
                        "SELECT c.CategoryName, COALESCE(SUM(t.FinalTotal), 0) AS V " +
                        "FROM StockTransactions t JOIN Products p ON t.ProductID = p.ProductID " +
                        "JOIN Categories c ON p.CategoryID = c.CategoryID " +
                        "WHERE t.TransactionType = 'STOCK-OUT'" + filter +
                        " GROUP BY c.CategoryID, c.CategoryName HAVING V > 0 ORDER BY V DESC");

                    _salesByProduct = Rows(
                        "SELECT p.ProductName, COALESCE(SUM(t.FinalTotal), 0) AS V " +
                        "FROM StockTransactions t JOIN Products p ON t.ProductID = p.ProductID " +
                        "WHERE t.TransactionType = 'STOCK-OUT'" + filter +
                        " GROUP BY p.ProductID, p.ProductName HAVING V > 0 ORDER BY V DESC LIMIT 10")
                        .OrderBy(s => s.Label, StringComparer.CurrentCultureIgnoreCase).ToList();

                    lblBalance.Text = balance.ToString("N0", _ci);
                    lblStockIn.Text = stockIn.ToString("N0", _ci);
                    lblStockOut.Text = stockOut.ToString("N0", _ci);
                    lblSales.Text = "₱" + sales.ToString("N2", _ci);
                    capBalance.Text = "Units in stock today";
                    capStockIn.Text = "Units received · " + periodText;
                    capStockOut.Text = "Units sold · " + periodText;
                    capSales.Text = "Stock-out revenue · " + periodText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load dashboard data: " + ex.Message, "Inventory Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            chBar.Invalidate(); chPie.Invalidate(); chDonut.Invalidate(); chLine.Invalidate();
        }

        private void AddParams(MySqlCommand cmd, string sql, int year)
        {
            if (sql.Contains("@y")) cmd.Parameters.AddWithValue("@y", year);
            if (sql.Contains("@m") && _month.HasValue) cmd.Parameters.AddWithValue("@m", _month.Value);
        }

        // =====================================================================
        // CHART DRAWING
        // =====================================================================
        private static double NiceMax(double v)
        {
            if (v <= 0) return 1;
            double exp = Math.Pow(10, Math.Floor(Math.Log10(v)));
            double f = v / exp;
            double nf = f <= 1 ? 1 : f <= 2 ? 2 : f <= 2.5 ? 2.5 : f <= 5 ? 5 : 10;
            return nf * exp;
        }

        private static string Axis(double v)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            return v >= 1000 ? (v / 1000).ToString("0.#", ci) + "k" : v.ToString("0.#", ci);
        }

        private static void DrawEmpty(Graphics g, Rectangle r)
        {
            using (var f = new Font("Segoe UI", 10f))
            using (var b = new SolidBrush(Theme.Muted))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("No data for this period", f, b, r, sf);
        }

        /// <summary>Draws horizontal grid lines + y labels and returns the plot rectangle.</summary>
        private static Rectangle DrawAxes(Graphics g, Rectangle r, double max)
        {
            const int left = 50, bottom = 30, top = 18, rightPad = 12;
            var plot = new Rectangle(r.Left + left, r.Top + top, r.Width - left - rightPad, r.Height - top - bottom);
            if (plot.Width < 20 || plot.Height < 20) return Rectangle.Empty;

            using (var grid = new Pen(Theme.Border))
            using (var muted = new SolidBrush(Theme.Muted))
            using (var small = new Font("Segoe UI", 8f))
            using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = plot.Bottom - plot.Height * i / 4f;
                    g.DrawLine(grid, plot.Left, y, plot.Right, y);
                    g.DrawString(Axis(max * i / 4), small, muted, new RectangleF(r.Left, y - 9, left - 8, 18), sf);
                }
            }
            return plot;
        }

        private static void DrawXLabel(Graphics g, Rectangle plot, float slot, int index, string text)
        {
            using (var small = new Font("Segoe UI", 8f))
            using (var muted = new SolidBrush(Theme.Muted))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
                g.DrawString(text, small, muted, new RectangleF(plot.Left + slot * index, plot.Bottom + 7, slot, 20), sf);
        }

        private static void DrawBars(Graphics g, Rectangle r, List<Slice> data, Color color)
        {
            if (data.Count == 0 || data.Max(d => d.Value) <= 0) { DrawEmpty(g, r); return; }
            double max = NiceMax(data.Max(d => d.Value));
            Rectangle plot = DrawAxes(g, r, max);
            if (plot.IsEmpty) return;

            float slot = plot.Width / (float)data.Count;
            float bw = Math.Min(54f, slot * 0.55f);

            using (var bar = new SolidBrush(color))
            using (var txt = new SolidBrush(Theme.Text))
            using (var bold = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
            {
                for (int i = 0; i < data.Count; i++)
                {
                    float h = (float)(data[i].Value / max * plot.Height);
                    float x = plot.Left + slot * i + (slot - bw) / 2f;
                    if (h >= 1) g.FillRectangle(bar, x, plot.Bottom - h, bw, h);
                    g.DrawString(data[i].Value.ToString("N0", CultureInfo.InvariantCulture), bold, txt,
                        new RectangleF(x - 20, plot.Bottom - h - 20, bw + 40, 18), sf);
                    DrawXLabel(g, plot, slot, i, data[i].Label);
                }
            }
        }

        private static void DrawLineChart(Graphics g, Rectangle r, List<Slice> data, Color color)
        {
            if (data.Count == 0 || data.Max(d => d.Value) <= 0) { DrawEmpty(g, r); return; }
            double max = NiceMax(data.Max(d => d.Value));
            Rectangle plot = DrawAxes(g, r, max);
            if (plot.IsEmpty) return;

            float slot = plot.Width / (float)data.Count;
            var pts = new PointF[data.Count];
            for (int i = 0; i < data.Count; i++)
                pts[i] = new PointF(plot.Left + slot * (i + 0.5f), plot.Bottom - (float)(data[i].Value / max * plot.Height));

            if (pts.Length >= 2)
            {
                var area = new List<PointF>(pts) { new PointF(pts[pts.Length - 1].X, plot.Bottom), new PointF(pts[0].X, plot.Bottom) };
                using (var fill = new SolidBrush(Color.FromArgb(45, color))) g.FillPolygon(fill, area.ToArray());
                using (var pen = new Pen(color, 2.5f) { LineJoin = LineJoin.Round }) g.DrawLines(pen, pts);
            }

            using (var mk = new Pen(color, 2f))
            using (var mkFill = new SolidBrush(Theme.Surface))
            using (var small = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var txt = new SolidBrush(Theme.Text))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
            {
                for (int i = 0; i < pts.Length; i++)
                {
                    g.FillEllipse(mkFill, pts[i].X - 4.5f, pts[i].Y - 4.5f, 9, 9);
                    g.DrawEllipse(mk, pts[i].X - 4.5f, pts[i].Y - 4.5f, 9, 9);
                    g.DrawString("₱" + data[i].Value.ToString("N0", CultureInfo.InvariantCulture), small, txt,
                        new RectangleF(pts[i].X - 40, pts[i].Y - 24, 80, 18), sf);
                    DrawXLabel(g, plot, slot, i, data[i].Label);
                }
            }
        }

        private static void DrawPie(Graphics g, Rectangle r, List<Slice> data, bool donut, string centerText, Func<double, string> fmt)
        {
            double total = data.Sum(x => x.Value);
            if (data.Count == 0 || total <= 0) { DrawEmpty(g, r); return; }

            int legendW = r.Width > 380 ? 200 : 0;
            var area = new Rectangle(r.Left, r.Top, r.Width - legendW, r.Height);
            int d = Math.Min(area.Width, area.Height) - 10;
            if (d < 50) return;
            var circle = new Rectangle(area.Left + (area.Width - d) / 2, area.Top + (area.Height - d) / 2, d, d);

            float start = -90f;
            using (var sep = new Pen(Theme.Surface, 2f))
            using (var lblFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i < data.Count; i++)
                {
                    float sweep = (float)(data[i].Value / total * 360.0);
                    Color c = Palette[i % Palette.Length];
                    using (var br = new SolidBrush(c)) g.FillPie(br, circle, start, sweep);
                    if (data.Count > 1) g.DrawPie(sep, circle, start, sweep);

                    if (sweep >= 16f)
                    {
                        double mid = (start + sweep / 2f) * Math.PI / 180.0;
                        double rad = d / 2.0 * (donut ? 0.78 : 0.65);
                        float cx = circle.Left + d / 2f + (float)(Math.Cos(mid) * rad);
                        float cy = circle.Top + d / 2f + (float)(Math.Sin(mid) * rad);
                        using (var tb = new SolidBrush(Theme.Lum(c) > 140 ? Theme.Ink : Color.White))
                            g.DrawString((data[i].Value / total).ToString("P0", CultureInfo.InvariantCulture), lblFont, tb, new RectangleF(cx - 26, cy - 9, 52, 18), sf);
                    }
                    start += sweep;
                }

                if (donut)
                {
                    int hole = (int)(d * 0.5);
                    var hr = new Rectangle(circle.Left + (d - hole) / 2, circle.Top + (d - hole) / 2, hole, hole);
                    using (var hb = new SolidBrush(Theme.Surface)) g.FillEllipse(hb, hr);
                    using (var cf = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (var cb = new SolidBrush(Theme.Text))
                    using (var mf = new Font("Segoe UI", 7.5f))
                    using (var mb = new SolidBrush(Theme.Muted))
                    {
                        g.DrawString("Total", mf, mb, new RectangleF(hr.Left, hr.Top + hole / 2f - 18, hr.Width, 16), sf);
                        g.DrawString(centerText, cf, cb, new RectangleF(hr.Left, hr.Top + hole / 2f - 2, hr.Width, 20), sf);
                    }
                }
            }

            if (legendW > 0)
            {
                int shown = Math.Min(data.Count, 8);
                int rowH = 24;
                int y = r.Top + Math.Max(0, (r.Height - shown * rowH) / 2);
                int x = area.Right + 8;
                using (var nameFont = new Font("Segoe UI", 8.5f))
                using (var tb = new SolidBrush(Theme.Text))
                using (var vb = new SolidBrush(Theme.Muted))
                using (var nameSf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, LineAlignment = StringAlignment.Center })
                using (var valSf = new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap, LineAlignment = StringAlignment.Center })
                {
                    for (int i = 0; i < shown; i++)
                    {
                        using (var sw = new SolidBrush(Palette[i % Palette.Length])) g.FillRectangle(sw, x, y + i * rowH + 7, 10, 10);
                        g.DrawString(data[i].Label, nameFont, tb, new RectangleF(x + 16, y + i * rowH, 92, rowH), nameSf);
                        g.DrawString(fmt(data[i].Value), nameFont, vb, new RectangleF(x + 108, y + i * rowH, legendW - 118, rowH), valSf);
                    }
                }
            }
        }
    }
}
