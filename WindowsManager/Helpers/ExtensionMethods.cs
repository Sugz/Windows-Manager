using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace WindowsManager.Helpers
{
    internal static class ExtensionMethods
    {
        internal static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        internal static int GetMostIntersectingRectIndex(this Rect rect, IList<Rect> testRects)
        {
            int[] areas = new int[testRects.Count];
            for (int i = 0; i < testRects.Count; i++)
            {
                Rect intersection = Rect.Intersect(testRects[i], rect);
                areas[i] = intersection.IsEmpty ? 0 : (int)(intersection.Width * intersection.Height);
            }

            return Array.IndexOf(areas, areas.Max());
        }


        internal static Rect SetTopLeft(this Rect rect, Point point)
        {
            rect.X = point.X;
            rect.Y = point.Y;
            return rect;
        }


        internal static bool CloseEnough(this Rect a, Rect b, int delta = 10)
        {
            return (Math.Abs(a.X - b.X) <= delta &&
                Math.Abs(a.Y - b.Y) <= delta &&
                Math.Abs(a.Width - b.Width) <= delta * 2 &&
                Math.Abs(a.Height - b.Height) <= delta * 2);
        }

    }
}
