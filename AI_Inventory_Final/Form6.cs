﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form6 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        private Label lblHeader, lblProduct, lblBarcode, lblQty, lblExpiry, lblRemarks;
        private ComboBox cmbProductsCustom;
        private TextBox txtBarcodeScanCustom, txtQuantityCustom, txtRemarksCustom;
        private DateTimePicker dtpExpiryCustom;
        private Button btnRestockCustom;
        private int _selectedProductId = 0;

        public Form6()
        {
            this.Size = new Size(460, 540);
            this.Text = "Inventory Management - Hybrid Restock Counter";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 249, 250);

            InitializeHybridRestockUI();
            Theme.Apply(this);
        }

        private void InitializeHybridRestockUI()
        {
            lblHeader = new Label { Text = "📥 INVENTORY RESTOCK COUNTER (STOCK-IN)", Location = new Point(20, 20), Size = new Size(405, 30), Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(21, 67, 96), TextAlign = ContentAlignment.MiddleCenter };

            lblProduct = new Label { Text = "Option 1: Select Existing Product (Manual Dropdown):", Location = new Point(40, 75), Size = new Size(380, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbProductsCustom = new ComboBox { Location = new Point(40, 100), Size = new Size(380, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProductsCustom.SelectedIndexChanged += new EventHandler(cmbProductsCustom_SelectedIndexChanged);

            lblBarcode = new Label { Text = "Option 2: Scan Product Barcode (Hardware Scanner):", Location = new Point(40, 145), Size = new Size(380, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtBarcodeScanCustom = new TextBox { Location = new Point(40, 170), Size = new Size(380, 28), Font = new Font("Segoe UI", 11) };
            txtBarcodeScanCustom.TextChanged += new EventHandler(txtBarcodeScanCustom_TextChanged);

            lblQty = new Label { Text = "Restock Quantity to Add (Bilang ng Idaragdag):", Location = new Point(40, 215), Size = new Size(380, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtQuantityCustom = new TextBox { Location = new Point(40, 240), Size = new Size(380, 28), Text = "1" };

            lblExpiry = new Label { Text = "New Batch Expiration Date (Expiry ng Bagong Kargada):", Location = new Point(40, 285), Size = new Size(380, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            dtpExpiryCustom = new DateTimePicker { Location = new Point(40, 310), Size = new Size(380, 28), Format = DateTimePickerFormat.Short };

            lblRemarks = new Label { Text = "Transaction Remarks / Supplier Details:", Location = new Point(40, 355), Size = new Size(380, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtRemarksCustom = new TextBox { Location = new Point(40, 380), Size = new Size(380, 28), Text = "New Batch Supply Restock" };

            btnRestockCustom = new Button { Text = "Process Restock & Generate New Batch", Location = new Point(40, 435), Size = new Size(380, 45), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRestockCustom.Click += new EventHandler(btnRestockCustom_Click);

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblProduct); this.Controls.Add(cmbProductsCustom);
            this.Controls.Add(lblBarcode); this.Controls.Add(txtBarcodeScanCustom);
            this.Controls.Add(lblQty); this.Controls.Add(txtQuantityCustom);
            this.Controls.Add(lblExpiry); this.Controls.Add(dtpExpiryCustom);
            this.Controls.Add(lblRemarks); this.Controls.Add(txtRemarksCustom);
            this.Controls.Add(btnRestockCustom);

            this.Load += new EventHandler(Form6_Load);
        }

        private void Form6_Load(object sender, EventArgs e) { LoadProductCatalog(); }

        private void LoadProductCatalog()
        {
            try
            {
                DataTable items = _db.GetAllProductsForSearch();
                DataRow topRow = items.NewRow(); topRow["ProductID"] = 0; topRow["ProductName"] = "-- Choose Item --"; topRow["Barcode"] = "";
                items.Rows.InsertAt(topRow, 0);
                cmbProductsCustom.DataSource = items; cmbProductsCustom.DisplayMember = "ProductName"; cmbProductsCustom.ValueMember = "ProductID";
            }
            catch { }
        }

        private void cmbProductsCustom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProductsCustom.SelectedIndex > 0 && cmbProductsCustom.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)cmbProductsCustom.SelectedItem;
                _selectedProductId = Convert.ToInt32(selectedRow["ProductID"]);
                if (txtBarcodeScanCustom.Text != selectedRow["Barcode"].ToString()) txtBarcodeScanCustom.Text = selectedRow["Barcode"].ToString();
            }
            else { _selectedProductId = 0; }
        }

        private void txtBarcodeScanCustom_TextChanged(object sender, EventArgs e)
        {
            string currentBarcode = txtBarcodeScanCustom.Text.Trim();
            if (string.IsNullOrEmpty(currentBarcode)) return;
            foreach (DataRowView item in cmbProductsCustom.Items)
            {
                if (item["Barcode"].ToString() == currentBarcode) { cmbProductsCustom.SelectedItem = item; _selectedProductId = Convert.ToInt32(item["ProductID"]); break; }
            }
        }

        private void btnRestockCustom_Click(object sender, EventArgs e)
        {
            if (_selectedProductId == 0 || !int.TryParse(txtQuantityCustom.Text, out int qty) || qty <= 0) return;
            try
            {
                string remarks = txtRemarksCustom.Text.Trim();
                _db.ProcessProductRestock(_selectedProductId, qty, dtpExpiryCustom.Value, remarks);

                // CRITICAL: SECURE AUDIT LOG EVENT TRIGGER LINK
                _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "STOCK-IN", $"Restocked {qty} units into Product ID: {_selectedProductId}.", txtBarcodeScanCustom.Text.Trim());

                MessageBox.Show("Inventory successfully restocked as a new batch!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
