using System.Drawing;

// ReSharper disable MemberCanBePrivate.Global

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides utility methods and predefined color values for use within the SAPUtils context.
    /// This class includes methods for color conversions and preconfigured color definitions
    /// to standardize visual consistency across the application.
    /// </summary>
    public static class SapColors {
        /// Represents a predefined color used to indicate disabled cells in SAP forms.
        /// The `DisabledCellGray` color is primarily utilized to set the background
        /// color of cells that are unavailable for editing or interaction. It is defined
        /// as a light gray color with an RGB value of (231, 231, 231).
        /// This property is read-only and designed for consistent UI rendering where such
        /// disabled states need a visual distinction.
        public static Color DisabledCellGray { get; } = Color.FromArgb(0xE7, 0xE7, 0xE7);

        /// Represents a predefined color, converted to an integer, used to indicate disabled cells in SAP forms.
        /// The `DisabledCellSapGray` property is specifically utilized to standardize the background color
        /// of disabled cells within SAP environments, ensuring consistent visual representation. Internally,
        /// it relies on the color value defined by `DisabledCellGray`.
        /// This property is read-only and provides the integer equivalent of the light gray shade. The color
        /// is defined with an RGB value of (231, 231, 231).
        /// See Also: <see cref="SAPUtils.Utils.SapColors.DisabledCellGray"/>
        public static int DisabledCellSapGray { get; } = ColorToInt(DisabledCellGray);

        /// Represents a transparent color specific to the SAP environment.
        /// This property returns a `Color` object with ARGB values set to (255, 192, 220, 192).
        /// Typically used for rendering transparent elements in compliance with SAP-specific design styles or requirements.
        public static Color SapTransparent => Color.FromArgb(255, 192, 220, 192);

        /// <summary>
        /// Converts a <see cref="Color"/> object to its integer representation.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>An integer value representing the input color in the format 0x00BBGGRR, where BB, GG, and RR are the blue, green, and red components respectively.</returns>
        public static int ColorToInt(Color color) => RgbToInt(color.R, color.G, color.B);

        /// <summary>
        /// Converts red, green, and blue byte components into a single integer representation of a color.
        /// </summary>
        /// <param name="red">The red component of the color, ranging from 0 to 255.</param>
        /// <param name="green">The green component of the color, ranging from 0 to 255.</param>
        /// <param name="blue">The blue component of the color, ranging from 0 to 255.</param>
        /// <returns>Returns an integer representation of the RGB color.</returns>
        public static int RgbToInt(byte red, byte green, byte blue) => red | green << 8 | blue << 16;

        /// <summary>
        /// Darkens a specified <see cref="Color"/> by applying a reduction factor to its RGB components.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to darken.</param>
        /// <param name="factor">
        /// A double value between 0 and 1 that determines the amount to darken the color.
        /// A value of 0 results in black, and 1 leaves the color unchanged.
        /// </param>
        /// <returns>
        /// A new <see cref="Color"/> object with its RGB components darkened by the specified factor.
        /// The alpha component remains unchanged.
        /// </returns>
        /// <seealso cref="Color"/>
        /// <seealso cref="SapColors"/>
        public static Color DarkenColor(Color color, double factor = 0.7) {
            factor = factor < 0
                ? 0
                : factor > 1
                    ? 1
                    : factor;
            return Color.FromArgb(
                color.A,
                (int)(color.R * factor),
                (int)(color.G * factor),
                (int)(color.B * factor)
            );
        }
    }
}