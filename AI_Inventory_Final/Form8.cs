﻿﻿using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    public class Form8 : Form
    {
        private DataTable _cartItems;
        private string _remarks, _cashier, _discountName;
        private decimal _subtotalAmount, _discountAmount, _finalGrandTotal, _cashReceived, _changeDue;

        private Label lblHeader, lblReceiptView, lblChange;
        private TextBox txtCashTendered;
        private Button btnFinalize;

        public bool IsTransactionPaid { get; private set; } = false;
        public decimal FinalCashReceived => _cashReceived;

        public Form8(DataTable cartItems, decimal subtotal, string discountName, decimal discountAmount, decimal finalGrandTotal, string remarks, string cashier)
        {
            _cartItems = cartItems; _subtotalAmount = subtotal; _discountName = discountName;
            _discountAmount = discountAmount; _finalGrandTotal = finalGrandTotal; _remarks = remarks; _cashier = cashier;

            this.Size = new Size(460, 680);
            this.Text = "POS Counter - Cash Payment & Receipt Preview";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 246, 250);

            InitializeReceiptUI();
            Theme.Apply(this);
        }

        private void InitializeReceiptUI()
        {
            lblHeader = new Label { Text = "💶 CASHIER MULTI-ITEM RECEIPT GENERATOR", Location = new Point(20, 15), Size = new Size(405, 25), Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), TextAlign = ContentAlignment.MiddleCenter };
            lblReceiptView = new Label { Location = new Point(20, 55), Size = new Size(405, 380), BackColor = Theme.Input, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 9.5f, FontStyle.Regular), ForeColor = Theme.Text, Padding = new Padding(12), Text = GenerateReceiptText(0, 0) };

            Label lblInputTitle = new Label { Text = "Enter Cash Received (Bayad ng Customer):", Location = new Point(20, 455), Size = new Size(405, 20), Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            txtCashTendered = new TextBox { Location = new Point(20, 480), Size = new Size(405, 30), Font = new Font("Segoe UI", 12f, FontStyle.Bold) };
            txtCashTendered.TextChanged += new EventHandler(txtCashTendered_TextChanged);

            lblChange = new Label { Text = "CHANGE DUE (SUKLI): ₱ 0.00", Location = new Point(20, 525), Size = new Size(405, 25), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 135, 84), TextAlign = ContentAlignment.MiddleLeft };
            btnFinalize = new Button { Text = "Print Official Receipt & Finalize Sale", Location = new Point(20, 565), Size = new Size(405, 45), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(44, 62, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnFinalize.Click += new EventHandler(btnFinalize_Click);

            this.Controls.Add(lblHeader); this.Controls.Add(lblReceiptView); this.Controls.Add(lblInputTitle); this.Controls.Add(txtCashTendered); this.Controls.Add(lblChange); this.Controls.Add(btnFinalize);
        }

        public string GenerateReceiptText(decimal cash, decimal change)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================");
            sb.AppendLine("          OFFICIAL RETAIL SLIP           ");
            sb.AppendLine("=========================================");
            sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Cashier: {_cashier}");
            sb.AppendLine("-----------------------------------------");
            sb.AppendLine(string.Format("{0,-22} {1,-5} {2,12}", "ITEM NAME", "QTY", "TOTAL COST"));
            sb.AppendLine("-----------------------------------------");
            foreach (DataRow row in _cartItems.Rows)
            {
                string rawName = row["Product Name"].ToString();
                string displayName = rawName.Length > 20 ? rawName.Substring(0, 17) + "..." : rawName;
                sb.AppendLine(string.Format("{0,-22} {1,-5} ₱ {2,10:N2}", displayName, row["Qty"], row["Total Cost"]));
            }
            sb.AppendLine("-----------------------------------------");
            sb.AppendLine(string.Format("SUBTOTAL:                  ₱ {0,10:N2}", _subtotalAmount));
            sb.AppendLine(string.Format("DISCOUNT TYPE:       {0}", _discountName));
            sb.AppendLine(string.Format("DISCOUNT AMOUNT:          -₱ {0,10:N2}", _discountAmount));
            sb.AppendLine("-----------------------------------------");
            sb.AppendLine(string.Format("GRAND TOTAL:               ₱ {0,10:N2}", _finalGrandTotal));
            sb.AppendLine(string.Format("CASH RECEIVED:             ₱ {0,10:N2}", cash));
            sb.AppendLine(string.Format("CHANGE DUE:                ₱ {0,10:N2}", change));
            sb.AppendLine("=========================================");
            return sb.ToString();
        }

        private void txtCashTendered_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtCashTendered.Text, out decimal cash))
            {
                _cashReceived = cash;
                if (_cashReceived >= _finalGrandTotal)
                {
                    _changeDue = _cashReceived - _finalGrandTotal;
                    lblChange.Text = $"CHANGE DUE (SUKLI): ₱ {_changeDue:N2}";
                    lblReceiptView.Text = GenerateReceiptText(_cashReceived, _changeDue);
                }
                else { lblChange.Text = "CHANGE DUE: [ INSUFFICIENT CASH ]"; lblChange.ForeColor = Theme.Red; }
            }
        }

        private void btnFinalize_Click(object sender, EventArgs e)
        {
            if (_cashReceived < _finalGrandTotal) return;
            try { PrintDocument pd = new PrintDocument(); pd.PrintPage += new PrintPageEventHandler(PrintReceiptDocPattern); pd.Print(); } catch { }
            this.IsTransactionPaid = true; this.DialogResult = DialogResult.OK; this.Close();
        }

        private void PrintReceiptDocPattern(object sender, PrintPageEventArgs e)
        {
            Font receiptFont = new Font("Consolas", 8.5f, FontStyle.Regular);
            e.Graphics.DrawString(GenerateReceiptText(_cashReceived, _changeDue), receiptFont, Brushes.Black, 10, 10);
        }
    }
}