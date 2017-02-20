using System.Collections.Generic;
using System.Windows.Media;

namespace Noterium.Core.Constants
{
    public static class DefaultTagColors
    {
        public static List<Color> Colors = new List<Color>
        {
            Convert("#5dccaa"),
            Convert("#58c0ff"),
            Convert("#cd392f"),
            Convert("#cd392f"),
            Convert("#f36e4b"),
            Convert("#330099"),
            Convert("#e7d3e2"),
            Convert("#669999"),
            Convert("#313131"),
            Convert("#6cadff")
        };

        private static Color Convert(string hex)
        {
            return (Color) ColorConverter.ConvertFromString(hex);
        }
    }
}