﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form9 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private int _targetProductId;
        private Label lblHeader, lblName, lblDesc, lblPrice, lblThreshold;
        private TextBox txtName, txtDesc, txtPrice, txtThreshold;
        private Button btnUpdateCustom;

        public Form9(int productId, string currentName, string currentDesc, decimal currentPrice, int currentThreshold)
        {
            _targetProductId = productId;
            this.Size = new Size(450, 480);
            this.Text = "Catalog Matrix Management - Edit Product Attributes";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 249, 250);

            InitializeModifierUI(currentName, currentDesc, currentPrice, currentThreshold);
            Theme.Apply(this);
        }

        private void InitializeModifierUI(string name, string desc, decimal price, int threshold)
        {
            lblHeader = new Label { Text = "✏️ EDIT PRODUCT CATALOG PROFILE", Location = new Point(20, 20), Size = new Size(395, 30), Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), TextAlign = ContentAlignment.MiddleCenter };

            lblName = new Label { Text = "Product Title Name:", Location = new Point(40, 75), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtName = new TextBox { Location = new Point(40, 100), Size = new Size(350, 28), Text = name };

            lblDesc = new Label { Text = "Product Description Specifications:", Location = new Point(40, 145), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtDesc = new TextBox { Location = new Point(40, 170), Size = new Size(350, 65), Multiline = true, Text = desc };

            lblPrice = new Label { Text = "Unit Selling Price (₱):", Location = new Point(40, 250), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtPrice = new TextBox { Location = new Point(40, 272), Size = new Size(350, 28), Text = price.ToString() };

            lblThreshold = new Label { Text = "Low Stock Alert Notification Limit:", Location = new Point(40, 315), Size = new Size(350, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtThreshold = new TextBox { Location = new Point(40, 337), Size = new Size(350, 28), Text = threshold.ToString() };

            btnUpdateCustom = new Button { Text = "Save Changes & Update Database Row", Location = new Point(40, 390), Size = new Size(350, 42), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(230, 126, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdateCustom.Click += new EventHandler(btnUpdateCustom_Click);

            this.Controls.Add(lblHeader); this.Controls.Add(lblName); this.Controls.Add(txtName);
            this.Controls.Add(lblDesc); this.Controls.Add(txtDesc); this.Controls.Add(lblPrice);
            this.Controls.Add(txtPrice); this.Controls.Add(lblThreshold); this.Controls.Add(txtThreshold);
            this.Controls.Add(btnUpdateCustom);
        }

        private void btnUpdateCustom_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || !decimal.TryParse(txtPrice.Text, out decimal price) || !int.TryParse(txtThreshold.Text, out int threshold)) return;

            try
            {
                _db.UpdateProductEntry(_targetProductId, txtName.Text.Trim(), txtDesc.Text.Trim(), price, threshold);

                // CRITICAL: SECURE AUDIT LOG EVENT TRIGGER LINK
                _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "PRODUCT-EDIT", $"Modified fields for Product ID: {_targetProductId}. Title: {txtName.Text.Trim()}.", _targetProductId.ToString());

                MessageBox.Show("Catalog information entry updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
