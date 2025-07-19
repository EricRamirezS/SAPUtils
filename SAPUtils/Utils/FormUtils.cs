using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.Extensions;
using Application = SAPbouiCOM.Application;

// ReSharper disable MemberCanBePrivate.Global

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides utility methods for working with forms in the SAP Business One SDK.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class FormUtils {
        /// <summary>
        /// Checks whether a form with the specified unique identifier (UID) exists in the SAP B1 application.
        /// </summary>
        /// <param name="formId">
        /// The unique identifier (UID) of the form to check existence for.
        /// </param>
        /// <param name="app">
        /// An instance of the SAP B1 application where the form is to be checked.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the form with the specified UID exists in the application.
        /// Returns <c>true</c> if the form exists; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// Catches any exceptions thrown during form enumeration in case of unexpected issues with the SAP B1 application.
        /// </exception>
        /// <seealso cref="SAPbouiCOM.Application"/>
        /// <seealso cref="SAPbouiCOM.Forms"/>
        public static bool ExistForm(string formId, Application app) {
            try {
                return app.Forms.Cast<IForm>().FirstOrDefault(x => x.UniqueID == formId) != null;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Determines the corresponding <see cref="BoFieldsType"/> based on the given SAP field type and subtype.
        /// </summary>
        /// <param name="fieldType">
        /// The SAP Business One field type, represented by <see cref="BoFieldTypes"/>.
        /// </param>
        /// <param name="subType">
        /// The SAP Business One field subtype, represented by <see cref="BoFldSubTypes"/>. This helps refine the field type for specific cases such as dates, floats, or quantities.
        /// </param>
        /// <returns>
        /// Returns the appropriate <see cref="BoFieldsType"/> based on the specified field type and subtype.
        /// </returns>
        /// <seealso cref="BoFieldTypes"/>
        /// <seealso cref="BoFldSubTypes"/>
        public static BoFieldsType GetFieldType(BoFieldTypes fieldType, BoFldSubTypes subType) {
            switch (fieldType) {
                case BoFieldTypes.db_Alpha:
                    return BoFieldsType.ft_AlphaNumeric;
                case BoFieldTypes.db_Memo:
                    return BoFieldsType.ft_Text;
                case BoFieldTypes.db_Date:
                    return subType == BoFldSubTypes.st_Time ? BoFieldsType.ft_Text : BoFieldsType.ft_Date;
                case BoFieldTypes.db_Numeric:
                    return BoFieldsType.ft_Integer;
                case BoFieldTypes.db_Float:
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (subType) {
                        case BoFldSubTypes.st_Quantity:
                            return BoFieldsType.ft_Quantity;
                        case BoFldSubTypes.st_Price:
                            return BoFieldsType.ft_Price;
                        case BoFldSubTypes.st_Rate:
                            return BoFieldsType.ft_Rate;
                        case BoFldSubTypes.st_Sum:
                            return BoFieldsType.ft_Sum;
                        case BoFldSubTypes.st_Percentage:
                            return BoFieldsType.ft_Percent;
                        case BoFldSubTypes.st_Measurement:
                            return BoFieldsType.ft_Measure;
                        default:
                            return BoFieldsType.ft_Float;
                    }
                default:
                    return BoFieldsType.ft_AlphaNumeric;
            }
        }

        /// <summary>
        /// Maps a specified SAP Business One field type and subtype to its corresponding data type representation.
        /// </summary>
        /// <param name="fieldType">
        /// The primary field type as defined in the SAP Business One SDK, represented by <see cref="SAPbobsCOM.BoFieldTypes"/>.
        /// </param>
        /// <param name="subType">
        /// The specific field subtype, represented by <see cref="SAPbobsCOM.BoFldSubTypes"/>, providing additional context for the field type.
        /// </param>
        /// <returns>
        /// The corresponding <see cref="SAPbouiCOM.BoDataType"/> that represents the appropriate data type
        /// based on the provided field type and subtype.
        /// </returns>
        /// <seealso cref="SAPbobsCOM.BoFieldTypes"/>
        /// <seealso cref="SAPbobsCOM.BoFldSubTypes"/>
        /// <seealso cref="SAPbouiCOM.BoDataType"/>
        public static BoDataType FieldTypeToDataType(BoFieldTypes fieldType, BoFldSubTypes subType) {
            BoDataType datatype;
            if (fieldType == BoFieldTypes.db_Numeric) {
                datatype = BoDataType.dt_SHORT_NUMBER;
            }
            else
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (subType) {
                    case BoFldSubTypes.st_Measurement:
                        datatype = BoDataType.dt_MEASURE;
                        break;
                    case BoFldSubTypes.st_Percentage:
                        datatype = BoDataType.dt_PERCENT;
                        break;
                    case BoFldSubTypes.st_Price:
                        datatype = BoDataType.dt_PRICE;
                        break;
                    case BoFldSubTypes.st_Quantity:
                        datatype = BoDataType.dt_QUANTITY;
                        break;
                    case BoFldSubTypes.st_Rate:
                        datatype = BoDataType.dt_RATE;
                        break;
                    default:
                    {
                        datatype = fieldType == BoFieldTypes.db_Float ? BoDataType.dt_LONG_NUMBER : BoDataType.dt_SHORT_TEXT;
                        break;
                    }
                }
            return datatype;
        }

        /// <summary>
        /// Validates the input provided in a time cell of a matrix column, ensuring the value is in a valid time format (HH:mm).
        /// Automatically adjusts and formats the input if valid, or displays an error message if invalid.
        /// </summary>
        /// <param name="sboObject">
        /// The SAP Business One object (typically a column) that triggered the event.
        /// </param>
        /// <param name="pVal">
        /// An instance of <see cref="SBOItemEventArg"/> containing information about the triggering item event.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean passed by reference to determine whether the event continues processing.
        /// Set to <c>false</c> if validation fails, halting further event propagation.
        /// </param>
        /// <remarks>
        /// If the provided time value is invalid, an error message is displayed in the SAP Business One status bar.
        /// Validation ensures the format and range of the time are valid within the 24-hour clock system.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.Application"/>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.EditText"/>
        public static void ValidateTimeCell(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent) {
            bubbleEvent = true;
            if (!(sboObject is ColumnClass columnClass)) return;
            if (!(columnClass.Cells.Item(pVal.Row).Specific is EditText editText)) return;

            string value = editText.Value;
            if (string.IsNullOrWhiteSpace(value)) {
                return;
            }
            string cleanValue = value.Replace(":", "", 1);

            bool tryParse = int.TryParse(cleanValue, out int time);
            if (tryParse) {
                int minutes = time % 100;
                if (minutes < 0 || minutes > 59) {
                    bubbleEvent = false;
                }
                int hours = time / 100;
                if (hours < 0 || hours > 23) {
                    bubbleEvent = false;
                }

                if (bubbleEvent) {
                    string visibleValue = $"{hours:D2}:{minutes:D2}";
                    if (visibleValue != value) {
                        editText.Value = visibleValue;
                    }
                }
            }
            else {
                bubbleEvent = false;
            }
            if (bubbleEvent == false) {
                SapAddon.Instance().Application.SetStatusBarMessage("Valor temporal no válido (ODBC-1031) [131-183]", BoMessageTime.bmt_Short);
            }
        }
    }
}