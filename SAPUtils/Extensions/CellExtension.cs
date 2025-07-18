using SAPbouiCOM;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for handling and manipulating cells in an SAP B1 matrix.
    /// </summary>
    /// <remarks>
    /// This static class offers functionality to retrieve values from various SAP B1 matrix cell controls
    /// by identifying the specific type of control within the cell.
    /// </remarks>
    /// <seealso cref="SAPbouiCOM.Matrix"/>
    /// <seealso cref="SAPbouiCOM.Cell"/>
    public static class CellExtension {
        /// <summary>
        /// Retrieves the value contained in a specified cell of an SAP B1 matrix, identifying the type of control within the cell to return the appropriate value.
        /// </summary>
        /// <param name="cell">
        /// The cell object from which the value is retrieved. It is assumed to be a part of an SAP B1 matrix.
        /// </param>
        /// <returns>
        /// The value contained in the cell, cast to the appropriate type based on the control within the cell.
        /// Returns an empty string <c>""</c> if the cell is null or the control type is unsupported.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Cell"/>
        /// <seealso cref="SAPbouiCOM.EditText"/>
        /// <seealso cref="SAPbouiCOM.ComboBox"/>
        /// <seealso cref="SAPbouiCOM.CheckBox"/>
        /// <seealso cref="SAPbouiCOM.StaticText"/>
        /// <seealso cref="SAPbouiCOM.Button"/>
        /// <seealso cref="SAPbouiCOM.PictureBox"/>
        /// <seealso cref="SAPbouiCOM.LinkedButton"/>
        public static object GetValue(this Cell cell) {
            if (cell == null)
                return "";
            object control = cell.Specific;

            switch (control) {
                case EditText editText:
                    return editText.Value;
                case ComboBox comboBox:
                    return comboBox.Selected?.Value ?? "";
                case CheckBox checkBox:
                    return checkBox.Checked;
                case StaticText staticText:
                    return staticText.Caption;
                case Button button:
                    return button.Caption;
                case PictureBox pictureBox:
                    return pictureBox.Picture;
                case LinkedButton _:
                    try {
                        EditText fallback = (EditText)cell.Specific;
                        return fallback.Value;
                    }
                    catch {
                        return "";
                    }
                default:
                    return "";
            }
        }
    }
}