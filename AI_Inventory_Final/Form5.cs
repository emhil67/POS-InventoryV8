﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form5 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private readonly SemanticSearchService _aiSearch = new SemanticSearchService();

        private Label lblHeader, lblBarcodeCustom, lblName, lblDesc, lblCategory, lblPrice, lblStock, lblThreshold, lblLocation, lblExpiryCustom, lblQuickCatTitle;
        private TextBox txtBarcodeCustom, txtName, txtDesc, txtPrice, txtStock, txtThreshold, txtLocation, txtNewCategoryCustom;
        private DateTimePicker dtpExpiryCustom;
        private ComboBox cmbCategory;
        private Button btnSave, btnAddCategoryDirectCustom;

        public Form5()
        {
            this.Size = new Size(500, 760);
            this.Text = "Inventory Management - Register New Arrival Stock";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 249, 250);

            InitializeRegistrationUI();
            Theme.Apply(this);
        }

        private void InitializeRegistrationUI()
        {
            lblHeader = new Label { Text = "➕ NEW ARRIVAL STOCK REGISTRATION", Location = new Point(20, 20), Size = new Size(445, 30), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41), TextAlign = ContentAlignment.MiddleCenter };

            lblBarcodeCustom = new Label { Text = "Scan or Type Product Barcode (UPC/EAN):", Location = new Point(40, 70), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtBarcodeCustom = new TextBox { Location = new Point(40, 92), Size = new Size(400, 28), Font = new Font("Segoe UI", 11) };

            lblName = new Label { Text = "Product Name / Title:", Location = new Point(40, 130), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtName = new TextBox { Location = new Point(40, 152), Size = new Size(400, 28) };

            lblCategory = new Label { Text = "Assigned Inventory Category:", Location = new Point(40, 200), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbCategory = new ComboBox { Location = new Point(40, 222), Size = new Size(400, 28), DropDownStyle = ComboBoxStyle.DropDownList };

            lblQuickCatTitle = new Label { Text = "Or Create New Category Here (Magdagdag ng Bagong Kategorya):", Location = new Point(40, 260), Size = new Size(400, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = Color.DimGray };
            txtNewCategoryCustom = new TextBox { Location = new Point(40, 280), Size = new Size(290, 28), Font = new Font("Segoe UI", 9.5f) };

            btnAddCategoryDirectCustom = new Button { Text = "➕ Add Cat", Location = new Point(340, 279), Size = new Size(100, 29), Font = new Font("Segoe UI", 9f, FontStyle.Bold), BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddCategoryDirectCustom.Click += new EventHandler(btnAddCategoryDirect_Click);

            lblLocation = new Label { Text = "Warehouse Storage Location (e.g., Rack A, Shelf 3):", Location = new Point(40, 320), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtLocation = new TextBox { Location = new Point(40, 342), Size = new Size(400, 28), Text = "Rack A, Shelf 1" };

            lblDesc = new Label { Text = "Detailed Product Description (For AI Semantic Matching):", Location = new Point(40, 380), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtDesc = new TextBox { Location = new Point(40, 402), Size = new Size(400, 65), Multiline = true, ScrollBars = ScrollBars.Vertical };

            lblExpiryCustom = new Label { Text = "Product Stock Expiration Date (Batch Expiry Tag):", Location = new Point(40, 480), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            dtpExpiryCustom = new DateTimePicker { Location = new Point(40, 502), Size = new Size(400, 28), Format = DateTimePickerFormat.Short };

            lblPrice = new Label { Text = "Retail Unit Price (₱):", Location = new Point(40, 545), Size = new Size(120, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtPrice = new TextBox { Location = new Point(40, 567), Size = new Size(120, 28) };

            lblStock = new Label { Text = "Initial Stock Qty:", Location = new Point(180, 545), Size = new Size(110, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtStock = new TextBox { Location = new Point(180, 567), Size = new Size(110, 28) };

            lblThreshold = new Label { Text = "Low Stock Alert At:", Location = new Point(310, 545), Size = new Size(130, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtThreshold = new TextBox { Location = new Point(310, 567), Size = new Size(130, 28), Text = "5" };

            btnSave = new Button { Text = "Generate AI Embedding & Save Product", Location = new Point(40, 630), Size = new Size(400, 45), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += new EventHandler(btnSave_Click);

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblBarcodeCustom); this.Controls.Add(txtBarcodeCustom);
            this.Controls.Add(lblName); this.Controls.Add(txtName);
            this.Controls.Add(lblCategory); this.Controls.Add(cmbCategory);
            this.Controls.Add(lblQuickCatTitle); this.Controls.Add(txtNewCategoryCustom); this.Controls.Add(btnAddCategoryDirectCustom);
            this.Controls.Add(lblLocation); this.Controls.Add(txtLocation);
            this.Controls.Add(lblDesc); this.Controls.Add(txtDesc);
            this.Controls.Add(lblExpiryCustom); this.Controls.Add(dtpExpiryCustom);
            this.Controls.Add(lblPrice); this.Controls.Add(txtPrice);
            this.Controls.Add(lblStock); this.Controls.Add(txtStock);
            this.Controls.Add(lblThreshold); this.Controls.Add(txtThreshold);
            this.Controls.Add(btnSave);

            this.Load += new EventHandler(Form5_Load);
        }

        private void Form5_Load(object sender, EventArgs e) { LoadCategoryDropdown(); }

        private void LoadCategoryDropdown()
        {
            try { DataTable categories = _db.GetCategories(); cmbCategory.DataSource = categories; cmbCategory.DisplayMember = "CategoryName"; cmbCategory.ValueMember = "CategoryID"; }
            catch { }
        }

        private void btnAddCategoryDirect_Click(object sender, EventArgs e)
        {
            string newCatName = txtNewCategoryCustom.Text.Trim();
            if (string.IsNullOrEmpty(newCatName)) return;
            try
            {
                _db.AddNewCategoryDirect(newCatName);
                MessageBox.Show($"Category '{newCatName}' successfully created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNewCategoryCustom.Clear();
                LoadCategoryDropdown();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBarcodeCustom.Text) || string.IsNullOrWhiteSpace(txtName.Text) || txtDesc.Text.Length < 10) return;
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || !int.TryParse(txtStock.Text, out int stock) || !int.TryParse(txtThreshold.Text, out int threshold)) return;

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string finalizedDescription = $"{txtDesc.Text.Trim()} Location: [{txtLocation.Text.Trim()}]";
                string searchPayloadText = $"{txtName.Text.Trim()} {finalizedDescription}";
                string vectorBase64Text = _aiSearch.GetEmbeddingAsString(searchPayloadText);

                _db.AddProductWithBarcode(txtBarcodeCustom.Text.Trim(), txtName.Text.Trim(), finalizedDescription, Convert.ToInt32(cmbCategory.SelectedValue), price, stock, threshold, dtpExpiryCustom.Value, vectorBase64Text);

                // CRITICAL: SECURE AUDIT LOG EVENT TRIGGER LINK
                _db.LogActivity(LoginForm.LoggedInUser, LoginForm.LoggedInRole, "PRODUCT-REG", $"Registered product: {txtName.Text.Trim()} with Stock of {stock}.", txtBarcodeCustom.Text.Trim());

                MessageBox.Show("Product recorded successfully!", "Asset Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; this.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { Cursor.Current = Cursors.Default; }
        }
    }
}
