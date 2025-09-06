using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SAPbouiCOM;
using static SAPUtils.Utils.Parsers;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for the <see cref="SAPbouiCOM.EditText"/> class to simplify parsing and setting values.
    /// </summary>
    [Localizable(false)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class EditTextExtensions {

        /// <summary>
        /// Converts the value of the current <see cref="SAPbouiCOM.EditText"/> to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="self">The instance of <see cref="SAPbouiCOM.EditText"/> containing the value to convert.</param>
        /// <returns>A <see cref="DateTime"/> that represents the value of the <see cref="SAPbouiCOM.EditText"/>.
        /// Returns <see cref="DateTime.MinValue"/> if the conversion fails.</returns>
        /// <seealso cref="SAPUtils.Utils.Parsers.ParseDate"/>
        public static DateTime ValueAsDate(this EditText self) {
            return ParseDate(self.Value);
        }

        /// <summary>
        /// Converts the value of the specified <see cref="EditText"/> to a time of type <see cref="DateTime"/>.
        /// </summary>
        /// <param name="self">The <see cref="EditText"/> object containing the value to be converted.</param>
        /// <returns>A <see cref="DateTime"/> representing the time value of the <see cref="EditText"/>. If the value cannot be converted, it will return <see cref="DateTime.MinValue"/>.</returns>
        /// <seealso cref="SAPUtils.Utils.Parsers.ParseTime(object)"/>
        public static DateTime ValueAsTime(this EditText self) {
            return ParseTime(self.Value);
        }

        /// <summary>
        /// Converts the value of the current <see cref="EditText"/> instance to a double value.
        /// </summary>
        /// <param name="self">The <see cref="EditText"/> instance whose value will be converted.</param>
        /// <returns>The numerical value represented by the <see cref="EditText"/> instance as a double.</returns>
        /// <remarks>If the value cannot be converted, this method may throw an appropriate exception or return 0, depending on the implementation.</remarks>
        /// <seealso cref="ParseDouble"/>
        public static double ValueAsDouble(this EditText self) {
            return ParseDouble(self.Value);
        }

        /// <summary>
        /// Retrieves the value of the <see cref="EditText"/> control as an integer.
        /// </summary>
        /// <param name="self">The <see cref="EditText"/> instance from which the value will be retrieved.</param>
        /// <returns>An <see cref="int"/> representation of the value of the <see cref="EditText"/> control.</returns>
        /// <exception cref="FormatException">Thrown if the value of the <see cref="EditText"/> cannot be parsed as an integer.</exception>
        /// <seealso cref="SAPUtils.Utils.Parsers.ParseInteger(object)"/>
        public static int ValueAsInteger(this EditText self) {
            return ParseInteger(self.Value);
        }

        /// <summary>
        /// Sets the value of the <see cref="EditText"/> to a date formatted in "yyyyMMdd".
        /// </summary>
        /// <param name="self">The <see cref="EditText"/> control to set the value for.</param>
        /// <param name="value">The <see cref="DateTime"/> value to set.</param>
        public static void SetValue(this EditText self, DateTime value) {
            self.Value = value.ToString("yyyyMMdd");
        }

        /// <summary>
        /// Sets the value of the specified <see cref="SAPbouiCOM.EditText"/> to the given string value.
        /// </summary>
        /// <param name="self">The instance of <see cref="SAPbouiCOM.EditText"/> to be updated.</param>
        /// <param name="value">The string value to set in the <see cref="SAPbouiCOM.EditText"/>.</param>
        public static void SetValue(this EditText self, string value) {
            self.Value = value;
        }

        /// <summary>
        /// Sets the value of the current <see cref="SAPbouiCOM.EditText"/> based on the specified <see cref="bool"/> value.
        /// </summary>
        /// <param name="self">The instance of <see cref="SAPbouiCOM.EditText"/> to set the value for.</param>
        /// <param name="value">A <see cref="bool"/> value to assign to the <see cref="SAPbouiCOM.EditText"/>.
        /// If true, the value is set to "Y"; if false, the value is set to "N".</param>
        public static void SetValue(this EditText self, bool value) {
            self.Value = value ? "Y" : "N";
        }

        /// <summary>
        /// Sets the value of the current <see cref="SAPbouiCOM.EditText"/> to the specified object.
        /// Supports various types including integers, doubles, floats, strings, dates, and more.
        /// </summary>
        /// <param name="self">The instance of <see cref="SAPbouiCOM.EditText"/> where the value will be set.</param>
        /// <param name="value">The object to set as the value of the <see cref="SAPbouiCOM.EditText"/>.
        /// If the object is null, an empty string will be set.</param>
        /// <seealso cref="ValueAsDouble"/>
        /// <seealso cref="ValueAsDate"/>
        /// <seealso cref="ValueAsInteger"/>
        /// <seealso cref="ValueAsTime"/>
        public static void SetValue(this EditText self, object value) {
            switch (value) {
                case long l:
                    self.SetValue(l);
                    break;
                case int i:
                    self.SetValue(i);
                    break;
                case short s:
                    self.SetValue(s);
                    break;
                case float f:
                    self.SetValue(f);
                    break;
                case double d:
                    self.SetValue(d);
                    break;
                case DateTime dt:
                    self.SetValue(dt);
                    break;
                case string str:
                    self.SetValue(str);
                    break;
                case bool b:
                    self.SetValue(b);
                    break;
                case null:
                    self.SetValue("");
                    break;
                default:
                    self.SetValue(value.ToString());
                    break;
            }
        }

        /// <summary>
        /// Sets the value of the <see cref="SAPbouiCOM.EditText"/> to the specified <see cref="double"/> value.
        /// </summary>
        /// <param name="self">The instance of <see cref="SAPbouiCOM.EditText"/> where the value will be set.</param>
        /// <param name="value">The <see cref="double"/> value to assign to the <see cref="SAPbouiCOM.EditText"/>.</param>
        /// <seealso cref="ValueAsDouble"/>
        public static void SetValue(this EditText self, double value) {
            self.Value = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the value of the current <see cref="SAPbouiCOM.EditText"/> to the specified integer.
        /// </summary>
        /// <param name="self">The instance of <see cref="SAPbouiCOM.EditText"/> where the value will be set.</param>
        /// <param name="value">The integer value to set in the <see cref="SAPbouiCOM.EditText"/>.</param>
        public static void SetValue(this EditText self, int value) {
            self.Value = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}