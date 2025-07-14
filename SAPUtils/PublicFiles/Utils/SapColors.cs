using System.Drawing;

namespace SAPUtils.Forms
{
    public static class SapColors {
        public static Color DisabledCellGray { get; } = Color.FromArgb(0xE7, 0xE7, 0xE7);

        public static int ColorToInt(Color color) => RgbToInt(color.R, color.G, color.B);

        public static int RgbToInt(byte red, byte green, byte blue) => red | (green << 8) | (blue << 16);

        public static Color SapTransparent => Color.FromArgb(255, 192, 220, 192);
    }
}