﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form7 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private Label lblHeader, lblUser, lblPass, lblName, lblRole;
        private TextBox txtUsername, txtPassword, txtFullName;
        private ComboBox cmbRole;
        private Button btnRegister;

        public Form7()
        {
            this.Size = new Size(450, 460);
            this.Text = "Account Management - Register System Personnel";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 249, 250);

            InitializeUserRegUI();
            Theme.Apply(this);
        }

        private void InitializeUserRegUI()
        {
            lblHeader = new Label
            {
                Text = "👤 PERSONNEL ACCOUNT REGISTRATION",
                Location = new Point(20, 20),
                Size = new Size(395, 30),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 44, 52),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Full Name Input
            lblName = new Label { Text = "Personnel Full Name (Pangalan):", Location = new Point(40, 75), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtFullName = new TextBox { Location = new Point(40, 100), Size = new Size(350, 28) };

            // Username Input
            lblUser = new Label { Text = "Desired Username:", Location = new Point(40, 145), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtUsername = new TextBox { Location = new Point(40, 170), Size = new Size(350, 28) };

            // Password Input
            lblPass = new Label { Text = "Account Password:", Location = new Point(40, 215), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtPassword = new TextBox { Location = new Point(40, 240), Size = new Size(350, 28), PasswordChar = '●' };

            // System Role Access Level Combo Box
            lblRole = new Label { Text = "System Access Privilege Role:", Location = new Point(40, 285), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbRole = new ComboBox { Location = new Point(40, 310), Size = new Size(350, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Admin", "Inventory Manager", "Staff" });
            cmbRole.SelectedIndex = 2; // Default sa Staff access level

            btnRegister = new Button
            {
                Text = "Create Personnel Account",
                Location = new Point(40, 365),
                Size = new Size(350, 42),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 44, 52), // Dark Slate Gray Theme
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.Click += new EventHandler(btnRegister_Click);

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblName); this.Controls.Add(txtFullName);
            this.Controls.Add(lblUser); this.Controls.Add(txtUsername);
            this.Controls.Add(lblPass); this.Controls.Add(txtPassword);
            this.Controls.Add(lblRole); this.Controls.Add(cmbRole);
            this.Controls.Add(btnRegister);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please fill up all required fields to create the profile.", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Isave ang account sa database
                _db.AddUserAccount(
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    txtFullName.Text.Trim(),
                    cmbRole.SelectedItem.ToString()
                );

                MessageBox.Show("Personnel profile generated successfully! Account is now active.", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to commit account query: {ex.Message}\nNote: Username must be unique.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
