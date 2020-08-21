using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WindowsManager.Views.Controls
{
    public class DistanceChangedEventArgs : EventArgs
    {
        public int OldValue { get; private set; }
        public int NewValue { get; private set; }

        public int Delta => NewValue - OldValue;

        public DistanceChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }


    public enum Position
    {
        Side,
        Center
    }

    public class DistanceIndicator : Control
    {

        private bool ExternalSizeChange = false;
        private Border _HorizontalLine;
        private Border _VerticalLine;


        #region Distance

        /// <summary>
        /// 
        /// </summary>
        public int Distance
        {
            get => (int)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        // DependencyProperty for Size
        public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
            "Distance",
            typeof(int),
            typeof(DistanceIndicator),
            new FrameworkPropertyMetadata(0, OnDistanceChanged)//, OnCoerceSize)
        );

        //private static object OnCoerceSize(DependencyObject d, object baseValue)
        //{
        //    return baseValue;
        //}

        private static void OnDistanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DistanceIndicator control = (DistanceIndicator)d;
            if (!control.ExternalSizeChange)
            {
                int oldValue = (int)e.OldValue;
                int newValue = (int)e.NewValue;
                EventHandler<DistanceChangedEventArgs> handler = control.DistanceChanged;
                handler?.Invoke(control, new DistanceChangedEventArgs(oldValue, newValue));
            }
        }

        #endregion Distance 


        #region Orientation

        /// <summary>
        /// 
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        // DependencyProperty for Orientation
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(DistanceIndicator),
            new FrameworkPropertyMetadata(Orientation.Horizontal, OnOrientationChanged)//, OnCoerceOrientation)
        );

        //private static object OnCoerceOrientation(DependencyObject d, object baseValue)
        //{
        //    return baseValue;
        //}

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DistanceIndicator control = (DistanceIndicator)d;
            control.SetLines();
        }

        #endregion Orientation 


        #region Position

        /// <summary>
        /// 
        /// </summary>
        public Position Position
        {
            get => (Position)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        // DependencyProperty for Position
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position",
            typeof(Position),
            typeof(DistanceIndicator),
            new FrameworkPropertyMetadata(Position.Side)
        );

        #endregion Position 



        public event EventHandler<DistanceChangedEventArgs> DistanceChanged;


        public DistanceIndicator()
        {
            Loaded += (s, e) => SetLines();
            SizeChanged += OnSizeChanged;
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    SetDistance((int)ActualHeight);
                    break;
                case Orientation.Vertical:
                    SetDistance((int)ActualWidth);
                    break;
            }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("HorizontalLine") is Border hline)
                _HorizontalLine = hline;
            if (GetTemplateChild("VerticalLine") is Border vline)
                _VerticalLine = vline;
        }


        public void SetDistance(int distance)
        {
            ExternalSizeChange = true;
            Distance = distance;
            ExternalSizeChange = false;
        }


        private void SetLines()
        {
            if (_HorizontalLine != null && _VerticalLine != null)
            {
                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        _HorizontalLine.Visibility = Visibility.Collapsed;
                        _VerticalLine.Visibility = Visibility.Visible;
                        break;
                    case Orientation.Vertical:
                        _HorizontalLine.Visibility = Visibility.Visible;
                        _VerticalLine.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

    }
}
