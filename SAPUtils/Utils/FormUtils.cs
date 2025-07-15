using System.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;
using ChooseFromList = SAPbouiCOM.ChooseFromList;

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides utility methods for working with forms in the SAP Business One SDK.
    /// </summary>
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
        /// <param name="fieldType">The SAP Business One field type, represented by <see cref="BoFieldTypes"/>.</param>
        /// <param name="subType">The SAP Business One field subtype, represented by <see cref="BoFldSubTypes"/>. This helps refine the field type for some cases like dates and floats.</param>
        /// <returns>
        /// Returns the appropriate <see cref="BoFieldsType"/> for the specified field type and subtype.
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
                    return subType == BoFldSubTypes.st_Time ? BoFieldsType.ft_ShortNumber : BoFieldsType.ft_Date;
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
        /// Adds a new column to the matrix based on the specified user table field.
        /// </summary>
        /// <param name="matrix">The matrix to which the column will be added.</param>
        /// <param name="uid">The unique identifier for the new column.</param>
        /// <param name="field">The user table field that defines the column's properties.</param>
        /// <param name="form">The form associated with the matrix.</param>
        /// <param name="application">The SAP Business One application instance.</param>
        /// <returns>The newly created column configured based on the field definition.</returns>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="IUserTableField"/>
        internal static Column AddColumnFromUserTableField(this Matrix matrix,
            string uid,
            IUserTableField field,
            IForm form,
            Application application) {
            Column column;

            bool isBoolean = field.Type == typeof(bool) ||
                             field.ValidValues?.Count == 2 &&
                             field.ValidValues.Any(v => v.Value == "Y") &&
                             field.ValidValues.Any(v => v.Value == "N");

            bool isCombo = !isBoolean && field.ValidValues != null && field.ValidValues.Count > 0;

            bool isLinked = field.LinkedSystemObject.HasValue ||
                            !string.IsNullOrWhiteSpace(field.LinkedTable) ||
                            !string.IsNullOrWhiteSpace(field.LinkedUdo);

            if (isBoolean) {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_CHECK_BOX);
            }
            else if (isCombo) {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_COMBO_BOX);

                foreach (IUserFieldValidValue validValue in field.ValidValues) {
                    column.ValidValues.Add(validValue.Value, validValue.Description);
                }
            }
            else if (isLinked) {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_EDIT);

                string cflId = $"_CFL{uid}";

                if (form.ChooseFromLists.OfType<ChooseFromList>().All(c => c.UniqueID != cflId)) {
                    ChooseFromListCreationParams cflParams =
                        (ChooseFromListCreationParams)application.CreateObject(BoCreatableObjectType.cot_ChooseFromListCreationParams);
                    cflParams.UniqueID = cflId;
                    cflParams.MultiSelection = false;
                    cflParams.ObjectType = field.LinkedSystemObject?.ToString()
                                           ?? field.LinkedUdo
                                           ?? field.LinkedTable;

                    ChooseFromList cfl = form.ChooseFromLists.Add(cflParams);

                    Conditions cflCond = (Conditions)application.CreateObject(BoCreatableObjectType.cot_Conditions);
                    cfl.SetConditions(cflCond);
                }

                column.ChooseFromListUID = cflId;
                column.ChooseFromListAlias = field.Name;
            }
            else {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_EDIT);
            }

            column.TitleObject.Caption = field.Description;
            column.Visible = true;

            if (field.Size > 0)
                column.Width = field.Size * 5;

            column.Editable = true;

            return column;
        }

        /// <summary>
        /// Adds Date and Time columns to a <see cref="Matrix"/> based on a user table field defined by the
        /// <see cref="SAPUtils.Attributes.UserTables.DateTimeUserTableFieldAttribute"/>.
        /// </summary>
        /// <param name="matrix">The matrix to which the columns will be added.</param>
        /// <param name="index">A reference to the current column index, this will be incremented after adding the columns.</param>
        /// <param name="field">The DateTime user table field attribute that defines the metadata for the columns.</param>
        /// <returns>A tuple containing the Date <see cref="SAPbouiCOM.Column"/> and Time <see cref="SAPbouiCOM.Column"/> objects added to the matrix.</returns>
        /// <remarks>
        /// The method creates two columns in the specified <see cref="Matrix"/>: one for the date and one for the time.
        /// The columns' titles and properties are configured based on the <see cref="DateTimeUserTableFieldAttribute"/>.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPUtils.Attributes.UserTables.DateTimeUserTableFieldAttribute"/>
        internal static (Column Date, Column Time) AddDateTimeColumnsFromUserTableField(this Matrix matrix,
            ref int index,
            DateTimeUserTableFieldAttribute field) {

            Column dateColumn = matrix.Columns.Add($"_C{index++}", BoFormItemTypes.it_EDIT);
            Column timeColumn = matrix.Columns.Add($"_C{index++}", BoFormItemTypes.it_EDIT);

            dateColumn.TitleObject.Caption = field.DateDescription;
            dateColumn.Visible = true;

            timeColumn.TitleObject.Caption = field.TimeDescription;
            timeColumn.Visible = true;

            dateColumn.Width = 50;
            timeColumn.Width = 20;

            dateColumn.Editable = true;
            timeColumn.Editable = true;

            return (dateColumn, timeColumn);
        }
    }
}