using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for performing string manipulations.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class StringExtension {
        /// <summary>
        /// Replaces a specified number of occurrences of a substring in the input string with another substring.
        /// </summary>
        /// <param name="input">The original string where replacements will be performed.</param>
        /// <param name="oldValue">The substring to replace.</param>
        /// <param name="newValue">The substring to replace with.</param>
        /// <param name="count">The maximum number of replacements to perform.</param>
        /// <returns>A new string with the specified number of replacements made. If the <paramref name="count"/> is less than or equal to 0, or <paramref name="input"/> or <paramref name="oldValue"/> is null or empty, the original string is returned.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="oldValue"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="oldValue"/> is an empty string.</exception>
        /// <seealso cref="StringBuilder"/>
        public static string Replace(this string input, string oldValue, string newValue, int count) {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue) || count <= 0)
                return input;

            int index = 0, replacements = 0;
            StringBuilder result = new StringBuilder();

            while (replacements < count) {
                int pos = input.IndexOf(oldValue, index, StringComparison.Ordinal);
                if (pos == -1)
                    break;

                result.Append(input, index, pos - index);
                result.Append(newValue);

                index = pos + oldValue.Length;
                replacements++;
            }

            result.Append(input, index, input.Length - index);
            return result.ToString();
        }

        /// <summary>
        /// Replaces up to a specified number of occurrences of a substring within a string with another substring.
        /// </summary>
        /// <param name="input">The original string where replacements will be made.</param>
        /// <param name="oldChar">The char to replace.</param>
        /// <param name="newChar">The char to replace with.</param>
        /// <param name="count">The maximum number of replacements to perform.</param>
        /// <returns>A new string with the specified number of replacements made, or the original string if no replacements were performed.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="oldChar"/> is null or empty.</exception>
        public static string Replace(this string input, char oldChar, char newChar, int count) {
            if (string.IsNullOrEmpty(input) || count <= 0)
                return input;

            int replacements = 0;
            StringBuilder result = new StringBuilder(input.Length);

            foreach (char c in input) {
                if (c == oldChar && replacements < count) {
                    result.Append(newChar);
                    replacements++;
                }
                else {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}