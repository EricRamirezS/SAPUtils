using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.Repository;
using SAPUtils.__Internal.Utils;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Extensions;
using SAPUtils.Forms;
using SAPUtils.Models.UserTables;
using Application = SAPbouiCOM.Application;
using ChooseFromList = SAPbouiCOM.ChooseFromList;
using IChooseFromList = SAPbouiCOM.IChooseFromList;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;

// ReSharper disable MemberCanBePrivate.Global

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
        /// Adds a new column to the specified matrix based on the given user table field and its associated properties.
        /// </summary>
        /// <param name="matrix">
        /// The matrix to which the new column will be added.
        /// </param>
        /// <param name="uid">
        /// The unique identifier for the column to be added.
        /// </param>
        /// <param name="field">
        /// An instance of the user table field defining the properties of the column (e.g., type, size, and description).
        /// </param>
        /// <param name="property">
        /// The property information associated with the user table field.
        /// </param>
        /// <param name="form">
        /// The user form associated with the matrix where the column is being added.
        /// </param>
        /// <param name="application">
        /// The SAP Business One application instance managing the matrix.
        /// </param>
        /// <returns>
        /// A new instance of a <see cref="SAPbouiCOM.Column"/> added to the matrix and configured based on the user table field.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPbouiCOM.Application"/>
        /// <seealso cref="IUserTableField"/>
        internal static Column AddColumnFromUserTableField(this Matrix matrix,
            string uid,
            IUserTableField field,
            PropertyInfo property,
            UserForm form,
            Application application) {
            Column column;

            bool isBoolean = IsBooleanField(field);

            bool isCombo = IsComboField(field);

            bool isLinked = IsLinkedField(field);

            if (isBoolean) {
                column = AddCheckBoxColumn(matrix, uid);
            }
            else if (isCombo) {
                column = AddComboBoxColumn(matrix, uid, field);
            }
            else if (isLinked) {
                column = string.IsNullOrEmpty(field.LinkedSystemObject?.ToString() ?? field.LinkedUdo)
                    ? AddComboBoxFromUdt(matrix, uid, field, form)
                    : AddLinkedButtonColumn(matrix, uid, field, form, application);
            }
            else if (field.FieldType == BoFieldTypes.db_Date && field.SubType == BoFldSubTypes.st_Time) {
                column = AddTimeColumn(matrix, uid);
            }
            else {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_EDIT);
            }

            column.TitleObject.Caption = field.Description ?? field.Name ?? property.Name;
            column.Visible = true;

            if (field.Size == int.MaxValue) {
                column.Width = 400;
            }
            else if (field.Size > 0)
                column.Width = Math.Max(Math.Min(300, field.Size * 4), 16);

            column.Editable = true;

            return column;
        }
        /// <summary>
        /// Determines whether a user table field is linked by checking if it has a linked system object,
        /// linked table, or linked UDO (User-Defined Object).
        /// </summary>
        /// <param name="field">
        /// The user table field to evaluate for link associations.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the specified field is linked.
        /// Returns <c>true</c> if the field is linked (via a linked system object, table, or UDO); otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField"/>
        /// <seealso cref="UDFLinkedSystemObjectTypesEnum"/>
        private static bool IsLinkedField(IUserTableField field) {
            return field.LinkedSystemObject.HasValue ||
                   !string.IsNullOrWhiteSpace(field.LinkedTable) ||
                   !string.IsNullOrWhiteSpace(field.LinkedUdo);
        }
        /// <summary>
        /// Determines whether the specified user table field is a combo field in the SAP Business One application.
        /// </summary>
        /// <param name="field">
        /// An instance of <see cref="IUserTableField"/> representing the user table field to be checked.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the specified field is a combo field.
        /// Returns <c>true</c> if the field is a combo field; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField"/>
        private static bool IsComboField(IUserTableField field) {
            return !IsBooleanField(field) && field.ValidValues != null && field.ValidValues.Count > 0;
        }
        /// <summary>
        /// Determines whether a user table field is of type boolean based on its properties and valid values.
        /// </summary>
        /// <param name="field">
        /// The user table field to evaluate. This is an instance of <c>IUserTableField</c> containing field type and valid values.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the specified field is boolean.
        /// Returns <c>true</c> if the field is boolean; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField"/>
        /// <seealso cref="SAPUtils.Models.UserTables.IUserFieldValidValue"/>
        private static bool IsBooleanField(IUserTableField field) {
            return field.Type == typeof(bool) ||
                   field.Type == typeof(bool?) ||
                   field.ValidValues?.Count == 2 &&
                   field.ValidValues.Any(v => v.Value == "Y") &&
                   field.ValidValues.Any(v => v.Value == "N");
        }
        /// <summary>
        /// Adds a new linked button column to the specified matrix with functionality tied to
        /// either a linked system object or a user-defined object (UDO), as defined in the provided user table field.
        /// </summary>
        /// <param name="matrix">
        /// The matrix control to which the linked button column will be added.
        /// </param>
        /// <param name="uid">
        /// The unique identifier (UID) for the new column.
        /// </param>
        /// <param name="field">
        /// An instance of <see cref="IUserTableField"/> describing the linked data source, including
        /// any system objects or user-defined objects associated with the column.
        /// </param>
        /// <param name="form">
        /// The user form in which the matrix resides, used to manage the column’s data sources and events.
        /// </param>
        /// <param name="application">
        /// The SAP B1 application instance used to interact with the SAP application environment.
        /// </param>
        /// <returns>
        /// A <see cref="SAPbouiCOM.Column"/> object representing the newly added linked button column.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField"/>
        /// <seealso cref="SAPbouiCOM.LinkedButton"/>
        private static Column AddLinkedButtonColumn(Matrix matrix, string uid, IUserTableField field, UserForm form, Application application) {
            Column column = matrix.Columns.Add(uid, BoFormItemTypes.it_LINKED_BUTTON);
            string cflId = $"_CFL{uid}";

            LinkedButton lb = column.ExtendedObject as LinkedButton;
            if (lb != null && field.LinkedSystemObject.HasValue) {
                lb.LinkedObject = (BoLinkedObject)(int)field.LinkedSystemObject.Value;
            }
            else if (lb != null && !string.IsNullOrEmpty(field.LinkedUdo)) {
                lb.LinkedObjectType = field.LinkedUdo;
            }

            string dbTableNane = field.LinkedSystemObject.GetTableName() ?? field.LinkedUdo;
            form.DataSources.DBDataSources.Add(dbTableNane);
            UserDataSource userDataSource = form.DataSources.UserDataSources.Add($"UDS{uid}", BoDataType.dt_SHORT_TEXT);

            if (form.ChooseFromLists.OfType<ChooseFromList>().All(c => c.UniqueID != cflId)) {
                CreateChooseFromList(field, form, application, cflId, column, userDataSource);
            }

            column.ChooseFromListUID = cflId;
            column.ChooseFromListAlias = SapUtils.GetPrimaryKey(dbTableNane);
            return column;
        }
        private static void CreateChooseFromList(IUserTableField field, UserForm form, Application application, string cflId, Column column,
            UserDataSource userDataSource) {
            ChooseFromListCreationParams cflParams =
                (ChooseFromListCreationParams)application.CreateObject(BoCreatableObjectType.cot_ChooseFromListCreationParams);
            cflParams.UniqueID = cflId;
            cflParams.MultiSelection = false;
            cflParams.ObjectType = field.LinkedSystemObject.HasValue
                ? ((int)field.LinkedSystemObject.Value).ToString()
                : field.LinkedUdo;


            IChooseFromList cfl = form.ChooseFromLists.Add(cflParams);

            column.DataBind.SetBound(true, "", userDataSource.UID);

            Conditions cflCond = (Conditions)application.CreateObject(BoCreatableObjectType.cot_Conditions);
            cfl.SetConditions(cflCond);

            CflSubscriber(form, cfl, column);
        }
        /// <summary>
        /// Adds a combo box to a matrix column, populated with valid values from a user-defined table (UDT).
        /// </summary>
        /// <param name="matrix">
        /// The matrix to which the combo box column will be added.
        /// </param>
        /// <param name="uid">
        /// The unique identifier (UID) for the combo box column.
        /// </param>
        /// <param name="field">
        /// The user table field containing metadata about the linked table and valid values source.
        /// </param>
        /// <param name="form">
        /// The user form instance to which the matrix and data sources belong.
        /// </param>
        /// <returns>
        /// The newly created column containing the combo box with valid values.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField"/>
        /// <seealso cref="SAPUtils.Forms.UserForm"/>
        private static Column AddComboBoxFromUdt(Matrix matrix, string uid, IUserTableField field, UserForm form) {
            try {
                form.DataSources.UserDataSources.Item($"_DS{uid}");
            }
            catch {
                form.DataSources.UserDataSources.Add($"_DS{uid}", BoDataType.dt_SHORT_TEXT, 50);
            }
            Column column = matrix.Columns.Add(uid, BoFormItemTypes.it_COMBO_BOX);
            column.DataBind.SetBound(true, "", $"_DS{uid}");

            IList<IUserFieldValidValue> vv = Repository.Get().GetValidValues(field.LinkedTable);

            foreach (IUserFieldValidValue userFieldValidValue in vv) {
                bool add = true;
                for (int i = 0; i < column.ValidValues.Count; i++)
                    if (column.ValidValues.Item(i).Value == userFieldValidValue.Value) {
                        add = false;
                        break;
                    }
                if (add)
                    column.ValidValues.Add(userFieldValidValue.Value, userFieldValidValue.Description);
            }
            column.DisplayDesc = true;
            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(field.LinkedTable);
            if (userTable != null) {
                column.ExpandType = userTable.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual
                    ? BoExpandType.et_ValueDescription
                    : BoExpandType.et_DescriptionOnly;
            }
            return column;
        }
        /// <summary>
        /// Adds a time column to the specified <see cref="SAPbouiCOM.Matrix"/> with the given unique identifier (UID).
        /// </summary>
        /// <param name="matrix">
        /// The SAP B1 <see cref="SAPbouiCOM.Matrix"/> instance to which the time column will be added.
        /// </param>
        /// <param name="uid">
        /// The unique identifier (UID) for the new column.
        /// </param>
        /// <returns>
        /// The newly added <see cref="SAPbouiCOM.Column"/> configured as a time field.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        private static Column AddTimeColumn(Matrix matrix, string uid) {
            Column column = matrix.Columns.Add(uid, BoFormItemTypes.it_EDIT);
            column.ValidateBefore += ValidateTimeCell;
            return column;
        }
        /// <summary>
        /// Adds a ComboBox column to the specified matrix based on the provided user table field.
        /// </summary>
        /// <param name="matrix">
        /// The SAP Business One Matrix control to which the ComboBox column will be added.
        /// </param>
        /// <param name="uid">
        /// The unique identifier for the column to be added.
        /// </param>
        /// <param name="field">
        /// The user table field providing metadata, such as valid values and their descriptions,
        /// for the ComboBox column.
        /// </param>
        /// <returns>
        /// The newly created ComboBox column added to the matrix.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField"/>
        /// <seealso cref="SAPUtils.Models.UserTables.IUserFieldValidValue"/>
        private static Column AddComboBoxColumn(Matrix matrix, string uid, IUserTableField field) {
            Column column = matrix.Columns.Add(uid, BoFormItemTypes.it_COMBO_BOX);

            foreach (IUserFieldValidValue validValue in field.ValidValues) {
                column.ValidValues.Add(validValue.Value, validValue.Description);
            }
            return column;
        }
        /// <summary>
        /// Adds a checkbox column to the specified SAP B1 Matrix with the given unique identifier (UID).
        /// </summary>
        /// <param name="matrix">
        /// The matrix object to which the checkbox column will be added.
        /// </param>
        /// <param name="uid">
        /// The unique identifier (UID) for the new checkbox column.
        /// </param>
        /// <returns>
        /// The newly created checkbox column.
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        private static Column AddCheckBoxColumn(Matrix matrix, string uid) {
            Column column = matrix.Columns.Add(uid, BoFormItemTypes.it_CHECK_BOX);
            column.ValOn = "Y";
            column.ValOff = "N";
            return column;
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

        /// <summary>
        /// Adds Date and Time columns to a <see cref="Matrix"/> based on a user table field defined by the
        /// <see cref="DateTimeFieldAttribute"/>.
        /// </summary>
        /// <param name="matrix">
        /// The matrix to which the columns will be added.
        /// </param>
        /// <param name="index">
        /// A reference to the current column index. This value will be incremented after adding the columns.
        /// </param>
        /// <param name="field">
        /// The DateTime user table field attribute that defines the metadata for the columns.
        /// </param>
        /// <param name="property">
        /// The property representing the user table field for the DateTime columns.
        /// </param>
        /// <returns>
        /// A tuple containing the Date <see cref="SAPbouiCOM.Column"/> and Time <see cref="SAPbouiCOM.Column"/> objects added to the matrix.
        /// </returns>
        /// <remarks>
        /// The method creates two columns in the specified <see cref="Matrix"/>: one for the date and one for the time.
        /// The columns' titles and properties are configured based on the <see cref="DateTimeFieldAttribute"/>.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="DateTimeFieldAttribute"/>
        internal static (Column Date, Column Time) AddDateTimeColumnsFromUserTableField(this Matrix matrix,
            ref int index,
            DateTimeFieldAttribute field,
            PropertyInfo property) {

            Column dateColumn = matrix.Columns.Add($"_C{index}D", BoFormItemTypes.it_EDIT);
            Column timeColumn = matrix.Columns.Add($"_C{index}T", BoFormItemTypes.it_EDIT);

            dateColumn.TitleObject.Caption = field.DateDescription == "Date"
                ? property.Name + "Date"
                : field.DateDescription;
            dateColumn.Visible = true;

            timeColumn.TitleObject.Caption = field.TimeDescription == "Time"
                ? property.Name + "Time"
                : field.TimeDescription;
            timeColumn.Visible = true;

            dateColumn.Width = 75;
            timeColumn.Width = 50;

            dateColumn.Editable = true;
            timeColumn.Editable = true;

            timeColumn.ValidateBefore += ValidateTimeCell;

            return (dateColumn, timeColumn);
        }

        /// <summary>
        /// Subscribes to Choose-From-List (CFL) events for a specified form, CFL, and column in the SAP B1 application.
        /// This sets up event handling to manage data selection from the CFL and populate the appropriate cell in the specified column.
        /// </summary>
        /// <param name="form">
        /// The form where the CFL and event handling are to be applied.
        /// </param>
        /// <param name="oChooseFromList">
        /// The instance of the Choose-From-List (CFL) used for data selection.
        /// </param>
        /// <param name="column">
        /// The column in the form where the selected data will be populated.
        /// </param>
        /// <exception cref="System.Exception">
        /// Handles potential exceptions during the event handling process and logs errors using the application logger.
        /// </exception>
        /// <seealso cref="SAPbouiCOM.IChooseFromList"/>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPUtils.Forms.UserForm"/>
        internal static void CflSubscriber(
            UserForm form,
            IChooseFromList oChooseFromList,
            Column column) {
            form.CloseAfter += FormOnCloseAfter;

            SapAddon.Instance().Application.ItemEvent += ApplicationOnItemEvent;

            return;

            void ApplicationOnItemEvent(string formUid, ref ItemEvent pVal, out bool bubbleEvent) {
                bubbleEvent = true;
                if (formUid != form.UIAPIRawForm.UniqueID) return;
                if (pVal.EventType != BoEventTypes.et_CHOOSE_FROM_LIST) return;

                // ReSharper disable once SuspiciousTypeConversion.Global
                IChooseFromListEvent oCflEvent = pVal as IChooseFromListEvent;

                if (oCflEvent != null && oCflEvent.ChooseFromListUID != oChooseFromList.UniqueID) return;

                if (oCflEvent != null && oCflEvent.BeforeAction) return;

                if (oCflEvent == null) return;

                DataTable cflDataTable = oCflEvent.SelectedObjects;

                string val = null;

                try {
                    val = Convert.ToString(cflDataTable.GetValue(0, 0));
                }
                catch (Exception ex) {
                    Logger.Instance.Error(ex);
                }
                if (pVal.ItemUID != "matrix") return;
                {
                    try {
                        ((EditText)column.Cells.Item(pVal.Row).Specific).Value = val;
                    }
                    catch (Exception ex) {
                        Logger.Instance.Error($"Error al aplicar CFL en matrix: {ex}");
                    }
                }
            }

            void FormOnCloseAfter(SBOItemEventArg pVal) {
                SapAddon.Instance().Application.ItemEvent -= ApplicationOnItemEvent;
            }
        }
    }
}