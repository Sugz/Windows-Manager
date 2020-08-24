﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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

        internal static Rect ToRect(this NativeMethods.RECT rect)
        {
            return new Rect(
                rect.Left,
                rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top);
        }
    }
}
