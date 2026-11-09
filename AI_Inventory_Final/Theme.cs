using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AI_Inventory_Final
{
    /// <summary>
    /// Shared dark palette (dark navy surfaces, orange primary, green/blue/pink accents)
    /// and a one-call styler: Theme.Apply(this) at the end of any form's constructor.
    /// Set a control's Tag to "keep" or "nav" to stop Apply from restyling it.
    /// </summary>
    internal static class Theme
    {
        // ---- Surfaces ----
        public static readonly Color Bg = Color.FromArgb(15, 22, 32);          // window canvas
        public static readonly Color Sidebar = Color.FromArgb(11, 17, 25);     // nav rail / brand panel
        public static readonly Color Surface = Color.FromArgb(24, 33, 46);     // cards, grids, tab pages
        public static readonly Color SurfaceAlt = Color.FromArgb(32, 43, 58);  // headers, ghost buttons, hover
        public static readonly Color Input = Color.FromArgb(12, 18, 27);       // text boxes, combo boxes
        public static readonly Color Border = Color.FromArgb(44, 57, 75);
        public static readonly Color CardTint = Color.FromArgb(18, 40, 40);    // green-tinted KPI card
        public static readonly Color CardTintBorder = Color.FromArgb(30, 78, 64);

        // ---- Text ----
        public static readonly Color Text = Color.FromArgb(240, 244, 248);
        public static readonly Color Muted = Color.FromArgb(138, 151, 168);
        public static readonly Color Ink = Color.FromArgb(12, 18, 27);         // dark text on bright buttons

        // ---- Accents ----
        public static readonly Color Orange = Color.FromArgb(255, 138, 0);     // primary / active
        public static readonly Color Green = Color.FromArgb(18, 183, 106);
        public static readonly Color Blue = Color.FromArgb(22, 128, 204);
        public static readonly Color Red = Color.FromArgb(214, 48, 80);
        public static readonly Color Pink = Color.FromArgb(214, 36, 110);
        public static readonly Color Amber = Color.FromArgb(201, 122, 16);

        public static readonly Color SelectionRow = Color.FromArgb(70, 50, 22);
        public static readonly Color AltRow = Color.FromArgb(27, 37, 51);

        // =====================================================================
        public static void Apply(Form form)
        {
            form.BackColor = Bg;
            form.ForeColor = Text;
            DarkTitleBar(form);
            Walk(form);
        }

        private static void Walk(Control root)
        {
            foreach (Control c in root.Controls)
            {
                string tag = c.Tag as string ?? "";
                if (tag != "keep" && tag != "nav") Style(c);
                if (c.HasChildren) Walk(c);
            }
        }

        private static void Style(Control c)
        {
            switch (c)
            {
                case DataGridView g: StyleGrid(g); break;
                case Button b: StyleButton(b); break;
                case TextBox t: t.BackColor = Input; t.ForeColor = Text; t.BorderStyle = BorderStyle.FixedSingle; break;
                case ComboBox cb: cb.FlatStyle = FlatStyle.Flat; cb.BackColor = Input; cb.ForeColor = Text; break;
                case NumericUpDown n: n.BackColor = Input; n.ForeColor = Text; break;
                case DateTimePicker d:
                    d.CalendarMonthBackground = Surface; d.CalendarForeColor = Text;
                    d.CalendarTitleBackColor = SurfaceAlt; d.CalendarTitleForeColor = Text;
                    d.CalendarTrailingForeColor = Muted; d.BackColor = Input; d.ForeColor = Text;
                    break;
                case TabControl tc: StyleTabs(tc); break;
                case TabPage tp: tp.UseVisualStyleBackColor = false; if (IsBright(tp.BackColor)) tp.BackColor = Surface; break;
                case Label l: StyleLabel(l); break;
                case CheckBox or RadioButton: c.ForeColor = Text; break;
                case GroupBox gb: gb.ForeColor = Text; break;
                case Panel p: if (IsBright(p.BackColor)) p.BackColor = IsWhite(p.BackColor) ? Surface : Bg; break;
            }
        }

        // ---- Buttons ----
        private static void StyleButton(Button b)
        {
            b.UseVisualStyleBackColor = false;
            b.FlatStyle = FlatStyle.Flat;
            Color target = MapAccent(b.BackColor, out bool ghost);
            b.BackColor = target;
            b.ForeColor = ghost ? Text : (target == Orange || target == Green ? Ink : Color.White);
            b.FlatAppearance.BorderSize = ghost ? 1 : 0;
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.MouseOverBackColor = Shift(target, ghost ? 10 : 18);
            b.FlatAppearance.MouseDownBackColor = Shift(target, -12);
            b.Cursor = Cursors.Hand;
        }

        /// <summary>Maps the old light-theme button colours onto the new accent palette.</summary>
        public static Color MapAccent(Color c, out bool ghost)
        {
            ghost = false;
            if (IsAccent(c)) return c;
            float s = c.GetSaturation(), l = c.GetBrightness(), h = c.GetHue();
            if (s < 0.2f || l < 0.22f || l > 0.85f) { ghost = true; return SurfaceAlt; }
            if (h >= 345f || h < 15f) return Red;
            if (h < 65f) return Orange;
            if (h < 175f) return Green;
            if (h < 265f) return Blue;
            return Pink;
        }

        // ---- Labels ----
        private static void StyleLabel(Label l)
        {
            if (l.BackColor.A != 0 && IsBright(l.BackColor)) l.BackColor = Surface;

            Color f = l.ForeColor;
            if (IsAccent(f) || f.ToArgb() == Ink.ToArgb() || Lum(f) >= 120) return;

            if (f.GetSaturation() > 0.35f)
            {
                float h = f.GetHue();
                if (h >= 345f || h < 15f) { l.ForeColor = Red; return; }
                if (h < 65f) { l.ForeColor = Orange; return; }
                if (h < 175f) { l.ForeColor = Green; return; }
            }
            bool strong = l.Font.Bold || l.Font.Size >= 11f;
            l.ForeColor = strong ? Text : Muted;
        }

        // ---- Grids ----
        public static void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.BackgroundColor = Surface;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Border;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            g.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;

            g.RowHeadersDefaultCellStyle.BackColor = SurfaceAlt;
            g.RowHeadersDefaultCellStyle.ForeColor = Text;
            g.RowHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;

            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = Text;
            g.DefaultCellStyle.SelectionBackColor = SelectionRow;
            g.DefaultCellStyle.SelectionForeColor = Text;
            g.AlternatingRowsDefaultCellStyle.BackColor = AltRow;
            g.AlternatingRowsDefaultCellStyle.ForeColor = Text;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = SelectionRow;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Text;
        }

        // ---- Tabs (owner-drawn so the headers are dark with an orange active pill) ----
        private static void StyleTabs(TabControl tc)
        {
            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.DrawItem -= DrawTab;
            tc.DrawItem += DrawTab;
            bool busy = false; // guard: setting ItemSize raises Resize again, which must not re-enter FitTabs
            tc.Resize += (s, e) =>
            {
                if (busy) return;
                busy = true;
                try { FitTabs(tc); } finally { busy = false; }
            };
            busy = true;
            try { FitTabs(tc); } finally { busy = false; }
        }

        private static void FitTabs(TabControl tc)
        {
            if (tc.TabCount == 0) return;
            int w = Math.Max(120, (tc.ClientSize.Width - 6) / tc.TabCount);
            var size = new Size(w, 36);
            // Only touch ItemSize when it really changes: setting it raises Resize again,
            // which would otherwise call this method forever (StackOverflow).
            if (tc.SizeMode != TabSizeMode.Fixed) tc.SizeMode = TabSizeMode.Fixed;
            if (tc.ItemSize != size) tc.ItemSize = size;
        }

        private static void DrawTab(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tc || e.Index < 0 || e.Index >= tc.TabPages.Count) return;
            bool selected = e.Index == tc.SelectedIndex;
            Rectangle r = Rectangle.Inflate(e.Bounds, -3, -3);
            using (var back = new SolidBrush(selected ? Orange : SurfaceAlt)) e.Graphics.FillRectangle(back, r);
            TextRenderer.DrawText(e.Graphics, tc.TabPages[e.Index].Text, tc.Font, r,
                selected ? Ink : Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        // ---- Helpers ----
        public static double Lum(Color c) => 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
        private static bool IsBright(Color c) => Lum(c) > 170;
        private static bool IsWhite(Color c) => c.R + c.G + c.B >= 750;

        private static bool IsAccent(Color c)
        {
            int a = c.ToArgb();
            return a == Orange.ToArgb() || a == Green.ToArgb() || a == Blue.ToArgb()
                || a == Red.ToArgb() || a == Pink.ToArgb() || a == Amber.ToArgb();
        }

        public static Color Shift(Color c, int delta) => Color.FromArgb(
            Math.Clamp(c.R + delta, 0, 255), Math.Clamp(c.G + delta, 0, 255), Math.Clamp(c.B + delta, 0, 255));

        /// <summary>Paints a 1px border (and optional left accent stripe) on a card panel.</summary>
        public static void CardBorder(Panel card, Color border, Color? stripe = null)
        {
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                if (stripe.HasValue) using (var b = new SolidBrush(stripe.Value)) e.Graphics.FillRectangle(b, 0, 0, 4, card.Height);
            };
        }

        // ---- Dark title bar on Windows 10 (2004+) / 11 ----
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        private static void DarkTitleBar(Form f)
        {
            void Set()
            {
                try
                {
                    int on = 1;
                    DwmSetWindowAttribute(f.Handle, 20, ref on, sizeof(int));
                    DwmSetWindowAttribute(f.Handle, 19, ref on, sizeof(int));
                }
                catch { /* older Windows: keep the default title bar */ }
            }
            if (f.IsHandleCreated) Set(); else f.HandleCreated += (s, e) => Set();
        }
    }
}
