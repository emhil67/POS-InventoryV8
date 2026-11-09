﻿﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form2 : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        // UI Layout Components
        private GroupBox gbStoreInventoryCustom;
        private DataGridView dgvStoreItemsCustom;
        private Label lblHeader;
        private Button btnDeleteProductCustom;
        private Button btnEditProductCustom; // NEW: Product Catalog Edit Button Module Link
        private Panel pnlTopLayout;

        public Form2()
        {
            // Baseline responsive frame configurations
            this.Size = new Size(980, 680);
            this.MinimumSize = new Size(850, 520); // Pigilan ang pagliit nang sobra na makakasira sa grid structures
            this.Text = "Store Retail Items Directory - Authorized View";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 246, 250);

            InitializeDirectoryUI();
            Theme.Apply(this);
        }

        private void InitializeDirectoryUI()
        {
            // 1. TOP TITLE ACCENT PANEL
            pnlTopLayout = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 15, 20, 0)
            };

            lblHeader = new Label
            {
                Text = "🏪 GENERAL STORE RETAIL DIRECTORY",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Theme.Text
            };
            pnlTopLayout.Controls.Add(lblHeader);

            // 2. MAIN CONTAINER GROUPBOX (FLUID SCALE LAYOUT)
            gbStoreInventoryCustom = new GroupBox
            {
                Text = "All Registered Products, Stock Levels, and Storage Locations",
                Location = new Point(20, 70),
                Size = new Size(925, 470),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.DimGray,
                // ANCHOR CONSTRAINTS: Sasabog at kakalat sa lahat ng panig kapag pinalaki ang main screen!
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvStoreItemsCustom = new DataGridView
            {
                Dock = DockStyle.Fill, // Pupunuin ang buong kahon ng GroupBox para maging fully responsive
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // Pipiliin ang buong linya kapag clinick ng user
                MultiSelect = false,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.Black,

                // AUTO-RESIZE FLUID CONSTANTS
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                RowTemplate = { Height = 40 }
            };
            dgvStoreItemsCustom.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            gbStoreInventoryCustom.Controls.Add(dgvStoreItemsCustom);

            // 3. DELETE PRODUCT ENTRY BUTTON (ANCHORED AT BOTTOM-LEFT)
            btnDeleteProductCustom = new Button
            {
                Text = "🗑️ Delete Selected Product Entry",
                Location = new Point(20, 565),
                Size = new Size(280, 42),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 53, 69), // Crimson Danger Red Theme
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnDeleteProductCustom.Click += new EventHandler(btnDeleteProduct_Click);

            // 4. NEW: EDIT PRODUCT ENTRY BUTTON MODULE (ANCHORED AT BOTTOM-LEFT NEXT TO DELETE)
            btnEditProductCustom = new Button
            {
                Text = "✏️ Edit Selected Product Entry",
                Location = new Point(320, 565), // Inurong sa tabi ng delete button na may sapat na breathing space
                Size = new Size(280, 42),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 126, 34), // Orange Theme Accent Color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnEditProductCustom.Click += new EventHandler(btnEditProductCustom_Click);

            // I-assemble ang mga controls sa Form viewport
            this.Controls.Add(gbStoreInventoryCustom);
            this.Controls.Add(btnDeleteProductCustom);
            this.Controls.Add(btnEditProductCustom); // Isama ang bagong edit controller sa interface canvas
            this.Controls.Add(pnlTopLayout);

            this.Load += new EventHandler(Form2_Load);
        }

        private void Form2_Load(object sender, EventArgs e) { LoadStoreRecords(); }

        private void LoadStoreRecords()
        {
            try
            {
                DataTable dt = _db.GetAllProductsForSearch();
                dgvStoreItemsCustom.DataSource = dt;

                // I-hide ang raw machine algorithm AI byte weights column para malinis tingnan ang grid table
                if (dgvStoreItemsCustom.Columns.Contains("ProductEmbedding"))
                    dgvStoreItemsCustom.Columns["ProductEmbedding"].Visible = false;

                // Ayusin ang mga headers para maging presentable sa inyong panel
                if (dgvStoreItemsCustom.Columns.Contains("Barcode")) dgvStoreItemsCustom.Columns["Barcode"].HeaderText = "Product Barcode";
                if (dgvStoreItemsCustom.Columns.Contains("ProductName")) dgvStoreItemsCustom.Columns["ProductName"].HeaderText = "Product Name";
                if (dgvStoreItemsCustom.Columns.Contains("ExpirationDate")) dgvStoreItemsCustom.Columns["ExpirationDate"].HeaderText = "Expiration Date";
            }
            catch (Exception ex) { MessageBox.Show($"Error syncing store directory lines: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // CONTROL MODULE A: TRIGGER LINK OPENS FORM9 PRODUCT INFORMATION MODIFIER WIZARD
        private void btnEditProductCustom_Click(object sender, EventArgs e)
        {
            if (dgvStoreItemsCustom.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please click on a row inside the catalog grid table to target a single item for modification.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var selectedRow = dgvStoreItemsCustom.SelectedRows[0];
                int pid = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
                string name = selectedRow.Cells["ProductName"].Value.ToString();
                string desc = selectedRow.Cells["Description"].Value.ToString();
                decimal price = Convert.ToDecimal(selectedRow.Cells["Price"].Value);
                int threshold = Convert.ToInt32(selectedRow.Cells["LowStockThreshold"].Value);

                // Buksan ang data modifier editor (Form9) gamit ang mga kasalukuyang detalye nito mula sa database row
                using (Form9 modifierWindow = new Form9(pid, name, desc, price, threshold))
                {
                    if (modifierWindow.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadStoreRecords(); // Awtomatikong i-refresh ang master grid pagkatapos mai-save ang edit lines
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Catalog parsing error context: {ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        // CONTROL MODULE B: DELETION OPERATION WITH SECURITY ROLE ACCESS GATES
        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (LoginForm.LoggedInRole != "Admin")
            {
                MessageBox.Show("Access Denied! Only administrators can delete product records.", "Security Restriction Block", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvStoreItemsCustom.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please click on a row inside the grid to select an item to permanently erase.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dgvStoreItemsCustom.SelectedRows[0];
            int targetedProductId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
            string targetedProductName = selectedRow.Cells["ProductName"].Value.ToString();

            DialogResult doubleCheckResult = MessageBox.Show($"Are you sure you want to permanently delete '{targetedProductName}' from your catalog records?\n\nThis action cannot be undone and will break relational history constraints.", "Confirm Structural Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (doubleCheckResult == DialogResult.Yes)
            {
                try
                {
                    _db.DeleteProduct(targetedProductId);
                    MessageBox.Show("The product inventory line entry has been successfully wiped from database rows.", "Item Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStoreRecords(); // Dynamic data grid layout refresh loops
                }
                catch (Exception ex) { MessageBox.Show($"Database mutation failure: {ex.Message}", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}