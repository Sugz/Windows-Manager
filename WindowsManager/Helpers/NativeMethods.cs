using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsManager.Helpers
{
    internal static class NativeMethods
    {
        #region HotKey Interop

        internal const int WM_HOTKEY = 0x0312;

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        internal static extern int RegisterHotKey(IntPtr hwnd, int id, int modifiers, int key);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        internal static extern int UnregisterHotKey(IntPtr hwnd, int id);

        #endregion


        #region Window Interop

        internal const uint WINEVENT_OUTOFCONTEXT = 0;
        internal const uint EVENT_SYSTEM_FOREGROUND = 3;
        internal const int nChars = 256;
        internal static IntPtr HWND_TOPMOST = new IntPtr(-1);

        [Flags()]
        internal enum SetWindowPosFlags : uint
        {
            IgnoreResize = 0x0001,
            IgnoreMove = 0x0002,
            IgnoreZOrder = 0x0004,
            DoNotRedraw = 0x0008,
            DoNotActivate = 0x0010,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            ShowWindow = 0x0040,
            HideWindow = 0x0080,
            DoNotCopyBits = 0x0100,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotReposition = 0x0200,
            DoNotSendChangingEvent = 0x0400,
            DeferErase = 0x2000,
            SynchronousWindowPosition = 0x4000,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        internal static string GetWindowTitle(IntPtr handle)
        {
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(handle, Buff, nChars) > 0)
                return Buff.ToString();

            return null;
        }


        [DllImport("user32.dll")]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);


        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        internal static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        #endregion Window Interop
    }
}
