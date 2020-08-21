using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using System.Diagnostics;
using WindowsManager.Views;
using System.Linq;
using System.Xml.Linq;

namespace WindowsManager.Helpers
{
    public class Screen
    {

        #region Fields

        private Forms.Screen _Screen;

        private readonly Orientation _Orientation;

        private SplitWindow _SplitWindow = null;

        #endregion Fields


        #region Properties

        public Rect WorkingArea { get; private set; }

        public int Index { get; private set; }

        public Rect[] Rects { get; private set; } = new Rect[3];

        #endregion Properties


        #region Events

        public event EventHandler SplitWindowHide;

        #endregion Events


        #region Constructor

        public Screen(Forms.Screen screen, int index, bool settingsExist)
        {
            _Screen = screen;
            Index = index;
            WorkingArea = new Rect(_Screen.WorkingArea.X, _Screen.WorkingArea.Y, _Screen.WorkingArea.Width, _Screen.WorkingArea.Height);
            _Orientation = WorkingArea.Width >= WorkingArea.Height ? Orientation.Horizontal : Orientation.Vertical;

            if (!settingsExist)
            {
                switch (_Orientation)
                {
                    case Orientation.Horizontal:
                        double width = WorkingArea.Width / 3;
                        double widthFloatingPart = width - Math.Truncate(width);

                        double[] widths;
                        if (widthFloatingPart < 0.5)
                            widths = new[] { Math.Floor(width), Math.Ceiling(width), Math.Floor(width), };
                        else
                            widths = new[] { Math.Ceiling(width), Math.Ceiling(width), Math.Floor(width), };

                        for (int i = 0; i < 3; i++)
                            Rects[i] = new Rect(0, 0, widths[i], WorkingArea.Height);

                        break;

                    case Orientation.Vertical:
                        double height = WorkingArea.Height / 3;
                        double heightFloatingPart = height - Math.Truncate(height);

                        double[] heights;
                        if (heightFloatingPart < 0.5)
                            heights = new[] { Math.Floor(height), Math.Ceiling(height), Math.Floor(height), };
                        else
                            heights = new[] { Math.Ceiling(height), Math.Ceiling(height), Math.Floor(height), };

                        for (int i = 0; i < 3; i++)
                            Rects[i] = new Rect(0, 0, WorkingArea.Width, heights[i]);

                        break;
                }
            }
        }

        #endregion Constructor


        #region Public Methods

        public void ShowSplitters()
        {
            if (_SplitWindow is null)
            {
                _SplitWindow = new SplitWindow
                {
                    Left = WorkingArea.X,
                    Top = WorkingArea.Y,
                    Width = WorkingArea.Width,
                    Height = WorkingArea.Height,
                    Orientation = _Orientation,
                    Rects = Rects
                };

                _SplitWindow.EscapePressed += (s, e) =>
                {
                    EventHandler handler = SplitWindowHide;
                    handler?.Invoke(this, EventArgs.Empty);
                };
            }

            _SplitWindow.Show();
        }


        public void HideSplitters()
        {
            _SplitWindow?.Hide();
            Rects = _SplitWindow.Rects;
        }


        public XElement Serialize()
        {
            XElement xElement = new XElement("Screen",
                new XAttribute("Index", Index));

            foreach (Rect rect in Rects)
                xElement.Add(new XElement("rect", rect));

            return xElement;
        }


        #endregion Public Methods

    }
}
