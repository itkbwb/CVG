using System;
using System.Drawing;
using System.Windows.Forms;
using static CVG.Form1;
using Timer = System.Windows.Forms.Timer;

namespace CVG
{
    public class PreviewOverlay : Form
    {
        private readonly UiItem item;
        private readonly Timer closeTimer;

        public PreviewOverlay(UiItem item, int showMs = 800)
        {
            this.item = item;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            BackColor = Color.Lime;
            TransparencyKey = Color.Lime;

            Bounds = SystemInformation.VirtualScreen;

            closeTimer = new Timer();
            closeTimer.Interval = Math.Max(100, showMs);
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                Close();
            };
            closeTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (item == null)
                return;

            if (item.Kind == UiItemKind.ReadRegion)
            {
                DrawRegion(e.Graphics);
            }
            else
            {
                DrawPoint(e.Graphics);
            }
        }

        private void DrawRegion(Graphics g)
        {
            var r = new Rectangle(item.X, item.Y, item.Width, item.Height);

            var local = new Rectangle(
                r.X - Bounds.X,
                r.Y - Bounds.Y,
                r.Width,
                r.Height
            );

            using var pen = new Pen(Color.Red, 3);
            g.DrawRectangle(pen, local);
        }

        private void DrawPoint(Graphics g)
        {
            int x = item.X - Bounds.X;
            int y = item.Y - Bounds.Y;

            using var pen = new Pen(Color.Red, 3);

            int size = 18;
            g.DrawLine(pen, x - size, y, x + size, y);
            g.DrawLine(pen, x, y - size, x, y + size);
            g.DrawEllipse(pen, x - 6, y - 6, 12, 12);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                closeTimer?.Dispose();

            base.Dispose(disposing);
        }
    }
}
