﻿﻿using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AI_Inventory_Final
{
    public class Form3 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        private Label lblHeader, lblRevenueTitle, lblRevenueValue;
        private Panel pnlRevenueCard;
        private TabControl tcAnalyticsTabsCustom;
        private TabPage tpToday, tpWeekly, tpMonthly, tpAudit, tpSalesTrend;
        private Panel pnlSalesTrendChart;
        private DataTable _salesTrendData = new DataTable();
        private DataGridView dgvTodayLog, dgvWeeklyLog, dgvMonthlyLog, dgvAuditTrailCustom;
        private Button btnClearLogsCustom, btnDeleteSingleLogCustom;

        public Form3()
        {
            this.Size = new Size(1120, 780);
            this.MinimumSize = new Size(950, 600);
            this.Text = "Store Retail Analytics Dashboard";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 242, 245);

            dgvTodayLog = new DataGridView(); dgvWeeklyLog = new DataGridView();
            dgvMonthlyLog = new DataGridView(); dgvAuditTrailCustom = new DataGridView();

            InitializeTabbedAnalyticsUI();
            Theme.Apply(this);
        }

        private void InitializeTabbedAnalyticsUI()
        {
            lblHeader = new Label { Text = "📈 PERIODIC RETAIL SALES ANALYTICS & ACCOUNTABILITY DASHBOARD", Location = new Point(20, 15), Size = new Size(650, 30), Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(21, 67, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left };

            btnClearLogsCustom = new Button { Text = "Wipe Selected Range Logs", Location = new Point(885, 12), Size = new Size(200, 35), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnClearLogsCustom.Click += new EventHandler(btnClearLogs_Click);

            pnlRevenueCard = new Panel { Location = new Point(20, 60), Size = new Size(380, 95), BackColor = Theme.CardTint, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            lblRevenueTitle = new Label { Text = "REAL-TIME STATEMENT BALANCE REVENUE", Location = new Point(18, 15), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Theme.Muted };
            lblRevenueValue = new Label { Text = "₱ 0.00", Location = new Point(18, 38), Size = new Size(350, 45), Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Theme.Text };
            pnlRevenueCard.Controls.Add(lblRevenueTitle); pnlRevenueCard.Controls.Add(lblRevenueValue);
            pnlRevenueCard.Paint += (s, e) => { using (var pen = new Pen(Theme.CardTintBorder)) e.Graphics.DrawRectangle(pen, 0, 0, pnlRevenueCard.Width - 1, pnlRevenueCard.Height - 1); using (var stripe = new SolidBrush(_revenueAccent)) e.Graphics.FillRectangle(stripe, 0, 0, 5, pnlRevenueCard.Height); };

            tcAnalyticsTabsCustom = new TabControl { Location = new Point(20, 175), Size = new Size(1065, 470), Font = new Font("Segoe UI", 10f, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            tcAnalyticsTabsCustom.SelectedIndexChanged += new EventHandler(tcAnalyticsTabsCustom_SelectedIndexChanged);

            tpToday = new TabPage { Text = "📋 Today's Sales Logs ", BackColor = Color.White };
            ConfigureSalesGrid(dgvTodayLog); tpToday.Controls.Add(dgvTodayLog);

            tpWeekly = new TabPage { Text = "📅 Weekly Sales Summary ", BackColor = Color.White };
            ConfigureSalesGrid(dgvWeeklyLog); tpWeekly.Controls.Add(dgvWeeklyLog);

            tpMonthly = new TabPage { Text = "📊 Monthly Sales Overview ", BackColor = Color.White };
            ConfigureSalesGrid(dgvMonthlyLog); tpMonthly.Controls.Add(dgvMonthlyLog);

            // Sales trend graph tab: uses actual STOCK-OUT transactions from MySQL.
            tpSalesTrend = new TabPage { Text = "📈 Sales Trend Graph", BackColor = Color.White };
            pnlSalesTrendChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
            pnlSalesTrendChart.Paint += new PaintEventHandler(PnlSalesTrendChart_Paint);
            tpSalesTrend.Controls.Add(pnlSalesTrendChart);

            tpAudit = new TabPage { Text = "🔐 System Security Audit Trails ", BackColor = Color.GhostWhite };
            dgvAuditTrailCustom.Dock = DockStyle.Fill; dgvAuditTrailCustom.BackgroundColor = Color.GhostWhite; dgvAuditTrailCustom.BorderStyle = BorderStyle.None; dgvAuditTrailCustom.ReadOnly = true; dgvAuditTrailCustom.AllowUserToAddRows = false; dgvAuditTrailCustom.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvAuditTrailCustom.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvAuditTrailCustom.RowTemplate.Height = 32; dgvAuditTrailCustom.Font = new Font("Segoe UI", 9.5f);
            tpAudit.Controls.Add(dgvAuditTrailCustom);

            tcAnalyticsTabsCustom.TabPages.Add(tpToday); tcAnalyticsTabsCustom.TabPages.Add(tpWeekly); tcAnalyticsTabsCustom.TabPages.Add(tpMonthly); tcAnalyticsTabsCustom.TabPages.Add(tpSalesTrend); tcAnalyticsTabsCustom.TabPages.Add(tpAudit);

            btnDeleteSingleLogCustom = new Button { Text = "🛑 Delete Selected Sales Record Entry", Location = new Point(20, 665), Size = new Size(320, 42), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnDeleteSingleLogCustom.Click += new EventHandler(btnDeleteSingleLog_Click);

            this.Controls.Add(lblHeader); this.Controls.Add(btnClearLogsCustom); this.Controls.Add(pnlRevenueCard);
            this.Controls.Add(tcAnalyticsTabsCustom); this.Controls.Add(btnDeleteSingleLogCustom);

            this.Load += new EventHandler(Form3_Load);
        }

        private void PnlSalesTrendChart_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Surface);

            int left = 70, right = 25, top = 35, bottom = 65;
            int chartWidth = Math.Max(100, pnlSalesTrendChart.ClientSize.Width - left - right);
            int chartHeight = Math.Max(100, pnlSalesTrendChart.ClientSize.Height - top - bottom);
            int baseY = top + chartHeight;

            using (var titleFont = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (var axisFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (var titleBrush = new SolidBrush(Theme.Text))
            using (var textBrush = new SolidBrush(Theme.Muted))
            using (var gridPen = new Pen(Theme.Border, 1))
            using (var axisPen = new Pen(Theme.Shift(Theme.Border, 30), 1))
            using (var barBrush = new SolidBrush(Theme.Blue))
            {
                g.DrawString("Daily Sales Revenue — Last 30 Days", titleFont, titleBrush, left, 8);
                if (_salesTrendData == null || _salesTrendData.Rows.Count == 0)
                {
                    g.DrawString("No sales transactions found for the last 30 days.", axisFont, textBrush, left, top + 25);
                    return;
                }

                var values = new List<decimal>();
                var labels = new List<string>();
                foreach (DataRow row in _salesTrendData.Rows)
                {
                    labels.Add(Convert.ToString(row["SalesPeriod"]) ?? "");
                    values.Add(row["TotalRevenue"] == DBNull.Value ? 0m : Convert.ToDecimal(row["TotalRevenue"]));
                }
                decimal maxValue = values.Count > 0 ? values.Max() : 0m;
                if (maxValue <= 0m) maxValue = 1m;
                for (int i = 0; i <= 4; i++)
                {
                    int y = top + (chartHeight * i / 4);
                    decimal tick = maxValue * (4 - i) / 4m;
                    g.DrawLine(gridPen, left, y, left + chartWidth, y);
                    g.DrawString(tick.ToString("N0", CultureInfo.InvariantCulture), axisFont, textBrush, 5, y - 7);
                }
                g.DrawLine(axisPen, left, top, left, baseY);
                g.DrawLine(axisPen, left, baseY, left + chartWidth, baseY);

                float slot = (float)chartWidth / values.Count;
                float barWidth = Math.Max(3f, Math.Min(24f, slot * 0.62f));
                for (int i = 0; i < values.Count; i++)
                {
                    float height = (float)(values[i] / maxValue) * chartHeight;
                    float x = left + i * slot + (slot - barWidth) / 2f;
                    float y = baseY - height;
                    if (values[i] > 0) g.FillRectangle(barBrush, x, y, barWidth, Math.Max(2f, height));
                    if (i % Math.Max(1, values.Count / 8) == 0 || i == values.Count - 1)
                    {
                        string label = labels[i];
                        SizeF labelSize = g.MeasureString(label, axisFont);
                        g.DrawString(label, axisFont, textBrush, x + barWidth / 2f - labelSize.Width / 2f, baseY + 8);
                    }
                }
            }
        }

        private Color _revenueAccent = Theme.Green;
        private void SetRevenueAccent(Color accent) { _revenueAccent = accent; pnlRevenueCard.Invalidate(); }

        private void ConfigureSalesGrid(DataGridView dgv)
        {
            if (dgv == null) return;
            dgv.Dock = DockStyle.Fill; dgv.BackgroundColor = Color.White; dgv.BorderStyle = BorderStyle.None; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false; dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.MultiSelect = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgv.RowTemplate.Height = 32; dgv.Font = new Font("Segoe UI", 9.5f);
        }

        private void Form3_Load(object sender, EventArgs e) { RefreshAnalyticsData(); }

        private void tcAnalyticsTabsCustom_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int index = tcAnalyticsTabsCustom.SelectedIndex;
                bool isAdmin = LoginForm.LoggedInRole == "Admin";

                if (index == 4)
                {
                    lblRevenueTitle.Text = "AUDIT MODE ACTIVE"; lblRevenueValue.Text = "SECURITY TRAILS";
                    SetRevenueAccent(Theme.Pink); btnDeleteSingleLogCustom.Enabled = false;
                    btnClearLogsCustom.Enabled = isAdmin; btnClearLogsCustom.Text = "Wipe Entire Security Logs"; btnClearLogsCustom.BackColor = Theme.Red;
                }
                else if (index == 3)
                {
                    SetRevenueAccent(Theme.Blue);
                    lblRevenueTitle.Text = "SALES TREND GRAPH · LAST 30 DAYS";
                    decimal trendTotal = 0m;
                    if (_salesTrendData != null && _salesTrendData.Columns.Contains("TotalRevenue"))
                        foreach (DataRow row in _salesTrendData.Rows)
                            if (row["TotalRevenue"] != DBNull.Value) trendTotal += Convert.ToDecimal(row["TotalRevenue"]);
                    lblRevenueValue.Text = string.Format("₱ {0:N2}", trendTotal);
                    btnDeleteSingleLogCustom.Enabled = false; // Trend view is read-only
                    btnClearLogsCustom.Enabled = false;
                }
                else
                {
                    SetRevenueAccent(Theme.Green); btnDeleteSingleLogCustom.Enabled = isAdmin; btnClearLogsCustom.Enabled = isAdmin;
                    btnClearLogsCustom.Text = "Wipe Selected Range Logs"; btnClearLogsCustom.BackColor = Theme.SurfaceAlt;

                    string rangeLabel = index == 0 ? "TODAY'S TOTAL NET REVENUE FLOW" : (index == 1 ? "PAST 7 DAYS SUMMARY VELOCITY" : "PAST 30 DAYS STATEMENT ACCUMULATION");
                    lblRevenueTitle.Text = rangeLabel;
                    lblRevenueValue.Text = string.Format("₱ {0:N2}", _db.GetCalculatedRevenueByRange(index));
                }
            }
            catch { }
        }

        private void RefreshAnalyticsData()
        {
            try
            {
                if (dgvTodayLog != null) dgvTodayLog.DataSource = _db.GetTodaySalesLog();
                if (dgvWeeklyLog != null) dgvWeeklyLog.DataSource = _db.GetWeeklySalesLog();
                if (dgvMonthlyLog != null) dgvMonthlyLog.DataSource = _db.GetMonthlySalesLog();
                _salesTrendData = _db.GetMonthlySalesChartData();
                if (pnlSalesTrendChart != null) pnlSalesTrendChart.Invalidate();

                FixGridHeaders(dgvTodayLog); FixGridHeaders(dgvWeeklyLog); FixGridHeaders(dgvMonthlyLog);

                DataTable logsTable = new DataTable();
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT LogID, TransactionDate, Username, Role, ActionType, Description, RecordID FROM activity_logs ORDER BY LogID DESC";
                    using (var cmd = new MySqlCommand(query, conn)) { using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(logsTable); } }
                }
                if (dgvAuditTrailCustom != null) dgvAuditTrailCustom.DataSource = logsTable;

                tcAnalyticsTabsCustom_SelectedIndexChanged(this, EventArgs.Empty);
            }
            catch { }
        }

        private void FixGridHeaders(DataGridView dgv)
        {
            if (dgv != null && dgv.Columns != null && dgv.Columns.Count > 0)
            {
                if (dgv.Columns.Contains("TransactionID")) dgv.Columns["TransactionID"].HeaderText = "TXN ID";
                if (dgv.Columns.Contains("OrderGroupId")) dgv.Columns["OrderGroupId"].HeaderText = "Order Slip ID";
                if (dgv.Columns.Contains("ProductName")) dgv.Columns["ProductName"].HeaderText = "Product Name";
                if (dgv.Columns.Contains("TransactionDate")) dgv.Columns["TransactionDate"].HeaderText = "Date Processed";
            }
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            if (LoginForm.LoggedInRole != "Admin") { MessageBox.Show("Access Denied! Administrators only.", "Privilege Warning", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }
            int activeTabIndex = tcAnalyticsTabsCustom.SelectedIndex;

            if (activeTabIndex == 4)
            {
                if (MessageBox.Show("WARNING: Permanently erase the entire Security Audit Trail?", "Confirm Audit Wipe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        using (var conn = _db.GetConnection())
                        {
                            conn.Open();
                            string query = "TRUNCATE TABLE activity_logs";
                            using (var cmd = new MySqlCommand(query, conn)) { cmd.ExecuteNonQuery(); }
                        }
                        _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "WIPE-AUDIT", "Master administrator cleared the security audit log.", "N/A");
                        MessageBox.Show("Security Audit Trail logs reset!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshAnalyticsData();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
            else
            {
                if (MessageBox.Show("Completely wipe selected operational range history?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    _db.ClearTodayTransactions();
                    _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "WIPE-DATA", "Administrator cleared operational sales logs.", "N/A");
                    RefreshAnalyticsData();
                }
            }
        }
        // FIXED: Allows the Admin profile to delete single rows from either the sales transactions or security audit logs tables dynamically
        // FIXED: Allows the Admin profile to delete single rows from either the sales transactions or security audit logs tables dynamically
        private void btnDeleteSingleLog_Click(object sender, EventArgs e)
        {
            if (LoginForm.LoggedInRole != "Admin")
            {
                MessageBox.Show("Access Denied! Administrators only.", "Privilege Warning", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            int activeIndex = tcAnalyticsTabsCustom.SelectedIndex;
            if (activeIndex == 3) return; // Sales trend chart has no selectable transaction row.

            // =========================================================================
            // CONDITION A: IF THE ADMIN IS TRYING TO DELETE A SECURITY AUDIT LOG ROW
            // =========================================================================
            if (activeIndex == 4)
            {
                if (dgvAuditTrailCustom == null || dgvAuditTrailCustom.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please highlight an audit log row first.", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    var row = dgvAuditTrailCustom.SelectedRows[0];
                    int logId = Convert.ToInt32(row.Cells["LogID"].Value);
                    string targetAction = row.Cells["ActionType"].Value.ToString();

                    if (MessageBox.Show($"Are you sure you want to permanently delete this security log row?\n\nLog ID: {logId}\nAction: {targetAction}", "Confirm Audit Row Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        using (var conn = _db.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM activity_logs WHERE LogID = @lid";
                            using (var cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@lid", logId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Security audit row successfully purged from the database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshAnalyticsData(); // Reloads grids cleanly
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting audit log row: " + ex.Message, "Database Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // =========================================================================
            // CONDITION B: STANDARD SALES TRANSACTION RECORD DELETION CORE (TABS 0, 1, 2)
            // =========================================================================
            DataGridView activeGrid = activeIndex == 0 ? dgvTodayLog : (activeIndex == 1 ? dgvWeeklyLog : dgvMonthlyLog);
            if (activeGrid == null || activeGrid.SelectedRows.Count == 0) return;

            try
            {
                if (activeGrid.SelectedRows.Count > 0)
                {
                    var row = activeGrid.SelectedRows[0];
                    int tid = Convert.ToInt32(row.Cells["TransactionID"].Value);
                    decimal amt = Convert.ToDecimal(row.Cells["TotalSales"].Value);

                    if (MessageBox.Show("Permanently delete this row record attribute?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _db.DeleteSingleTransaction(tid);
                        _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "SINGLE-DELETE", $"Deleted a single sales log asset entry mapping record ID: {tid} valued ₱ {amt:N2}.", tid.ToString());
                        RefreshAnalyticsData();
                    }
                }
            }
            catch { }
        }
    }
}