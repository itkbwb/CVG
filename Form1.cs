using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CVG
{
    public partial class Form1 : Form
    {
        // ¬¬¬à¬à¬â¬Õ¬Ú¬ß¬Ñ¬ä¬í ¬Ü¬Ý¬Ú¬Ü¬Ñ ¬á¬à ¬ï¬Ü¬â¬Ñ¬ß¬å
        // ¬©¬Õ¬Ö¬ã¬î ¬á¬â¬à¬ã¬ä¬à ¬á¬â¬Ú¬Þ¬Ö¬â, ¬á¬à¬Õ¬Ô¬à¬ß¬Ú ¬á¬à¬Õ ¬ã¬Ó¬à¬Û ¬Þ¬à¬ß¬Ú¬ä¬à¬â
        private const int TargetX = 50;
        private const int TargetY = 1050;

        public Form1()
        {
            InitializeComponent();

            // ¬Ö¬ã¬Ý¬Ú ¬à¬Ò¬â¬Ñ¬Ò¬à¬ä¬é¬Ú¬Ü ¬ß¬Ö ¬á¬â¬Ú¬Ó¬ñ¬Ù¬Ñ¬ß ¬é¬Ö¬â¬Ö¬Ù ¬Õ¬Ú¬Ù¬Ñ¬Û¬ß¬Ö¬â
            // buttonMacro.Click += buttonMacro_Click;
        }

        // ¬ª¬Þ¬á¬à¬â¬ä¬í WinAPI ¬Õ¬Ý¬ñ ¬Þ¬í¬ê¬Ú ¬Ú ¬Ñ¬Ü¬ä¬Ú¬Ó¬Ñ¬è¬Ú¬Ú ¬à¬Ü¬ß¬Ñ
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        
        private static void ClickAt(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, UIntPtr.Zero);
        }

        private static void ActivateVisualStudio()
        {
            // ¬ª¬Þ¬ñ ¬á¬â¬à¬è¬Ö¬ã¬ã¬Ñ VS ¬à¬Ò¬í¬é¬ß¬à devenv
            Process[] vs = Process.GetProcessesByName("devenv");
            if (vs.Length == 0)
                return;

            IntPtr handle = vs[0].MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. ¬¬¬Ý¬Ú¬Ü ¬á¬à ¬Ù¬Ñ¬Õ¬Ñ¬ß¬ß¬í¬Þ ¬Ü¬à¬à¬â¬Õ¬Ú¬ß¬Ñ¬ä¬Ñ¬Þ
            ClickAt(TargetX, TargetY);
            Thread.Sleep(200);

            // 2. ¬£¬Ó¬à¬Õ ¬ä¬Ö¬Ü¬ã¬ä¬Ñ ¬Ú Enter
            // ¬£¬Ñ¬Ø¬ß¬à ¬é¬ä¬à¬Ò¬í ¬Ò¬í¬Ý¬Ñ ¬Ó¬Ü¬Ý¬ð¬é¬Ö¬ß¬Ñ ¬Ü¬à¬â¬Ö¬Û¬ã¬Ü¬Ñ¬ñ ¬â¬Ñ¬ã¬Ü¬Ý¬Ñ¬Õ¬Ü¬Ñ ¬Ú¬Ý¬Ú IME
            SendKeys.SendWait("camera");
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(500);

            // 3. ¬±¬Ö¬â¬Ö¬Ü¬Ý¬ð¬é¬Ö¬ß¬Ú¬Ö ¬ß¬Ñ ¬à¬Ü¬ß¬à Visual Studio
            ActivateVisualStudio();
        }
    }
}
