using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Tesseract;
using static CVG.Form1;



namespace CVG
{

    public partial class Form1 : Form
    {
        // Координаты клика по экрану
        // Здесь просто пример, подгони под свой монитор
        private int TargetX = 50;
        private int TargetY = 1050;
        private Profile profile;
        private BindingList<UiItem> items;
        private GlobalKeyboardHook keyboardHook;
        private TesseractEngine ocrEngine;
        private string capturedItemName = "";

        private System.Windows.Forms.Timer watchTimer;
        private bool watchRunning;

        private string watchRegionName1 = "axis 2 position value";  // имя UiItem в списке
        private string watchRegionName2 = "axis 3 position value";  // имя UiItem в списке

        private CancellationTokenSource watchCts;
        private Task watchTask;
        private readonly object ocrLock = new object();

        private enum AddCaptureState
        {
            Idle = 0,
            WaitingProcess = 1,
            WaitingLocation = 2
        }

        private AddCaptureState addState = AddCaptureState.Idle;
        private string capturedProcessName = "";

        private System.Windows.Forms.Timer capsHoldTimer;
        private bool capsIsDown;
        private bool overlayStarted;

        public Form1()
        {
            InitializeComponent();
            watchTimer = new System.Windows.Forms.Timer();
            watchTimer.Interval = 500;
            watchTimer.Tick += watchTimer_Tick;

            keyboardHook = new GlobalKeyboardHook();
            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            keyboardHook.KeyUp += KeyboardHook_KeyUp;
            keyboardHook.SuppressKey = SuppressKeyForCapture;

            capsHoldTimer = new System.Windows.Forms.Timer();
            capsHoldTimer.Interval = 1000;
            capsHoldTimer.Tick += CapsHoldTimer_Tick;

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            ocrEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            ocrEngine.SetVariable("tessedit_char_whitelist", "0123456789.,um");

        }

        // Импорты WinAPI для мыши и активации окна
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static string GetActiveProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return "";

            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0)
                return "";

            try
            {
                return Process.GetProcessById((int)pid).ProcessName;
            }
            catch
            {
                return "";
            }
        }


        private static void ClickAt(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, UIntPtr.Zero);
        }

        private static void ActivateVisualStudio()
        {
            // Имя процесса VS обычно devenv
            Process[] vs = Process.GetProcessesByName("devenv");
            if (vs.Length == 0)
                return;

            IntPtr handle = vs[0].MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);
            }
        }

        private void button1_ClickOld(object sender, EventArgs e)
        {
            // 1. Клик по заданным координатам
            ClickAt(TargetX, TargetY);
            Thread.Sleep(200);

            // 2. Ввод текста и Enter
            // Важно чтобы была включена корейская раскладка или IME
            SendKeys.SendWait("camera");
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(500);

            // 3. Переключение на окно Visual Studio
            ActivateVisualStudio();
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F2)
                return;

            var pos = Cursor.Position;

            TargetX = pos.X;
            TargetY = pos.Y;

            if (listBox1.SelectedItem is UiItem item)
            {
                item.X = pos.X;
                item.Y = pos.Y;
                listBox1.Refresh();
                SaveProfile();
            }

            labelCoords.Text = $"X: {TargetX}, Y: {TargetY}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var screens = Screen.AllScreens;

            if (screens.Length < 2)
                return;

            var screen = screens[1]; // второй монитор

            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.WorkingArea.Location;

            profile = ProfileStore.LoadOrCreate();

            items = new BindingList<UiItem>(profile.Items);
            listBox1.DataSource = items;
        }

        Rectangle positionRegion = new Rectangle(
            x: 1382,
            y: 453,
            width: 1483 - 1382,
            height: 472 - 453
        );

        Bitmap CaptureRegion(Rectangle r)
        {
            var bmp = new Bitmap(r.Width, r.Height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(r.Location, Point.Empty, r.Size);
            return bmp;
        }

        string ReadText(Bitmap bmp)
        {
            using var pix = PixConverter.ToPix(bmp);
            using var page = ocrEngine.Process(pix);
            return page.GetText();
        }

        double ParsePosition(string text)
        {
            text = text
                .Replace("µm", "")
                .Replace("um", "")
                .Replace(",", ".")
                .Trim();

            return double.Parse(text, CultureInfo.InvariantCulture);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var bmp = CaptureRegion(positionRegion);
            string raw = ReadText(bmp);

            try
            {
                double pos = ParsePosition(raw);
                labelCoords.Text = pos.ToString("F2") + " µm";
            }
            catch
            {
                labelCoords.Text = "OCR error: " + raw;
            }
        }
        public enum UiItemKind
        {
            ClickPoint = 0,
            ReadRegion = 1
        }

        public class UiItem
        {
            public string Name { get; set; } = "";
            public string ProcessName { get; set; } = "";
            public UiItemKind Kind { get; set; } = UiItemKind.ClickPoint;

            public int X { get; set; }
            public int Y { get; set; }

            public int Width { get; set; }   // 0 для точки
            public int Height { get; set; }  // 0 для точки

            public override string ToString()
            {
                if (Kind == UiItemKind.ClickPoint)
                    return $"{Name} [{ProcessName}] ({X},{Y})";

                return $"{Name} [{ProcessName}] ({X},{Y}) {Width}x{Height}";
            }
        }

        public class Profile
        {
            public List<UiItem> Items { get; set; } = new List<UiItem>();
        }

        private void SaveProfile()
        {
            profile.Items = items.ToList();
            ProfileStore.Save(profile);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            string name = PromptName("Enter item name");
            if (string.IsNullOrWhiteSpace(name))
            {
                labelCoords.Text = "Add canceled";
                return;
            }

            capturedItemName = name;
            capturedProcessName = "";
            addState = AddCaptureState.WaitingProcess;

            labelCoords.Text = "Open target window and press CapsLock";
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is not UiItem item)
                return;

            item.Name = Prompt("Name", item.Name);
            item.ProcessName = Prompt("ProcessName", item.ProcessName);

            string kindStr = Prompt("Kind (0 ClickPoint, 1 ReadRegion)", ((int)item.Kind).ToString());
            if (int.TryParse(kindStr, out int k) && (k == 0 || k == 1))
                item.Kind = (UiItemKind)k;

            string xStr = Prompt("X", item.X.ToString());
            string yStr = Prompt("Y", item.Y.ToString());

            if (int.TryParse(xStr, out int x)) item.X = x;
            if (int.TryParse(yStr, out int y)) item.Y = y;

            if (item.Kind == UiItemKind.ReadRegion)
            {
                string wStr = Prompt("Width", item.Width.ToString());
                string hStr = Prompt("Height", item.Height.ToString());

                if (int.TryParse(wStr, out int w)) item.Width = w;
                if (int.TryParse(hStr, out int h)) item.Height = h;
            }
            else
            {
                item.Width = 0;
                item.Height = 0;
            }

            RefreshList();
            SaveProfile();
        }

        private static string PromptName(string title)
        {
            using var f = new Form();
            f.Text = title;
            f.Width = 400;
            f.Height = 140;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.StartPosition = FormStartPosition.CenterParent;
            f.MaximizeBox = false;
            f.MinimizeBox = false;

            var tb = new TextBox { Left = 12, Top = 12, Width = 360 };
            var ok = new Button { Text = "OK", Left = 216, Width = 75, Top = 45, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 297, Width = 75, Top = 45, DialogResult = DialogResult.Cancel };

            f.Controls.Add(tb);
            f.Controls.Add(ok);
            f.Controls.Add(cancel);

            f.AcceptButton = ok;
            f.CancelButton = cancel;

            return f.ShowDialog() == DialogResult.OK ? tb.Text.Trim() : "";
        }

        private string Prompt(string title, string defaultValue)
        {
            using var prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };
            var textLabel = new Label() { Left = 20, Top = 20, Text = title, Width = 340 };
            var textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
            var confirmation = new Button() { Text = "Ok", Left = 280, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : defaultValue;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is not UiItem item)
                return;
            items.Remove(item);
            SaveProfile();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ocrEngine?.Dispose();
            keyboardHook?.Dispose();
            watchTimer?.Stop();

            //base.OnFormClosed(e);
        }
        private bool SuppressKeyForCapture(int vk)
        {
            if (addState == AddCaptureState.Idle)
                return false;

            return vk == (int)Keys.CapsLock;
        }

        private void KeyboardHook_KeyDown(int vk)
        {
            if (vk != (int)Keys.CapsLock)
                return;

            if (addState == AddCaptureState.Idle)
                return;

            if (capsIsDown)
                return;

            capsIsDown = true;
            overlayStarted = false;

            capsHoldTimer.Stop();
            capsHoldTimer.Start();
        }

        private void KeyboardHook_KeyUp(int vk)
        {
            if (vk != (int)Keys.CapsLock)
                return;

            if (addState == AddCaptureState.Idle)
                return;

            capsHoldTimer.Stop();

            bool wasHold = overlayStarted;

            capsIsDown = false;

            if (wasHold)
                return;

            HandleCapsShortPress();
        }

        private void CapsHoldTimer_Tick(object sender, EventArgs e)
        {
            capsHoldTimer.Stop();

            if (!capsIsDown)
                return;

            overlayStarted = true;

            if (addState != AddCaptureState.WaitingLocation)
                return;

            BeginInvoke(new Action(() =>
            {
                using var overlay = new RegionSelectOverlay();
                var result = overlay.ShowDialog();

                if (result == DialogResult.OK)
                {
                    Rectangle r = overlay.SelectedRegion;
                    AddNewItemFromRegion(r);
                }
                else
                {
                    labelCoords.Text = "Region selection canceled";
                }
            }));
        }

        private void HandleCapsShortPress()
        {
            if (addState == AddCaptureState.WaitingProcess)
            {
                capturedProcessName = GetActiveProcessName();
                if (string.IsNullOrWhiteSpace(capturedProcessName))
                {
                    labelCoords.Text = "Cannot detect active process";
                    return;
                }

                addState = AddCaptureState.WaitingLocation;
                labelCoords.Text = $"Process: {capturedProcessName}. Press CapsLock for point or hold for region";
                return;
            }

            if (addState == AddCaptureState.WaitingLocation)
            {
                var pos = Cursor.Position;
                AddNewItemFromPoint(pos);
                return;
            }
        }
        private void AddNewItemFromPoint(Point pos)
        {
            var item = new UiItem
            {
                Name = capturedItemName,
                ProcessName = capturedProcessName,
                Kind = UiItemKind.ClickPoint,
                X = pos.X,
                Y = pos.Y,
                Width = 0,
                Height = 0
            };

            items.Add(item);
            listBox1.SelectedItem = item;
            listBox1.Refresh();
            SaveProfile();

            addState = AddCaptureState.Idle;
            labelCoords.Text = $"Added point: {item.Name}";
        }

        private void AddNewItemFromRegion(Rectangle r)
        {
            var item = new UiItem
            {
                Name = capturedItemName,
                ProcessName = capturedProcessName,
                Kind = UiItemKind.ReadRegion,
                X = r.X,
                Y = r.Y,
                Width = r.Width,
                Height = r.Height
            };

            items.Add(item);
            listBox1.SelectedItem = item;
            listBox1.Refresh();
            SaveProfile();

            addState = AddCaptureState.Idle;
            labelCoords.Text = $"Added region: {item.Name}";
        }

        private void RefreshList()
        {
            if (listBox1.DataSource == null)
            {
                listBox1.Refresh();
                return;
            }

            var cm = (CurrencyManager)BindingContext[listBox1.DataSource];
            cm.Refresh();
        }

        private void buttonShow_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is not UiItem item)
                return;

            var overlay = new PreviewOverlay(item, showMs: 900);
            overlay.Show();
        }

        private async Task buttonWatch_ClickAsync(object sender, EventArgs e)
        {
            if (!watchRunning)
            {
                watchRunning = true;
                watchCts = new CancellationTokenSource();
                watchTask = StartWatchAsync(watchCts.Token);
                labelCoords.Text = "Watch started";
                return;
            }

            watchRunning = false;
            watchCts.Cancel();

            try
            {
                await watchTask;
            }
            catch
            {
            }
            finally
            {
                watchCts.Dispose();
                watchCts = null;
                watchTask = null;
            }

            labelCoords.Text = "Watch stopped";
        }

        private UiItem FindRegionByName(string name)
        {
            foreach (var it in items)
            {
                if (it.Kind == UiItemKind.ReadRegion && it.Name == name)
                    return it;
            }
            return null;
        }

        private string ReadRegionText(UiItem regionItem)
        {
            var r = new Rectangle(regionItem.X, regionItem.Y, regionItem.Width, regionItem.Height);

            using var bmp = CaptureRegion(r);
            string raw = ReadText(bmp);

            raw = raw.Replace("\r", "").Replace("\n", "").Trim();
            return raw;
        }

        private void watchTimer_Tick(object sender, EventArgs e)
        {
            var r1 = FindRegionByName(watchRegionName1);
            var r2 = FindRegionByName(watchRegionName2);

            if (r1 == null || r2 == null)
            {
                labelAxis2Value.Text = r1 == null ? "Region1 not found" : labelAxis2Value.Text;
                labelAxis3Value.Text = r2 == null ? "Region2 not found" : labelAxis3Value.Text;
                return;
            }

            try
            {
                labelAxis2Value.Text = ReadRegionText(r1);
            }
            catch (Exception ex)
            {
                labelAxis2Value.Text = "OCR err";
            }

            try
            {
                labelAxis3Value.Text = ReadRegionText(r2);
            }
            catch (Exception ex)
            {
                labelAxis3Value.Text = "OCR err";
            }
        }

        private string ReadRegionTextThreadSafe(UiItem regionItem)
        {
            var r = new Rectangle(regionItem.X, regionItem.Y, regionItem.Width, regionItem.Height);

            using var bmp = CaptureRegion(r);

            string raw;
            lock (ocrLock)
            {
                raw = ReadText(bmp);
            }

            return raw.Replace("\r", "").Replace("\n", "").Trim();
        }

        private Task StartWatchAsync(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    UiItem r1 = null;
                    UiItem r2 = null;

                    BeginInvoke(new Action(() =>
                    {
                        r1 = FindRegionByName(watchRegionName1);
                        r2 = FindRegionByName(watchRegionName2);
                    }));

                    await Task.Delay(1, token);

                    string t1 = "";
                    string t2 = "";

                    if (r1 == null) t1 = "Region1 not found";
                    if (r2 == null) t2 = "Region2 not found";

                    if (r1 != null)
                    {
                        try { t1 = ReadRegionTextThreadSafe(r1); }
                        catch { t1 = "OCR err"; }
                    }

                    if (r2 != null)
                    {
                        try { t2 = ReadRegionTextThreadSafe(r2); }
                        catch { t2 = "OCR err"; }
                    }

                    BeginInvoke(new Action(() =>
                    {
                        labelAxis2Value.Text = t1;
                        labelAxis3Value.Text = t2;
                    }));

                    await Task.Delay(500, token);
                }
            }, token);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (watchRunning && watchCts != null)
            {
                watchRunning = false;
                watchCts.Cancel();
            }

            base.OnFormClosing(e);
        }

    }
}
