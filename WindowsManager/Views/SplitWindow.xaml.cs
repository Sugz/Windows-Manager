using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WindowsManager.Views.Controls;

namespace WindowsManager.Views
{
    /// <summary>
    /// Interaction logic for SplitWindow.xaml
    /// </summary>
    public partial class SplitWindow : Window
    {

        #region Properties

        public Orientation Orientation { get; set; }

        public Rect[] Rects
        {
            get
            {
                Rect[] rects = new Rect[3];

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        double x = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            double width = Grid.ColumnDefinitions[i * 2].Width.Value + 3;
                            rects[i] = new Rect(x, 0, width, ActualHeight);
                            x += width;
                        }
                        rects[1].Width += 3;
                        rects[2].X += 3;
                        break;
                    case Orientation.Vertical:
                        double y = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            double height = Grid.RowDefinitions[i * 2].Height.Value + 3;
                            rects[i] = new Rect(0, y, ActualWidth, height);
                            y += height;
                        }
                        rects[1].Height += 3;
                        rects[2].Y += 3;
                        break;
                }

                return rects;
            }
            set
            {
                CleanGrid();

                Rect[] rects = value;
                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rects[0].Width - 3) });
                        Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
                        Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rects[1].Width - 6) });
                        Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
                        Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rects[2].Width - 3) });

                        Grid.SetColumn(FirstSplitter, 1);
                        Grid.SetColumn(SecondSplitter, 3);
                        Grid.SetColumn(FirstIndicator, 0);
                        Grid.SetColumn(SecondIndicator, 2);
                        Grid.SetColumn(ThirdIndicator, 4);

                        FirstIndicator.Orientation = Orientation.Vertical;
                        SecondIndicator.Orientation = Orientation.Vertical;
                        ThirdIndicator.Orientation = Orientation.Vertical;

                        break;

                    case Orientation.Vertical:
                        Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rects[0].Height - 3) });
                        Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6) });
                        Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rects[1].Height - 6) });
                        Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6) });
                        Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rects[2].Height - 3) });

                        Grid.SetRow(FirstSplitter, 1);
                        Grid.SetRow(SecondSplitter, 3);
                        Grid.SetRow(FirstIndicator, 0);
                        Grid.SetRow(SecondIndicator, 2);
                        Grid.SetRow(ThirdIndicator, 4);

                        FirstIndicator.Orientation = Orientation.Horizontal;
                        SecondIndicator.Orientation = Orientation.Horizontal;
                        ThirdIndicator.Orientation = Orientation.Horizontal;

                        break;
                }


            }
        }

        #endregion Properties


        #region Events

        public event EventHandler EscapePressed;

        #endregion Events


        #region Constructor

        public SplitWindow()
        {
            InitializeComponent();
        }

        #endregion Constructor


        #region Events Handlers

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                EventHandler handler = EscapePressed;
                handler?.Invoke(this, e);
                e.Handled = true;
            }
        }

        private void OnSplitterDragDelta(object sender, DragDeltaEventArgs e)
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    if (sender == FirstSplitter)
                        Grid.ColumnDefinitions[2].Width = new GridLength(Grid.ColumnDefinitions[2].Width.Value - e.HorizontalChange);
                    else
                        Grid.ColumnDefinitions[4].Width = new GridLength(Grid.ColumnDefinitions[4].Width.Value - e.HorizontalChange);

                    break;

                case Orientation.Vertical:
                    if (sender == FirstSplitter)
                        Grid.RowDefinitions[2].Height = new GridLength(Grid.RowDefinitions[2].Height.Value - e.VerticalChange);
                    else
                        Grid.RowDefinitions[4].Height = new GridLength(Grid.RowDefinitions[4].Height.Value - e.VerticalChange);

                    break;
            }

        }

        private void OnIndicatorDistanceChanged(object sender, DistanceChangedEventArgs e)
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    if (sender == FirstIndicator)
                    {
                        Grid.ColumnDefinitions[0].Width = new GridLength(e.NewValue);
                        Grid.ColumnDefinitions[2].Width = new GridLength(Grid.ColumnDefinitions[2].Width.Value - e.Delta);
                    }
                    else if (sender == SecondIndicator)
                    {
                        Grid.ColumnDefinitions[0].Width = new GridLength(Grid.ColumnDefinitions[0].Width.Value - Math.Ceiling(e.Delta / 2.0));
                        Grid.ColumnDefinitions[4].Width = new GridLength(Grid.ColumnDefinitions[4].Width.Value - Math.Floor(e.Delta / 2.0));
                        Grid.ColumnDefinitions[2].Width = new GridLength(e.NewValue);
                    }
                    else
                    {
                        Grid.ColumnDefinitions[2].Width = new GridLength(Grid.ColumnDefinitions[2].Width.Value - e.Delta);
                        Grid.ColumnDefinitions[4].Width = new GridLength(e.NewValue);
                    }
                    break;
                case Orientation.Vertical:
                    if (sender == FirstIndicator)
                    {
                        Grid.RowDefinitions[0].Height = new GridLength(e.NewValue);
                        Grid.RowDefinitions[2].Height = new GridLength(Grid.RowDefinitions[2].Height.Value - e.Delta);
                    }
                    else if (sender == SecondIndicator)
                    {
                        Grid.RowDefinitions[0].Height = new GridLength(Grid.RowDefinitions[0].Height.Value - Math.Ceiling(e.Delta / 2.0));
                        Grid.RowDefinitions[4].Height = new GridLength(Grid.RowDefinitions[4].Height.Value - Math.Floor(e.Delta / 2.0));
                        Grid.RowDefinitions[2].Height = new GridLength(e.NewValue);
                    }
                    else
                    {
                        Grid.RowDefinitions[2].Height = new GridLength(Grid.RowDefinitions[2].Height.Value - e.Delta);
                        Grid.RowDefinitions[4].Height = new GridLength(e.NewValue);
                    }
                    break;
            }
        }

        private void OnDistanceInputKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FirstSplitter.Focus();
        }

        #endregion Events Handlers


        #region Private Methods

        private void CleanGrid()
        {
            Grid.ColumnDefinitions.Clear();
            Grid.RowDefinitions.Clear();

            Grid.SetColumn(FirstSplitter, 0);
            Grid.SetColumn(SecondSplitter, 0);
            Grid.SetColumn(FirstIndicator, 0);
            Grid.SetColumn(SecondIndicator, 0);
            Grid.SetColumn(ThirdIndicator, 0);

            Grid.SetRow(FirstSplitter, 0);
            Grid.SetRow(SecondSplitter, 0);
            Grid.SetRow(FirstIndicator, 0);
            Grid.SetRow(SecondIndicator, 0);
            Grid.SetRow(ThirdIndicator, 0);
        }

        #endregion Private Methods


        
    }
}
