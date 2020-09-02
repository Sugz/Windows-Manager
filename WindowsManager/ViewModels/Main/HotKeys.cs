using GalaSoft.MvvmLight;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Imaging = System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsManager.Helpers;

namespace WindowsManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {

        #region Structs and Enums

        private struct ContainingRects
        {
            public bool IsNull;
            public int Screen;
            public int Rect;

            public ContainingRects(int screen, int rect)
            {
                IsNull = false;
                Screen = screen;
                Rect = rect;
            }

            public static ContainingRects Null => new ContainingRects { IsNull = true, Screen = -1, Rect = -1 };

        }

        private struct WindowRect
        {
            public Rect Rect;
            public NativeMethods.RECT Borders;

            public WindowRect(Rect rect, NativeMethods.RECT borders)
            {
                Rect = rect;
                Borders = borders;
            }

            public WindowRect(Rect rect)
            {
                Rect = rect;
                Borders = new NativeMethods.RECT
                {
                    Left = 0,
                    Top = 0,
                    Right = 0,
                    Bottom = 0
                };
            }
        }

        private enum Side
        {
            Left,
            Up,
            Right,
            Down
        }

        #endregion Structs and Enums


        #region Fields

        private bool _IsLoadingSettings = false;
        private int _MoveStep;
        private int _ResizeStep;
        private int _ExplorerWidth;
        private int _ExplorerHeight;
        private string _ImageViewerPath;


        #endregion Fields


        #region Properties

        public int MoveStep
        {
            get => _MoveStep;
            set
            {
                Set(ref _MoveStep, value);

                if (!_IsLoadingSettings)
                    _SettingsManager.MoveStep = _MoveStep;
            }
        }
        public int ResizeStep
        {
            get => _ResizeStep;
            set
            {
                Set(ref _ResizeStep, value);

                if (!_IsLoadingSettings)
                    _SettingsManager.ResizeStep = _ResizeStep;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int ExplorerWidth
        {
            get => _ExplorerWidth;
            set
            {
                Set(ref _ExplorerWidth, value);

                if (!_IsLoadingSettings)
                    _SettingsManager.ExplorerWidth = _ExplorerWidth;
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        public int ExplorerHeight
        {
            get => _ExplorerHeight;
            set
            {
                Set(ref _ExplorerHeight, value);

                if (!_IsLoadingSettings)
                    _SettingsManager.ExplorerHeight = _ExplorerHeight;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        public string ImageViewerPath
        {
            get => _ImageViewerPath;
            set
            {
                Set(ref _ImageViewerPath, value);

                if (!_IsLoadingSettings)
                    _SettingsManager.ImageViewerPath = _ImageViewerPath;
            }
        }


        #endregion Properties


        #region Initialisation

        private void LoadHotKeysSettings()
        {
            _IsLoadingSettings = true;

            MoveStep = _SettingsManager.MoveStep;
            ResizeStep = _SettingsManager.ResizeStep;
            ExplorerWidth = _SettingsManager.ExplorerWidth;
            ExplorerHeight = _SettingsManager.ExplorerHeight;
            ImageViewerPath = _SettingsManager.ImageViewerPath;

            _IsLoadingSettings = false;
        }

        private void CreateHotKeys()
        {

            #region Switch Monitor

            // Create a shortcut for each monitor
            string[] numbers = new[] { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth" };
            foreach (Screen screen in Screens)
            {
                int index = screen.Index;
                AddHotKey(new HotKey(
                    _IdGen.Next(),
                    $"Switch to {numbers[index]} monitor",
                    () => SwitchToScreen(index),
                    IsValidForegroundWindow,
                    (Key)75 + index,
                    ModifierKeys.Control,
                    $"Switch the current window to the {numbers[index]} monitor center area"
                    ));
            }

            #endregion Switch Monitor

            #region Maximize

            AddHotKey(new HotKey(
                _IdGen.Next(),
                "Maximize Window",
                () => SwitchToArea(),
                IsValidForegroundWindow,
                Key.Enter,
                ModifierKeys.Control,
                "Maximize the current window in the nearest area"
                ));

            #endregion Maximize

            #region Switch areas

            AddHotKey(new HotKey(
                _IdGen.Next(),
                "Switch to Previous Area",
                () => SwitchToArea(-1),
                IsValidForegroundWindow,
                Key.PageDown,
                ModifierKeys.Control,
                "Switch the current window to the previous area"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                "Switch to Next Area",
                () => SwitchToArea(1),
                IsValidForegroundWindow,
                Key.PageUp,
                ModifierKeys.Control,
                "Switch the current window to the next area"
                ));

            #endregion Switch areas

            #region Extend to area

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Extend Window to previous area",
                () => ExtendToArea(-1),
                IsValidForegroundWindow,
                Key.PageDown,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Extend the current window using the current and previous area"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Extend Window to next area",
                () => ExtendToArea(1),
                IsValidForegroundWindow,
                Key.PageUp,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Extend the current window using the current and next area"
                ));

            #endregion Extend to area

            #region Move

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Move Window Left",
                () => MoveWindow(Side.Left),
                IsValidForegroundWindow,
                Key.Left,
                ModifierKeys.Control,
                $"Move left the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Move Window Up",
                () => MoveWindow(Side.Up),
                IsValidForegroundWindow,
                Key.Up,
                ModifierKeys.Control,
                $"Move up the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Move Window Right",
                () => MoveWindow(Side.Right),
                IsValidForegroundWindow,
                Key.Right,
                ModifierKeys.Control,
                $"Move right the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Move Window Down",
                () => MoveWindow(Side.Down),
                IsValidForegroundWindow,
                Key.Down,
                ModifierKeys.Control,
                $"Move down the current window"
                ));

            #endregion Move

            #region Positive Resize

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Left",
                () => ResizeWindow(Side.Left, 1),
                IsValidForegroundWindow,
                Key.Left,
                ModifierKeys.Control | ModifierKeys.Shift,
                $"Positive Resize left the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Up",
                () => ResizeWindow(Side.Up, 1),
                IsValidForegroundWindow,
                Key.Up,
                ModifierKeys.Control | ModifierKeys.Shift,
                $"Positive Resize up the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Right",
                () => ResizeWindow(Side.Right, 1),
                IsValidForegroundWindow,
                Key.Right,
                ModifierKeys.Control | ModifierKeys.Shift,
                $"Positive Resize right the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Down",
                () => ResizeWindow(Side.Down, 1),
                IsValidForegroundWindow,
                Key.Down,
                ModifierKeys.Control | ModifierKeys.Shift,
                $"Positive Resize down the current window"
                ));

            #endregion Positive Resize

            #region Negative Resize

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Left",
                () => ResizeWindow(Side.Left, -1),
                IsValidForegroundWindow,
                Key.Left,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Positive Resize left the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Up",
                () => ResizeWindow(Side.Up, -1),
                IsValidForegroundWindow,
                Key.Up,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Positive Resize up the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Right",
                () => ResizeWindow(Side.Right, -1),
                IsValidForegroundWindow,
                Key.Right,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Positive Resize right the current window"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Positive Resize Window Down",
                () => ResizeWindow(Side.Down, -1),
                IsValidForegroundWindow,
                Key.Down,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Positive Resize down the current window"
                ));

            #endregion Negative Resize

            #region Explorer

            AddHotKey(new HotKey(
                _IdGen.Next(),
                "Explorer",
                GetExplorerWindow,
                Key.E,
                ModifierKeys.Control,
                "Set the Explorer window to the center of the screen"
                ));

            #endregion Explorer

            #region Screen Capture

            AddHotKey(new HotKey(
                _IdGen.Next(),
                "Explorer",
                CaptureScreen,
                Key.PrintScreen,
                ModifierKeys.None,
                "Set the Explorer window to the center of the screen"
                ));

            #endregion Screen Capture

        }

        #endregion Initialisation


        #region Methods and Handlers


        /// <summary>
        /// Check if the foreground window is valid (aka not fullscreen or desktop)
        /// </summary>
        /// <returns></returns>
        public bool IsValidForegroundWindow()
        {
            // go through simple test first
            if (_CurrentForegroundWindow != IntPtr.Zero &&
                !NativeMethods.IsDesktopWindow(_CurrentForegroundWindow) &&
                NativeMethods.GetWindowPlacement(_CurrentForegroundWindow) != NativeMethods.ShowWindowCommands.Maximized)
            {
                // some windows return a wrong rect, so we use CloseEnough to determine if the window is fullscreen
                NativeMethods.GetWindowRect(_CurrentForegroundWindow, out NativeMethods.RECT windowRect);
                Rect rect = windowRect.ToRect();
                ContainingRects rects = GetContainingRects(rect);
                Screen screen = Screens[rects.Screen];

                return !(screen.WorkingArea.CloseEnough(rect) || screen.Bounds.CloseEnough(rect));
            }

            return false;
        }


        /// <summary>
        /// Get a window Rect and invisible borders
        /// </summary>
        /// <returns></returns>
        private WindowRect GetWindowSize()
        {
            //GetWindowRect doesn't always return the correct RECT
            //DwmGetWindowAttribute can sometimes give an indication wheter the window have invisible borders

            NativeMethods.GetWindowRect(_CurrentForegroundWindow, out NativeMethods.RECT windowRect);

            if (Environment.OSVersion.Version.Major < 6)
                return new WindowRect(windowRect.ToRect());

            int size = Marshal.SizeOf(typeof(NativeMethods.RECT));
            NativeMethods.DwmGetWindowAttribute(_CurrentForegroundWindow, NativeMethods.DwmWindowAttribute.ExtendedFrameBounds, out NativeMethods.RECT frame, size);

            NativeMethods.RECT borders;
            borders.Left = frame.Left - windowRect.Left;
            borders.Top = frame.Top - windowRect.Top;
            borders.Right = windowRect.Right - frame.Right;
            borders.Bottom = windowRect.Bottom - frame.Bottom;

            return new WindowRect(windowRect.ToRect(), borders);
        }


        /// <summary>
        /// Get which screen and screen area contain a given rect
        /// </summary>
        /// <param name="wndRect"></param>
        /// <returns></returns>
        private ContainingRects GetContainingRects(Rect wndRect)
        {
            List<Rect> screensRect = Screens.Select(x => x.WorkingArea).ToList();

            int screenIndex = wndRect.GetMostIntersectingRectIndex(screensRect);
            Screen screen = Screens[screenIndex];
            wndRect.X -= screen.WorkingArea.X;
            wndRect.Y -= screen.WorkingArea.Y;

            int rectIndex = wndRect.GetMostIntersectingRectIndex(screen.Rects);

            return new ContainingRects(screenIndex, rectIndex);
        }


        private void SetWindowInScreenArea(NativeMethods.RECT borders, int screenIndex, int rectIndex = 1)
        {
            // allow windows to navigate through screens
            if (rectIndex < 0)
            {
                if (screenIndex == 0)
                    return;
                screenIndex--;
                rectIndex = Screens[screenIndex].Rects.Length - 1;
            }
            else if (rectIndex >= Screens[screenIndex].Rects.Length)
            {
                if (screenIndex == Screens.Count - 1)
                    return;
                screenIndex++;
                rectIndex = 0;
            }

            Screen screen = Screens[screenIndex];
            Rect workingArea = screen.WorkingArea;
            Rect rect = screen.Rects[rectIndex];

            NativeMethods.SetWindowPos(
                _CurrentForegroundWindow,
                IntPtr.Zero,
                (int)(workingArea.X + rect.X - borders.Left),
                (int)(workingArea.Y + rect.Y - borders.Top),
                (int)(rect.Width + borders.Left + borders.Right),
                (int)(rect.Height + borders.Top + borders.Bottom),
                Convert.ToUInt32(NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.ShowWindow));
        }


        private void SwitchToScreen(int index)
        {
            WindowRect windowRect = GetWindowSize();
            SetWindowInScreenArea(windowRect.Borders, index);
        }


        private void SwitchToArea(int index = 0)
        {
            WindowRect windowRect = GetWindowSize();
            ContainingRects indexes = GetContainingRects(windowRect.Rect);
            if (!indexes.IsNull)
                SetWindowInScreenArea(windowRect.Borders, indexes.Screen, indexes.Rect + index);
        }


        private void ExtendToArea(int index)
        {
            WindowRect windowRect = GetWindowSize();
            ContainingRects indexes = GetContainingRects(windowRect.Rect);
            if (indexes.IsNull)
                return;

            Screen screen = Screens[indexes.Screen];

            if (indexes.Rect == 0 && index == -1)
                return;

            if (indexes.Rect == screen.Rects.Length - 1 && index == 1)
                return;

            Rect rect = new Rect();
            Rect currentRect = screen.Rects[indexes.Rect];
            Rect targetRect = screen.Rects[indexes.Rect + index];
            Rect workingArea = screen.WorkingArea;
            NativeMethods.RECT borders = windowRect.Borders;

            rect = rect.SetTopLeft(index == -1 ? targetRect.TopLeft : currentRect.TopLeft);
            switch (screen.Orientation)
            {
                case Orientation.Horizontal:
                    rect.Width = currentRect.Width + targetRect.Width;
                    rect.Height = currentRect.Height;
                    break;
                case Orientation.Vertical:
                    rect.Width = currentRect.Width;
                    rect.Height = currentRect.Height + targetRect.Height;
                    break;
            }

            NativeMethods.SetWindowPos(
                _CurrentForegroundWindow,
                IntPtr.Zero,
                (int)(workingArea.X + rect.X - borders.Left),
                (int)(workingArea.Y + rect.Y - borders.Top),
                (int)(rect.Width + borders.Left + borders.Right),
                (int)(rect.Height + borders.Top + borders.Bottom),
                Convert.ToUInt32(NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.ShowWindow));
        }


        private void MoveWindow(Side side)
        {
            NativeMethods.GetWindowRect(_CurrentForegroundWindow, out NativeMethods.RECT windowRect);
            Rect rect = windowRect.ToRect();
            switch (side)
            {
                case Side.Left:
                    rect.X -= _MoveStep;
                    break;
                case Side.Up:
                    rect.Y -= _MoveStep;
                    break;
                case Side.Right:
                    rect.X += _MoveStep;
                    break;
                case Side.Down:
                    rect.Y += _MoveStep;
                    break;
            }

            NativeMethods.SetWindowPos(
                _CurrentForegroundWindow,
                IntPtr.Zero,
                (int)rect.X,
                (int)rect.Y,
                0,
                0,
                Convert.ToUInt32(NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.ShowWindow | NativeMethods.SetWindowPosFlags.IgnoreResize));
        }


        private void ResizeWindow(Side side, int multiplier)
        {
            NativeMethods.GetWindowRect(_CurrentForegroundWindow, out NativeMethods.RECT windowRect);
            Rect rect = windowRect.ToRect();

            switch (side)
            {
                case Side.Left:
                    rect.X -= _ResizeStep * multiplier;
                    goto case Side.Right;
                case Side.Right:
                    rect.Width += _ResizeStep * multiplier;
                    break;
                case Side.Up:
                    rect.Y -= _ResizeStep * multiplier;
                    goto case Side.Down;
                case Side.Down:
                    rect.Height += _ResizeStep * multiplier;
                    break;
            }

            NativeMethods.SetWindowPos(
                _CurrentForegroundWindow,
                IntPtr.Zero,
                (int)rect.X,
                (int)rect.Y,
                (int)rect.Width,
                (int)rect.Height,
                Convert.ToUInt32(NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.ShowWindow));
        }


        private void GetExplorerWindow()
        {
            ShellWindows shellWindows = new ShellWindows();

            if (shellWindows.Count == 0)
            {
                Process.Start("explorer.exe");
                int counter = 0, maxCounter = 30;
                while (shellWindows.Count == 0)
                {
                    Thread.Sleep(100);
                    shellWindows = new ShellWindows();
                    if (++counter >= maxCounter)
                        return;
                }
            }

            foreach (InternetExplorer ie in shellWindows)
            {
                string filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();
                if (filename.Equals("explorer"))
                {
                    IntPtr explorerHwnd = new IntPtr(ie.HWND);
                    SetWindowInFront(explorerHwnd, ExplorerWidth, ExplorerHeight);
                    return;
                }
            }


        }


        private void SetWindowInFront(IntPtr hwnd, int? width = null, int? height = null)
        {
            //get the window size, then move it to the center of the first screen
            if (!NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rct))
                return;

            Rect rect = rct.ToRect();
            int left, top;

            NativeMethods.SetWindowPosFlags flags = NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.ShowWindow;
            if (width is null || height is null)
            {
                flags |= NativeMethods.SetWindowPosFlags.IgnoreResize;
                left = (int)((Screens[0].WorkingArea.Width / 2) - (rect.Width / 2));
                top = (int)((Screens[0].WorkingArea.Height / 2) - (rect.Height / 2));
                width = 0;
                height = 0;
            }
            else
            {
                left = (int)((Screens[0].WorkingArea.Width / 2) - (width / 2));
                top = (int)((Screens[0].WorkingArea.Height / 2) - (height / 2));
            }


            NativeMethods.ShowWindow(hwnd, NativeMethods.WindowShowStyle.Restore);

            NativeMethods.SetWindowPos(
                hwnd,
                NativeMethods.HWND_TOPMOST,
                left,
                top,
                width.Value,
                height.Value,
                Convert.ToUInt32(flags));

            NativeMethods.SetForegroundWindow(hwnd);
        }

        #endregion Methods and Handlers


        #region Screen Capture

        private BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                Imaging.ImageLockMode.ReadWrite,
                Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }


        private void CaptureScreen()
        {
            Bitmap bmp = new Bitmap(_ScreensBounds.Width, _ScreensBounds.Height, Imaging.PixelFormat.Format32bppArgb);

            using Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(_ScreensBounds.Location, System.Drawing.Point.Empty, bmp.Size);
            Clipboard.SetImage(CreateBitmapSourceFromGdiBitmap(bmp));

            if (_ImageViewerPath != null)
                Process.Start(_ImageViewerPath, "-sc -bg");
        } 

        #endregion Screen Capture

    }
}
