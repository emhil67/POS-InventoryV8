using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public partial class Form1 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private readonly SemanticSearchService _aiSearch = new SemanticSearchService();
        private readonly Color navy = Theme.Sidebar;
        private readonly Color green = Theme.Orange; // primary action colour
        private readonly Color canvas = Theme.Bg;
        private Panel sidebar, content, topbar, chartPanel;
        private FlowLayoutPanel navPanel, cardsPanel;
        private Label lblPageTitle, lblGreeting, lblTodaySales, lblProducts, lblUnits, lblInventoryValue, lblLowStock, lblExpiry;
        private TextBox txtAiQuery;
        private Button btnAiSearch;
        private DataGridView dgvLowStock, dgvForecast, dgvAiResults;
        private Panel aiResultPanel;
        private bool dashboardLoaded;
        private DataTable weeklySales = new DataTable();

        private Form2 directoryForm;
        private Form3 analyticsForm;
        private InventoryDashboardForm inventoryDashForm;
        private Form4 posForm;
        private Form5 productForm;
        private Form6 restockForm;

        public Form1()
        {
            Text = "Smart-Retail | AI Inventory Management";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1120, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = canvas;
            Font = new Font("Segoe UI", 9.5f);
            BuildWorkspace();
            Theme.Apply(this);
            Shown += (s, e) => RefreshDashboard();
        }

        private void BuildWorkspace()
        {
            sidebar = new Panel { Dock = DockStyle.Left, Width = 238, BackColor = navy, Padding = new Padding(14, 18, 14, 14) };
            var brand = new Panel { Dock = DockStyle.Top, Height = 92 };
            var brandIcon = new Label { Text = "S", Location = new Point(2, 2), Size = new Size(42, 42), BackColor = Theme.Orange, ForeColor = Theme.Ink, Font = new Font("Segoe UI", 22, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var brandName = new Label { Text = "Smart-Retail", Location = new Point(53, 2), Size = new Size(145, 30), ForeColor = Theme.Text, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
            var brandSub = new Label { Text = "AI INVENTORY SUITE", Location = new Point(55, 34), Size = new Size(145, 20), ForeColor = Theme.Muted, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };
            brand.Controls.AddRange(new Control[] { brandIcon, brandName, brandSub });
            navPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = navy, Padding = new Padding(0, 5, 0, 0) };
            AddNav("⌂", "Dashboard", RefreshDashboard, true);
            AddNav("▦", "Inventory Dashboard", () => OpenChild(inventoryDashForm, () => new InventoryDashboardForm(), f => inventoryDashForm = f));
            AddNav("▣", "Product Catalog", () => OpenChild(directoryForm, () => new Form2(), f => directoryForm = f));
            AddNav("＋", "Add Product", () => OpenChild(productForm, () => new Form5(), f => productForm = f));
            AddNav("⇧", "Bulk Restock", () => OpenChild(restockForm, () => new Form6(), f => restockForm = f));
            AddNav("▤", "Sales & Analytics", () => OpenChild(analyticsForm, () => new Form3(), f => analyticsForm = f));
            AddNav("🛒", "Point of Sale", () => OpenChild(posForm, () => new Form4(), f => posForm = f));
            AddNav("✧", "AI Product Search", FocusAiSearch);
            if (LoginForm.LoggedInRole == "Admin") AddNav("♙", "User Management", OpenUsers); // Admin only
            var session = new Panel { Dock = DockStyle.Bottom, Height = 78, Padding = new Padding(4, 10, 4, 0) };
            var user = new Label { Dock = DockStyle.Top, Height = 24, Text = "●  " + LoginForm.LoggedInUser, ForeColor = Theme.Text, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var role = new Label { Dock = DockStyle.Top, Height = 22, Text = LoginForm.LoggedInRole + " access", ForeColor = Theme.Muted };
            var logout = new Button { Dock = DockStyle.Bottom, Height = 30, Text = "↪  Sign out", FlatStyle = FlatStyle.Flat, ForeColor = Theme.Text, BackColor = Theme.SurfaceAlt, TextAlign = ContentAlignment.MiddleLeft, Tag = "keep" };
            logout.FlatAppearance.BorderSize = 1; logout.FlatAppearance.BorderColor = Theme.Border; logout.FlatAppearance.MouseOverBackColor = Theme.Red; logout.Click += Logout;
            session.Controls.Add(logout); session.Controls.Add(role); session.Controls.Add(user);
            sidebar.Controls.Add(navPanel); sidebar.Controls.Add(session); sidebar.Controls.Add(brand);

            content = new Panel { Dock = DockStyle.Fill, BackColor = canvas, Padding = new Padding(24, 0, 24, 18), AutoScroll = true };
            topbar = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Theme.Sidebar, Padding = new Padding(24, 0, 24, 0) };
            lblPageTitle = new Label { Text = "Dashboard", Dock = DockStyle.Left, Width = 300, ForeColor = Theme.Text, Font = new Font("Segoe UI", 17, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            var topRight = new Label { Text = DateTime.Now.ToString("dddd, dd MMMM yyyy") + "     |     " + LoginForm.LoggedInUser, Dock = DockStyle.Right, Width = 330, ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleRight };
            topbar.Controls.Add(topRight); topbar.Controls.Add(lblPageTitle);

            var page = new Panel { Dock = DockStyle.Fill, BackColor = canvas, AutoScroll = true, Padding = new Padding(0, 18, 0, 0) };
            lblGreeting = new Label { Text = "Good day! Here's your store at a glance.", Location = new Point(0, 8), Size = new Size(720, 30), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Theme.Text };
            var refresh = MakeButton("⟳  Refresh data", green, Theme.Ink, 132, 34); refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right; refresh.Location = new Point(Width - 430, 7); refresh.Click += (s, e) => RefreshDashboard();
            cardsPanel = new FlowLayoutPanel { Location = new Point(0, 52), Height = 112, Width = 1500, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, WrapContents = false, BackColor = canvas };
            lblTodaySales = AddMetric("TODAY'S SALES", "₱ 0.00", "↗", Theme.Green);
            lblProducts = AddMetric("TOTAL PRODUCTS", "0", "▦", Theme.Blue);
            lblUnits = AddMetric("UNITS IN STOCK", "0", "◫", Theme.Pink);
            lblInventoryValue = AddMetric("INVENTORY VALUE", "₱ 0.00", "₱", Theme.Orange);

            chartPanel = new Panel { Location = new Point(0, 180), Size = new Size(650, 285), BackColor = Theme.Surface, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            chartPanel.Paint += DrawSalesChart;
            Theme.CardBorder(chartPanel, Theme.Border);
            var chartTitle = new Label { Text = "Sales overview  ·  Last 7 days", Location = new Point(18, 14), Size = new Size(350, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Theme.Text };
            chartPanel.Controls.Add(chartTitle);

            var alertPanel = new Panel { Location = new Point(666, 180), Size = new Size(430, 285), BackColor = Theme.Surface, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            Theme.CardBorder(alertPanel, Theme.Border);
            var alertTitle = new Label { Text = "Inventory alerts", Location = new Point(18, 14), Size = new Size(300, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Theme.Text };
            lblLowStock = new Label { Text = "Low stock items: —", Location = new Point(20, 65), Size = new Size(360, 38), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Theme.Red };
            lblExpiry = new Label { Text = "Expiring products: —", Location = new Point(20, 111), Size = new Size(360, 38), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Theme.Orange };
            var alertHint = new Label { Text = "Review stock levels and upcoming expiry dates to avoid lost sales and waste.", Location = new Point(20, 172), Size = new Size(380, 64), ForeColor = Theme.Muted };
            alertPanel.Controls.AddRange(new Control[] { alertTitle, lblLowStock, lblExpiry, alertHint });

            var lowTitle = new Label { Text = "Low stock items", Location = new Point(0, 484), Size = new Size(350, 26), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Theme.Text };
            dgvLowStock = MakeGrid(); dgvLowStock.Location = new Point(0, 516); dgvLowStock.Size = new Size(650, 205); dgvLowStock.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            var aiTitle = new Label { Text = "AI stock forecast", Location = new Point(666, 484), Size = new Size(350, 26), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Theme.Text };
            dgvForecast = MakeGrid(); dgvForecast.Location = new Point(666, 516); dgvForecast.Size = new Size(430, 205); dgvForecast.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var aiBox = new Panel { Location = new Point(0, 744), Size = new Size(1096, 245), BackColor = Theme.Surface, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Padding = new Padding(18) };
            Theme.CardBorder(aiBox, Theme.Border);
            var aiHeader = new Label { Text = "✧  AI Product Discovery", Location = new Point(18, 12), Size = new Size(400, 26), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Theme.Text };
            var aiDesc = new Label { Text = "Search by meaning, use case, or product description — not just exact product names.", Location = new Point(18, 42), Size = new Size(760, 22), ForeColor = Theme.Muted };
            txtAiQuery = new TextBox { Location = new Point(18, 76), Size = new Size(780, 32), Font = new Font("Segoe UI", 11) };
            txtAiQuery.PlaceholderText = "e.g. something for cleaning glass or a product for headache";
            btnAiSearch = MakeButton("Search with AI", green, Theme.Ink, 145, 34); btnAiSearch.Location = new Point(812, 75); btnAiSearch.Click += RunAiSearch;
            dgvAiResults = MakeGrid(); dgvAiResults.Location = new Point(18, 119); dgvAiResults.Size = new Size(1048, 108); dgvAiResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            aiBox.Controls.AddRange(new Control[] { aiHeader, aiDesc, txtAiQuery, btnAiSearch, dgvAiResults });

            page.Controls.AddRange(new Control[] { lblGreeting, refresh, cardsPanel, chartPanel, alertPanel, lowTitle, dgvLowStock, aiTitle, dgvForecast, aiBox });
            content.Controls.Add(page);
            Controls.Add(content); Controls.Add(topbar); Controls.Add(sidebar);
            Resize += (s, e) => { if (refresh.Parent != null) refresh.Left = Math.Max(0, page.ClientSize.Width - refresh.Width); ResizeDashboardPanels(page); };
            ResizeDashboardPanels(page);
        }

        private void AddNav(string icon, string title, Action action, bool active = false)
        {
            var b = new Button { Width = 204, Height = 42, Text = "  " + icon + "    " + title, TextAlign = ContentAlignment.MiddleLeft, FlatStyle = FlatStyle.Flat, BackColor = active ? Theme.Orange : navy, ForeColor = active ? Theme.Ink : Color.FromArgb(190, 201, 214), Font = new Font("Segoe UI", 9.5f, active ? FontStyle.Bold : FontStyle.Regular), Margin = new Padding(0, 3, 0, 3), Cursor = Cursors.Hand, Tag = "nav" };
            b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = active ? Theme.Orange : Theme.SurfaceAlt; b.FlatAppearance.MouseDownBackColor = active ? Theme.Orange : Theme.Border;
            b.Click += (s, e) => action(); navPanel.Controls.Add(b);
        }

        private Label AddMetric(string title, string value, string icon, Color accent)
        {
            var card = new Panel { Width = 250, Height = 96, BackColor = Theme.CardTint, Margin = new Padding(0, 0, 14, 0), Padding = new Padding(14) };
            Theme.CardBorder(card, Theme.CardTintBorder);
            var stripe = new Panel { Dock = DockStyle.Left, Width = 0, BackColor = accent };
            var t = new Label { Text = title, Location = new Point(16, 12), Size = new Size(205, 20), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Theme.Muted };
            var v = new Label { Text = value, Location = new Point(16, 38), Size = new Size(215, 40), Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = Theme.Text };
            var i = new Label { Text = icon, Dock = DockStyle.Right, Width = 38, ForeColor = accent, Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            card.Controls.AddRange(new Control[] { stripe, t, v, i }); cardsPanel.Controls.Add(card); return v;
        }

        private static Button MakeButton(string text, Color back, Color fore, int width, int height) => new Button { Text = text, Width = width, Height = height, BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        private static DataGridView MakeGrid() { var g = new DataGridView { ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, ColumnHeadersHeight = 34, RowTemplate = { Height = 30 }, Font = new Font("Segoe UI", 8.5f) }; Theme.StyleGrid(g); g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold); g.DefaultCellStyle.Padding = new Padding(4); return g; }

        private void ResizeDashboardPanels(Panel page)
        {
            if (page == null || chartPanel == null) return;
            int w = Math.Max(700, page.ClientSize.Width);
            int gap = 16; int leftW = Math.Max(400, (w - gap) * 60 / 100); int rightW = Math.Max(280, w - leftW - gap);
            chartPanel.Width = leftW; var alert = chartPanel.Parent.Controls.OfType<Panel>().FirstOrDefault(p => p != chartPanel && p.Location.Y == 180);
            if (alert != null) { alert.Left = leftW + gap; alert.Width = rightW; }
            dgvLowStock.Width = leftW; dgvForecast.Left = leftW + gap; dgvForecast.Width = rightW;
            foreach (Control c in page.Controls) if (c is Panel p && p.Height == 245 && p.Location.Y == 744) p.Width = w;
            cardsPanel.Width = w;
        }

        private void RefreshDashboard()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(*) AS ProductCount, COALESCE(SUM(CurrentStock),0) AS Units, COALESCE(SUM(CurrentStock * Price),0) AS StockValue FROM Products", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) { lblProducts.Text = Convert.ToInt32(r["ProductCount"]).ToString("N0"); lblUnits.Text = Convert.ToInt64(r["Units"]).ToString("N0"); lblInventoryValue.Text = "₱ " + Convert.ToDecimal(r["StockValue"]).ToString("N2"); }
                    }
                }
                lblTodaySales.Text = "₱ " + _db.GetCalculatedRevenueByRange(0).ToString("N2");
                var low = _db.GetLowStockProducts(); dgvLowStock.DataSource = low; lblLowStock.Text = "⚠  Low stock items: " + low.Rows.Count;
                var expiry = _db.GetExpiringProducts(); lblExpiry.Text = "◷  Expiring products: " + expiry.Rows.Count;
                var forecast = _db.GetAIPredictiveStockForecast(); dgvForecast.DataSource = forecast;
                if (dgvForecast.Columns.Contains("ProductID")) dgvForecast.Columns["ProductID"].Visible = false;
                if (dgvForecast.Columns.Contains("EstimatedDaysLeft")) dgvForecast.Columns["EstimatedDaysLeft"].HeaderText = "Days left";
                if (dgvForecast.Columns.Contains("RecommendedRestockQty")) dgvForecast.Columns["RecommendedRestockQty"].HeaderText = "Suggested qty";
                if (dgvForecast.Columns.Contains("DailyVelocity")) dgvForecast.Columns["DailyVelocity"].HeaderText = "Units/day";
                try { weeklySales = _db.GetWeeklySalesChartData(); } catch { weeklySales = new DataTable(); }
                chartPanel.Invalidate(); dashboardLoaded = true;
            }
            catch (Exception ex) { MessageBox.Show("Unable to load dashboard data. Check that MySQL is running and the ai_inventory_db database is available.\n\n" + ex.Message, "Dashboard data", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { Cursor = Cursors.Default; }
        }

        private void DrawSalesChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int left = 48, top = 62, bottom = chartPanel.Height - 34, right = chartPanel.Width - 22;
            using var pen = new Pen(Theme.Border); using var textBrush = new SolidBrush(Theme.Muted); using var barBrush = new SolidBrush(Theme.Blue);
            for (int i = 0; i < 4; i++) { int y = top + (bottom - top) * i / 3; g.DrawLine(pen, left, y, right, y); }
            DataTable data = weeklySales;
            if (data == null || data.Rows.Count == 0) { g.DrawString("No sales activity for the selected period", Font, textBrush, left + 10, top + 55); return; }
            decimal max = data.AsEnumerable().Select(r => Convert.ToDecimal(r["TotalRevenue"])).DefaultIfEmpty(0).Max(); if (max <= 0) max = 1;
            int count = data.Rows.Count, slot = Math.Max(24, (right - left) / count), barW = Math.Max(12, slot / 2);
            for (int i = 0; i < count; i++)
            {
                decimal value = Convert.ToDecimal(data.Rows[i]["TotalRevenue"]); int h = (int)((bottom - top - 8) * value / max); int x = left + i * slot + (slot - barW) / 2;
                g.FillRectangle(barBrush, new Rectangle(x, bottom - h, barW, Math.Max(3, h)));
                g.DrawString(Convert.ToString(data.Rows[i]["SalesPeriod"]), new Font("Segoe UI", 8), textBrush, x - 5, bottom + 7);
            }
        }

        private void RunAiSearch(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAiQuery.Text)) { MessageBox.Show("Enter a product description or use case first.", "AI Product Search", MessageBoxButtons.OK, MessageBoxIcon.Information); txtAiQuery.Focus(); return; }
            try { Cursor = Cursors.WaitCursor; dgvAiResults.DataSource = _aiSearch.SearchProducts(txtAiQuery.Text.Trim(), 8); if (dgvAiResults.Columns.Contains("ProductEmbedding")) dgvAiResults.Columns["ProductEmbedding"].Visible = false; }
            catch (Exception ex) { MessageBox.Show("AI search could not be completed: " + ex.Message, "AI Search", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { Cursor = Cursors.Default; }
        }
        private void FocusAiSearch() { txtAiQuery.Focus(); }
        private void OpenUsers() { if (LoginForm.LoggedInRole != "Admin") { MessageBox.Show("Only administrators can manage user accounts.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } using var f = new Form7(); f.ShowDialog(this); }
        private void OpenChild<T>(T current, Func<T> factory, Action<T> assign) where T : Form
        {
            if (current == null || current.IsDisposed)
            {
                T created = factory(); assign(created);
                created.FormClosed += (s, e) => { assign(null); if (dashboardLoaded) RefreshDashboard(); };
                created.Show(this);
            }
            else { if (current.WindowState == FormWindowState.Minimized) current.WindowState = FormWindowState.Normal; current.BringToFront(); }
        }
        private void Logout(object sender, EventArgs e)
        {
            if (MessageBox.Show("Sign out of the current account?", "Confirm sign out", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            LoginForm.LogoutRequested = true; // Program.cs shows the login again after this window closes
            Close();
        }

        // Kept for the legacy designer-generated event hookup. The redesigned UI uses RunAiSearch.
        private void btnSearch_Click(object sender, EventArgs e) { RunAiSearch(sender, e); }
    }
}
