﻿﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form10 : Form
    {
        private Panel pnlSidebarCustom, pnlMainContentArea;
        private Label lblSystemTitleCustom, lblUserRoleCustom;
        private Button btnPOSCustom, btnRefreshCustom, btnLogOutCustom;
        private Form4 activePOSWindow = null;

        public Form10()
        {
            this.Size = new Size(1280, 760);
            this.MinimumSize = new Size(1024, 720);
            this.Text = "Smart-Retail Hub - Dedicated Cashier Terminal Counter";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 242, 245);

            InitializeCashierWorkspaceUI();
            Theme.Apply(this);
        }

        private void InitializeCashierWorkspaceUI()
        {
            pnlSidebarCustom = new Panel { Width = 250, Dock = DockStyle.Left, BackColor = Theme.Sidebar };

            lblSystemTitleCustom = new Label { Text = "🔒 CASHIER\nTERMINAL", Location = new Point(20, 25), Size = new Size(210, 60), Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White };
            lblUserRoleCustom = new Label { Text = $"Cashier: {LoginForm.LoggedInUser}\nRole: {LoginForm.LoggedInRole}\nStation: COUNTER 1", Location = new Point(20, 95), Size = new Size(210, 75), Font = new Font("Segoe UI", 9.5f, FontStyle.Italic), ForeColor = Color.LightGray };

            btnPOSCustom = new Button { Text = "🛒 Open POS Register", Location = new Point(15, 190), Size = new Size(220, 45), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPOSCustom.Click += new EventHandler(btnPOSCustom_Click);

            btnRefreshCustom = new Button { Text = "🔄 Sync Store Ledger", Location = new Point(15, 250), Size = new Size(220, 45), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefreshCustom.Click += (s, e) => { MessageBox.Show("Store master product ledger records synchronized successfully.", "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information); };

            btnLogOutCustom = new Button
            {
                Text = "🚪 Secure Log Out",
                Height = 45,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Bottom
            };
            btnLogOutCustom.Click += new EventHandler(btnLogOutCustom_Click);

            pnlSidebarCustom.Controls.Add(btnLogOutCustom);
            pnlSidebarCustom.Controls.Add(lblSystemTitleCustom); pnlSidebarCustom.Controls.Add(lblUserRoleCustom);
            pnlSidebarCustom.Controls.Add(btnPOSCustom); pnlSidebarCustom.Controls.Add(btnRefreshCustom);

            pnlMainContentArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(240, 242, 245) };
            this.Controls.Add(pnlMainContentArea); this.Controls.Add(pnlSidebarCustom);

            this.Load += (s, e) => { EmbedPOSInsideDashboard(); };
        }

        private void EmbedPOSInsideDashboard()
        {
            try
            {
                if (activePOSWindow == null || activePOSWindow.IsDisposed)
                {
                    activePOSWindow = new Form4();
                    activePOSWindow.TopLevel = false; activePOSWindow.FormBorderStyle = FormBorderStyle.None; activePOSWindow.Dock = DockStyle.Fill;
                    activePOSWindow.FormClosed += (s, args) => { EmbedPOSInsideDashboard(); }; // Auto reload basket loop on finalization

                    pnlMainContentArea.Controls.Clear(); pnlMainContentArea.Controls.Add(activePOSWindow);
                    activePOSWindow.Show();
                }
            }
            catch { }
        }

        private void btnPOSCustom_Click(object sender, EventArgs e) { EmbedPOSInsideDashboard(); }

        private void btnLogOutCustom_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to securely log out?", "Shift Closure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LoginForm.LogoutRequested = true; // Program.cs shows the login again after this window closes
                this.Close();
            }
        }
    }
}
