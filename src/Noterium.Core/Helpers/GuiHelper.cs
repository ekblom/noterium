using System;
using System.Drawing;

namespace Noterium.Core.Helpers
{
    public static class GuiHelper
    {
        public static double GetDistanceBetweenPoints(Point p, Point q)
        {
            double a = p.X - q.X;
            double b = p.Y - q.Y;
            var distance = Math.Sqrt(a * a + b * b);
            return distance;
        }
    }
}