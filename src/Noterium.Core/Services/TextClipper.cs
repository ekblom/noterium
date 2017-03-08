using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using log4net;
using Shortcut;

namespace Noterium.Core.Services
{
    public class TextClipper
    {
        public delegate void TextClippedEvent(string windowTitle, string text);

        private const int MOD_NOREPEAT = 0x4000;
        public const int C_KEY = 0x43;
        public static int MOD_ALT = 0x1;
        public static int MOD_CONTROL = 0x2;
        public static int MOD_SHIFT = 0x4;
        public static int MOD_WIN = 0x8;
        public static int WM_HOTKEY = 0x312;
        public static int HOTKEY_ID = MOD_CONTROL + MOD_SHIFT;
        private static int keyId = "Noterium hotkey".GetHashCode();

        private readonly HotkeyBinder _hotkeyBinder = new HotkeyBinder();
        private readonly ILog _log = LogManager.GetLogger(typeof (TextClipper));
        private Hotkey _hotkeyCombination;
        private DateTime _lastHotkey = DateTime.MinValue;
        private readonly object _lastHotkeyLocker = new object();

        private IntPtr _owner;
        public event TextClippedEvent OnTextClipped;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public void Init(IntPtr owner)
        {
            _owner = owner;

            _hotkeyCombination = new Hotkey(Modifiers.Control | Modifiers.Shift, Keys.C);

            if (_hotkeyBinder.IsHotkeyAlreadyBound(_hotkeyCombination))
                return;

            _hotkeyBinder.Bind(_hotkeyCombination).To(HotkeyCallback);
            Application.ApplicationExit += Application_ApplicationExit;
        }

        private void HotkeyCallback()
        {
            _log.Debug("HotkeyCallback " + DateTime.Now);

            lock (_lastHotkeyLocker)
            {
                if (_lastHotkey != DateTime.MinValue)
                {
                    var span = DateTime.Now - _lastHotkey;
                    if (span.TotalMilliseconds < 1000)
                        return;
                }
            }

            if (OnTextClipped != null)
            {
                lock (_lastHotkeyLocker)
                {
                    _lastHotkey = DateTime.Now;
                }
                var text = GetActiveWindowsTextSelection();
                var winTitle = GetActiveWindowTitle();
                OnTextClipped(winTitle, text);
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            _hotkeyBinder.Unbind(_hotkeyCombination);
        }

        private void SendCtrlC(IntPtr hWnd)
        {
            uint KEYEVENTF_KEYUP = 2;
            byte VK_CONTROL = 0x11;
            SetForegroundWindow(hWnd);
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(C_KEY, 0, 0, 0); //Send the C key (43 is "C")
            keybd_event(C_KEY, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0); // 'Left Control Up
        }

        private string GetActiveWindowsTextSelection()
        {
            // Obtain the handle of the active window.
            //IntPtr handle = GetForegroundWindow();
            //SendCtrlC(handle);

            if (Clipboard.ContainsData(DataFormats.Html))
            {
                var html = (string) Clipboard.GetData(DataFormats.Html);

                var output = ClipboardHtmlHelper.ParseString(html);

                if (!string.IsNullOrWhiteSpace(output.Source))
                {
                    return string.Format("<a href=\"{0}\">{0}</a><br /><br />{1}", output.Source, output.Html);
                }
                return output.Html;
            }

            return Clipboard.GetText();
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var Buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
    }
}