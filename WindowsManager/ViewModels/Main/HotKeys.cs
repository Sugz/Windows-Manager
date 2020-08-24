using GalaSoft.MvvmLight;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsManager.Helpers;

namespace WindowsManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
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



        public int MoveStep { get; set; } = 5;
        public int ResizeStep { get; set; } = 5;


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
                    () => _CurrentForegroundWindow != IntPtr.Zero,
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
                () => _CurrentForegroundWindow != IntPtr.Zero,
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
                () => _CurrentForegroundWindow != IntPtr.Zero,
                Key.PageDown,
                ModifierKeys.Control,
                "Switch the current window to the previous area"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                "Switch to Next Area",
                () => SwitchToArea(1),
                () => _CurrentForegroundWindow != IntPtr.Zero,
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
                () => _CurrentForegroundWindow != IntPtr.Zero,
                Key.PageDown,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Extend the current window using the current and previous area"
                ));

            AddHotKey(new HotKey(
                _IdGen.Next(),
                $"Extend Window to next area",
                () => ExtendToArea(1),
                () => _CurrentForegroundWindow != IntPtr.Zero,
                Key.PageUp,
                ModifierKeys.Control | ModifierKeys.Alt,
                $"Extend the current window using the current and next area"
                ));

            #endregion Extend to area

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
            
        }




        private WindowRect GetWindowBorders()
        {
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
            WindowRect windowRect = GetWindowBorders();
            SetWindowInScreenArea(windowRect.Borders, index);
        }


        private void SwitchToArea(int index = 0)
        {
            WindowRect windowRect = GetWindowBorders();
            ContainingRects indexes = GetContainingRects(windowRect.Rect);
            if (!indexes.IsNull)
                SetWindowInScreenArea(windowRect.Borders, indexes.Screen, indexes.Rect + index);
        }


        private void ExtendToArea(int index)
        {
            WindowRect windowRect = GetWindowBorders();
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
            switch(screen.Orientation)
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
                    SetWindowInFront(explorerHwnd);
                    return;
                }
            }


        }


        private void SetWindowInFront(IntPtr hwnd)
        {
            //get the window size, then move it to the center of the first screen
            if (!NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rct))
                return;

            int wndWidth = rct.Right - rct.Left;
            int wndHeight = rct.Bottom - rct.Top;

            int left = (int)(Screens[0].WorkingArea.X + (Screens[0].WorkingArea.Width / 2) - (wndWidth / 2));
            int top = (int)(Screens[0].WorkingArea.Y + (Screens[0].WorkingArea.Height / 2) - (wndHeight / 2));

            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, left, top, 0, 0,
                    Convert.ToUInt32(NativeMethods.SetWindowPosFlags.IgnoreZOrder | NativeMethods.SetWindowPosFlags.IgnoreResize | NativeMethods.SetWindowPosFlags.ShowWindow));

            NativeMethods.SetForegroundWindow(hwnd);
        }
    }
}
