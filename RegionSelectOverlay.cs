using System;
using System.Drawing;
using System.Windows.Forms;

namespace CVG
{
    public class RegionSelectOverlay : Form
    {
        private Point start;
        private Point current;
        private bool dragging;

        public Rectangle SelectedRegion { get; private set; }

        public RegionSelectOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            Cursor = Cursors.Cross;

            Bounds = SystemInformation.VirtualScreen;
            BackColor = Color.Black;
            Opacity = 0.35;

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    SelectedRegion = Rectangle.Empty;
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };

            MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left)
                    return;

                dragging = true;
                start = PointToScreen(e.Location);
                current = start;
                Invalidate();
            };

            MouseMove += (s, e) =>
            {
                if (!dragging)
                    return;

                current = PointToScreen(e.Location);
                Invalidate();
            };

            MouseUp += (s, e) =>
            {
                if (!dragging || e.Button != MouseButtons.Left)
                    return;

                dragging = false;
                current = PointToScreen(e.Location);

                SelectedRegion = NormalizeRect(start, current);
                DialogResult = SelectedRegion.Width > 2 && SelectedRegion.Height > 2
                    ? DialogResult.OK
                    : DialogResult.Cancel;

                Close();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!dragging)
                return;

            Rectangle r = NormalizeRect(start, current);

            using var pen = new Pen(Color.White, 2);
            using var fill = new SolidBrush(Color.FromArgb(60, Color.White));

            Rectangle local = new Rectangle(
                r.X - Bounds.X,
                r.Y - Bounds.Y,
                r.Width,
                r.Height
            );

            e.Graphics.FillRectangle(fill, local);
            e.Graphics.DrawRectangle(pen, local);
        }

        private static Rectangle NormalizeRect(Point a, Point b)
        {
            int x1 = Math.Min(a.X, b.X);
            int y1 = Math.Min(a.Y, b.Y);
            int x2 = Math.Max(a.X, b.X);
            int y2 = Math.Max(a.Y, b.Y);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
    }
}
