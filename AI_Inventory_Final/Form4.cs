﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AI_Inventory_Final
{
    public class Form4 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private Label lblHeader, lblProduct, lblBarcode, lblQty, lblCustomer, lblDiscount, lblGrandTotal;
        private ComboBox cmbProductsCustom, cmbCustomersCustom, cmbDiscountCustom;
        private TextBox txtBarcodeScanCustom, txtQuantityCustom;
        private Button btnAddToCartCustom, btnFinalizeCheckoutCustom;
        private DataGridView dgvCartCustom;
        private DataTable _cartTable;
        private decimal _grandTotalAmount = 0;

        public Form4()
        {
            this.Size = new Size(880, 650);
            this.Text = "Point Of Sale (POS) - FEFO/FIFO Smart Terminal";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 242, 245);

            InitializeMultiItemPOSUI();
            Theme.Apply(this);
        }

        private void InitializeMultiItemPOSUI()
        {
            lblHeader = new Label { Text = "🛒 ENTERPRISE SMART POS TERMINAL (FEFO/FIFO ACTIVE)", Location = new Point(20, 15), Size = new Size(820, 30), Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), TextAlign = ContentAlignment.MiddleCenter };

            lblProduct = new Label { Text = "Select Store Item Batch (Inuna ang pinakamalapit ma-expire):", Location = new Point(20, 65), Size = new Size(310, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbProductsCustom = new ComboBox { Location = new Point(20, 88), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProductsCustom.SelectedIndexChanged += new EventHandler(cmbProductsCustom_SelectedIndexChanged);

            lblBarcode = new Label { Text = "Or Scan Product Barcode (Hardware Link):", Location = new Point(20, 130), Size = new Size(300, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtBarcodeScanCustom = new TextBox { Location = new Point(20, 152), Size = new Size(300, 28), Font = new Font("Segoe UI", 11) };
            txtBarcodeScanCustom.TextChanged += new EventHandler(txtBarcodeScanCustom_TextChanged);

            lblQty = new Label { Text = "Quantity to Purchase (Dami):", Location = new Point(20, 195), Size = new Size(300, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtQuantityCustom = new TextBox { Location = new Point(20, 218), Size = new Size(300, 28), Text = "1" };

            btnAddToCartCustom = new Button { Text = "📥 Add Item To Shopping Cart", Location = new Point(20, 265), Size = new Size(300, 40), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddToCartCustom.Click += new EventHandler(btnAddToCartCustom_Click);

            lblCustomer = new Label { Text = "Assign Registered Customer Profile:", Location = new Point(20, 330), Size = new Size(300, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbCustomersCustom = new ComboBox { Location = new Point(20, 352), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };

            lblDiscount = new Label { Text = "Apply Special Store Discount (Promos):", Location = new Point(20, 400), Size = new Size(300, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbDiscountCustom = new ComboBox { Location = new Point(20, 422), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDiscountCustom.Items.AddRange(new string[] { "None (Standard Rate)", "10% Student Discount", "20% Senior Citizen / PWD" });
            cmbDiscountCustom.SelectedIndex = 0;
            cmbDiscountCustom.SelectedIndexChanged += new EventHandler(cmbDiscountCustom_SelectedIndexChanged);

            dgvCartCustom = new DataGridView { Location = new Point(345, 65), Size = new Size(495, 385), BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowTemplate = { Height = 30 } };
            lblGrandTotal = new Label { Text = "GRAND TOTAL: ₱ 0.00", Location = new Point(345, 470), Size = new Size(495, 35), Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 135, 84), TextAlign = ContentAlignment.MiddleRight };

            btnFinalizeCheckoutCustom = new Button { Text = "💳 Finalize Payment & Print Receipt", Location = new Point(345, 525), Size = new Size(495, 48), Font = new Font("Segoe UI", 11f, FontStyle.Bold), BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnFinalizeCheckoutCustom.Click += new EventHandler(btnFinalizeCheckoutCustom_Click);

            this.Controls.Add(lblHeader); this.Controls.Add(lblProduct); this.Controls.Add(cmbProductsCustom);
            this.Controls.Add(lblBarcode); this.Controls.Add(txtBarcodeScanCustom); this.Controls.Add(lblQty);
            this.Controls.Add(txtQuantityCustom); this.Controls.Add(btnAddToCartCustom);
            this.Controls.Add(lblCustomer); this.Controls.Add(cmbCustomersCustom);
            this.Controls.Add(lblDiscount); this.Controls.Add(cmbDiscountCustom);
            this.Controls.Add(dgvCartCustom); this.Controls.Add(lblGrandTotal); this.Controls.Add(btnFinalizeCheckoutCustom);

            BuildTemporaryCartStructure();
            this.Load += new EventHandler(Form4_Load);
        }

        private void BuildTemporaryCartStructure()
        {
            _cartTable = new DataTable();
            _cartTable.Columns.Add("ProductID", typeof(int)); _cartTable.Columns.Add("Barcode", typeof(string));
            _cartTable.Columns.Add("Product Name", typeof(string)); _cartTable.Columns.Add("Unit Price", typeof(decimal));
            _cartTable.Columns.Add("Qty", typeof(int)); _cartTable.Columns.Add("Total Cost", typeof(decimal));
            dgvCartCustom.DataSource = _cartTable;
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            LoadSmartFEFOProducts();
            try
            {
                cmbCustomersCustom.DataSource = _db.GetCustomers(); cmbCustomersCustom.DisplayMember = "CustomerName"; cmbCustomersCustom.ValueMember = "CustomerID";
            }
            catch { }
        }

        // 🔮 FEFO EXTRACTION: Hinahila ang items kung saan unahan ang malapit nang ma-expire (ORDER BY ExpirationDate ASC)
        private void LoadSmartFEFOProducts()
        {
            try
            {
                DataTable dt = new DataTable();
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    // Ang query na ito ay pinalalabas ang Expiration Date sa mismong pangalan para makita ng Cashier!
                    string query = @"SELECT ProductID, Barcode, Price, CurrentStock,
                                     CASE 
                                        WHEN ExpirationDate IS NOT NULL THEN CONCAT(ProductName, ' (Exp: ', DATE_FORMAT(ExpirationDate, '%m/%Y'), ') [Stock: ', CurrentStock, ']')
                                        ELSE CONCAT(ProductName, ' (No Expiry) [Stock: ', CurrentStock, ']')
                                     END AS DisplayName
                                     FROM Products 
                                     WHERE CurrentStock > 0 
                                     ORDER BY CASE WHEN ExpirationDate IS NULL THEN 1 ELSE 0 END, ExpirationDate ASC"; // Kung walang expiry, inilalagay sa hulihan
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                    }
                }

                DataRow topRow = dt.NewRow(); topRow["ProductID"] = 0; topRow["DisplayName"] = "-- Choose Item Batch (FEFO Sorted) --"; topRow["Barcode"] = "";
                dt.Rows.InsertAt(topRow, 0);

                cmbProductsCustom.DataSource = dt;
                cmbProductsCustom.DisplayMember = "DisplayName";
                cmbProductsCustom.ValueMember = "ProductID";
            }
            catch { }
        }

        private void cmbProductsCustom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProductsCustom.SelectedIndex > 0 && cmbProductsCustom.SelectedItem != null)
            {
                DataRowView row = (DataRowView)cmbProductsCustom.SelectedItem;
                if (txtBarcodeScanCustom.Text != row["Barcode"].ToString()) txtBarcodeScanCustom.Text = row["Barcode"].ToString();
            }
        }

        private void txtBarcodeScanCustom_TextChanged(object sender, EventArgs e)
        {
            string currentBarcode = txtBarcodeScanCustom.Text.Trim();
            if (string.IsNullOrEmpty(currentBarcode)) return;

            // AUTOMATED SCANNING PRIORITY: Pag nag-scan, awtomatikong pipiliin ang may ganyang barcode na pinakamalapit nang ma-expire!
            foreach (DataRowView item in cmbProductsCustom.Items)
            {
                if (item["Barcode"].ToString() == currentBarcode)
                {
                    cmbProductsCustom.SelectedItem = item;
                    break; // Titigil sa una (na siyang pinaka-critical expiring batch ayon sa sort query!)
                }
            }
        }

        private void btnAddToCartCustom_Click(object sender, EventArgs e)
        {
            if (cmbProductsCustom.SelectedIndex <= 0 || !int.TryParse(txtQuantityCustom.Text, out int qty) || qty <= 0) return;
            DataRowView selectedProduct = (DataRowView)cmbProductsCustom.SelectedItem;
            int prodId = Convert.ToInt32(selectedProduct["ProductID"]);
            string bcode = selectedProduct["Barcode"].ToString();
            string name = selectedProduct["DisplayName"].ToString();
            decimal price = Convert.ToDecimal(selectedProduct["Price"]);
            int availableStock = Convert.ToInt32(selectedProduct["CurrentStock"]);
            if (availableStock < qty) { MessageBox.Show("Shortage failure! Insufficient stock in this specific lot batch.", "POS Alert", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }
            bool found = false;
            foreach (DataRow row in _cartTable.Rows)
            {
                if (Convert.ToInt32(row["ProductID"]) == prodId)
                {
                    row["Qty"] = Convert.ToInt32(row["Qty"]) + qty;
                    row["Total Cost"] = Convert.ToDecimal(row["Unit Price"]) * Convert.ToInt32(row["Qty"]);
                    found = true; break;
                }
            }
            if (!found)
            {
                DataRow cartRow = _cartTable.NewRow();
                cartRow["ProductID"] = prodId; cartRow["Barcode"] = bcode; cartRow["Product Name"] = name;
                cartRow["Unit Price"] = price; cartRow["Qty"] = qty; cartRow["Total Cost"] = price * qty;
                _cartTable.Rows.Add(cartRow);
            }
            CalculateGrandTotalSummary();
            txtBarcodeScanCustom.Clear(); txtQuantityCustom.Text = "1"; cmbProductsCustom.SelectedIndex = 0;
        }
        private void cmbDiscountCustom_SelectedIndexChanged(object sender, EventArgs e) { CalculateGrandTotalSummary(); }
        private void CalculateGrandTotalSummary()
        {
            decimal subtotal = 0;
            foreach (DataRow row in _cartTable.Rows) { subtotal += Convert.ToDecimal(row["Total Cost"]); }
            decimal rate = cmbDiscountCustom.SelectedIndex == 1 ? 0.10m : (cmbDiscountCustom.SelectedIndex == 2 ? 0.20m : 0.00m);
            _grandTotalAmount = subtotal - (subtotal * rate);
            lblGrandTotal.Text = $"GRAND TOTAL: ₱ {_grandTotalAmount:N2}";
        }
        private void btnFinalizeCheckoutCustom_Click(object sender, EventArgs e)
        {
            if (_cartTable.Rows.Count == 0) return;
            string generatedOrderGroupId = "REC-" + DateTime.Now.Ticks.ToString().Substring(10);
            int assignedCustomerId = Convert.ToInt32(cmbCustomersCustom.SelectedValue);
            string chosenDiscountName = cmbDiscountCustom.SelectedItem.ToString();
            try
            {
                decimal subtotal = 0;
                foreach (DataRow row in _cartTable.Rows) { subtotal += Convert.ToDecimal(row["Total Cost"]); }
                decimal discountAmount = subtotal * (cmbDiscountCustom.SelectedIndex == 1 ? 0.10m : (cmbDiscountCustom.SelectedIndex == 2 ? 0.20m : 0.00m));
                using (Form8 invoiceWindow = new Form8(_cartTable, subtotal, chosenDiscountName, discountAmount, _grandTotalAmount, "Multi-Item Sale", LoginForm.LoggedInUser))
                {
                    if (invoiceWindow.ShowDialog(this) == DialogResult.OK && invoiceWindow.IsTransactionPaid)
                    {
                        foreach (DataRow cartRow in _cartTable.Rows)
                        {
                            _db.ProcessMultiItemFIFOSale(generatedOrderGroupId, Convert.ToInt32(cartRow["ProductID"]), cartRow["Barcode"].ToString(), Convert.ToInt32(cartRow["Qty"]), chosenDiscountName, discountAmount, _grandTotalAmount, invoiceWindow.FinalCashReceived, (invoiceWindow.FinalCashReceived - _grandTotalAmount), assignedCustomerId, "POS Sale");
                        }
                        _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "STOCK-OUT", $"Finalized POS sale cart checkout totaling ₱ {_grandTotalAmount:N2} using FEFO enforcement routing.", generatedOrderGroupId);
                        MessageBox.Show("Transaction Finalized!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK; this.Close();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}