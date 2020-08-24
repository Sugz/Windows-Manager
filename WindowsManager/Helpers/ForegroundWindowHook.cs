using System;
using System.ComponentModel;

namespace WindowsManager.Helpers
{
    internal class ForegroundWindowHook
    {
        private readonly NativeMethods.WinEventDelegate _WinEventDelegate = null;
        private IntPtr _WinEventHook = IntPtr.Zero;

        internal event EventHandler<ForegroundWindowChangedEventArgs> ForegroundWindowChanged;

        internal ForegroundWindowHook()
        {
            _WinEventDelegate = new NativeMethods.WinEventDelegate(WinEventProc);
        }

        internal void Start()
        {
            _WinEventHook = NativeMethods.SetWinEventHook(
                NativeMethods.EVENT_SYSTEM_FOREGROUND,
                NativeMethods.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                _WinEventDelegate,
                0,
                0,
                NativeMethods.WINEVENT_OUTOFCONTEXT);
        }

        internal void Stop()
        {
            NativeMethods.UnhookWinEvent(_WinEventHook);
        }


        internal void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            EventHandler<ForegroundWindowChangedEventArgs> handler = ForegroundWindowChanged;
            handler?.Invoke(this, new ForegroundWindowChangedEventArgs(NativeMethods.GetForegroundWindow()));
        }


    }


    internal class ForegroundWindowChangedEventArgs : HandledEventArgs
    {
        public IntPtr Handle { get; }
        public string Title { get => NativeMethods.GetWindowTitle(Handle); }

        public ForegroundWindowChangedEventArgs(IntPtr handle)
        {
            Handle = handle;
        }
    }

}
