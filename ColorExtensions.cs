using System;
using System.Drawing;

namespace ClassifiedDocumentsComparer
{
    public static class ColorExtensions
    {
        public static bool CompareRGB(this Color color, Color otherColor)
        {
            bool result = true;
            const double eps = 10;

            if (Math.Abs(color.R - otherColor.R) > eps)
                result = false;
            if (Math.Abs(color.G - otherColor.G) > eps)
                result = false;
            if (Math.Abs(color.B - otherColor.B) > eps)
                result = false;

            return result;            
        }
    }
}