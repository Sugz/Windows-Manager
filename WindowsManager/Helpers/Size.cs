using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsManager.Helpers
{
    public class Size : ViewModelBase
    {
        private int _Width;
        private int _Height;


        public int Width 
        {
            get => _Width;
            set => Set(ref _Width, value);
        }

        public int Height
        {
            get => _Height;
            set => Set(ref _Height, value);
        }


        public bool IsNull { get; set; }

        public Size() { }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
            IsNull = false;
        }

        public static Size Null => new Size { Width = 0, Height = 0, IsNull = true };
    }
}
