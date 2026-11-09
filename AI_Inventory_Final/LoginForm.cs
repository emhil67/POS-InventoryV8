using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AI_Inventory_Final
{
    public partial class LoginForm : Form
    {
        public static string LoggedInUser = "";
        public static string LoggedInRole = "";
        public static bool LogoutRequested = false; // set by a workspace when the user signs out

        private TextBox txtUsernameCustom, txtPasswordCustom;
        private Button btnLoginCustom;

        public LoginForm()
        {
            Text = "Smart-Retail | Sign in";
            ClientSize = new Size(900, 560);
            MinimumSize = new Size(820, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Theme.Bg;
            Font = new Font("Segoe UI", 10f);
            BuildLogin();
        }

        private void BuildLogin()
        {
            var hero = new Panel { Dock = DockStyle.Left, Width = 390, BackColor = Theme.Sidebar, Padding = new Padding(38) };
            var logo = new Label { Text = "S", Location = new Point(38, 46), Size = new Size(54, 54), BackColor = Theme.Orange, ForeColor = Theme.Ink, Font = new Font("Segoe UI", 28, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var brand = new Label { Text = "Smart-Retail", Location = new Point(106, 50), Size = new Size(220, 45), ForeColor = Theme.Text, Font = new Font("Segoe UI", 23, FontStyle.Bold) };
            var heroTitle = new Label { Text = "Smarter inventory.\nBetter decisions.", Location = new Point(38, 190), Size = new Size(310, 105), ForeColor = Theme.Text, Font = new Font("Segoe UI", 24, FontStyle.Bold) };
            var heroText = new Label { Text = "Manage products, monitor stock, track sales, and discover products with AI-powered search — all in one workspace.", Location = new Point(40, 310), Size = new Size(300, 100), ForeColor = Theme.Muted, Font = new Font("Segoe UI", 10.5f), AutoSize = false };
            var footer = new Label { Text = "AI INVENTORY MANAGEMENT SYSTEM  ·  2026", Location = new Point(40, 490), Size = new Size(310, 24), ForeColor = Theme.Muted, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };
            var heroEdge = new Panel { Dock = DockStyle.Right, Width = 1, BackColor = Theme.Border };
            hero.Controls.AddRange(new Control[] { logo, brand, heroTitle, heroText, footer, heroEdge });

            var formPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, Padding = new Padding(54, 60, 54, 40) };
            var title = new Label { Text = "Welcome back", Location = new Point(54, 65), Size = new Size(360, 42), Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Theme.Text };
            var subtitle = new Label { Text = "Enter your account. Your workspace opens automatically.", Location = new Point(56, 111), Size = new Size(370, 28), ForeColor = Theme.Muted };
            var userLabel = MakeLabel("Username", 56, 180);
            txtUsernameCustom = new TextBox { Location = new Point(56, 205), Size = new Size(350, 36), Font = new Font("Segoe UI", 11f), BorderStyle = BorderStyle.FixedSingle, BackColor = Theme.Input, ForeColor = Theme.Text };
            var passLabel = MakeLabel("Password", 56, 265);
            txtPasswordCustom = new TextBox { Location = new Point(56, 290), Size = new Size(350, 36), Font = new Font("Segoe UI", 11f), BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true, BackColor = Theme.Input, ForeColor = Theme.Text };
            btnLoginCustom = new Button { Text = "Sign in  →", Location = new Point(56, 360), Size = new Size(350, 44), BackColor = Theme.Orange, ForeColor = Theme.Ink, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLoginCustom.FlatAppearance.BorderSize = 0; btnLoginCustom.Click += btnLoginCustom_Click;
            var hint = new Label { Text = "Authorized personnel only", Location = new Point(56, 417), Size = new Size(350, 24), ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5f) };
            formPanel.Controls.AddRange(new Control[] { title, subtitle, userLabel, txtUsernameCustom, passLabel, txtPasswordCustom, btnLoginCustom, hint });
            Controls.Add(formPanel); Controls.Add(hero);
            AcceptButton = btnLoginCustom;
            Theme.Apply(this);
        }

        /// <summary>Maps the Role text stored in the users table to one of the three system roles (null = unknown).</summary>
        private static string? NormalizeRole(string? raw)
        {
            string r = (raw ?? "").Trim().ToLowerInvariant();
            if (r == "admin" || r == "administrator") return "Admin";
            if (r == "inventory manager" || r == "manager") return "Inventory Manager";
            if (r == "staff" || r == "cashier") return "Staff";
            return null;
        }

        private static Label MakeLabel(string text, int x, int y) => new Label { Text = text, Location = new Point(x, y), Size = new Size(350, 22), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.Text };

        private void btnLoginCustom_Click(object sender, EventArgs e)
        {
            string inputUser = txtUsernameCustom.Text.Trim();
            string inputPass = txtPasswordCustom.Text;
            if (string.IsNullOrWhiteSpace(inputUser) || string.IsNullOrWhiteSpace(inputPass))
            {
                MessageBox.Show("Please enter your username and password.", "Missing credentials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var db = new DatabaseHelper();
                DataTable userTable = new DataTable();
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    const string query = "SELECT FullName, Role FROM users WHERE Username = @user AND `Password` = @pass LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", inputUser);
                        cmd.Parameters.AddWithValue("@pass", inputPass);
                        using (var adapter = new MySqlDataAdapter(cmd)) adapter.Fill(userTable);
                    }
                }
                if (userTable.Rows.Count > 0)
                {
                    string role = NormalizeRole(Convert.ToString(userTable.Rows[0]["Role"]));
                    if (role == null)
                    {
                        MessageBox.Show("This account has no valid role (Admin, Inventory Manager, or Staff). Please contact the administrator.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    LoggedInUser = Convert.ToString(userTable.Rows[0]["FullName"]) ?? inputUser;
                    LoggedInRole = role;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else MessageBox.Show("The username or password is incorrect. Please try again.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception ex) { MessageBox.Show("Unable to connect to the inventory database.\n\n" + ex.Message, "Database connection", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
