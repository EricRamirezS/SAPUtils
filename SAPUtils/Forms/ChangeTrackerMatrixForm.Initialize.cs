using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Extensions;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Utils;
using ChooseFromList = SAPbouiCOM.ChooseFromList;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private Column _stateColumn;

        /// <summary>
        /// Specifies whether the matrix has already been generated and attached to the form.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, this property returns <c>false</c>. When set to <c>false</c>, the class will
        /// dynamically build and configure the matrix at runtime based on the underlying 
        /// model <typeparamref name="T"/>. This behavior is convenient during development 
        /// and testing, since it allows modifications in the model structure to be automatically 
        /// reflected in the generated matrix.  
        /// </para>
        /// <para>
        /// However, this dynamic generation process is expensive in terms of performance 
        /// because the matrix and its columns must be created, bound, and initialized every time 
        /// the form is opened.  
        /// </para>
        /// <para>
        /// For production environments it is strongly recommended to set this property to <c>true</c> 
        /// and to include a pre-generated matrix definition that already contains its columns. 
        /// Such a definition can be obtained by calling <see cref="UserForm.GetAsXML"/> on the form 
        /// after the matrix has been built once in development.  
        /// </para>
        /// <para>
        /// The key point is that in production the model should be stable and not subject to 
        /// structural changes. Therefore, it is unnecessary and inefficient to regenerate 
        /// the matrix dynamically at runtime. Instead, reusing the pre-generated XML provides 
        /// consistent behavior with significantly reduced startup cost.  
        /// </para>
        /// </remarks>
        /// <seealso cref="SAPUtils.Forms.ChangeTrackerMatrixForm{T}"/>
        protected virtual bool MatrixAlreadyGenerated => false;

        /// <inheritdoc />
        [Localizable(false)]
        public override void OnInitializeComponent() {
            _saveButton = GetSaveButton();
            _matrix = GetMatrix();
            _exportButton = GetExportToExcelButton();
            _importButton = GetImportFromExcelButton();

            try {
                _dataTable = DataSources.DataTables.Item("_DataTable");
            }
            catch {
                _dataTable = DataSources.DataTables.Add("_DataTable");
            }

            try {
                _helper = (EditText)UIAPIRawForm.Items.Item("_helper").Specific;
            }
            catch {
                _helper = (EditText)UIAPIRawForm.Items.Add("_helper", BoFormItemTypes.it_EDIT).Specific;
            }
            _helper.Item.Top = -10000;
            _helper.Item.Left = -10000;
            _helper.Item.Enabled = true;
            _helper.Item.Visible = true;
        }

        [Localizable(false)]
        private void CustomInitializeComponent() {
            DataColumn indexDataColumn = _dataTable.Columns.Add("#", BoFieldsType.ft_Integer);
            DataColumn codeDataColumn = _dataTable.Columns.Add("Code", BoFieldsType.ft_AlphaNumeric, 50);
            DataColumn nameDataColumn = _dataTable.Columns.Add("Name", BoFieldsType.ft_AlphaNumeric, 100);

            Column indexColumn = _matrix.Columns.Add("#", BoFormItemTypes.it_EDIT);
            indexColumn.Editable = false;
            indexColumn.DataBind.Bind(_dataTable.UniqueID, "#");
            indexColumn.Width = 2 * 5;

            Column codeColumn = _matrix.Columns.Add("Code", BoFormItemTypes.it_EDIT);
            codeColumn.Editable = false;
            codeColumn.TitleObject.Caption = Texts.ChangeTrackerMatrixForm_CustomInitializeComponent_Code;
            codeColumn.DataBind.Bind(_dataTable.UniqueID, "Code");
            codeColumn.Width = 50;

            Column nameColumn = _matrix.Columns.Add("Name", BoFormItemTypes.it_EDIT);
            nameColumn.Editable = true;
            nameColumn.TitleObject.Caption = Texts.ChangeTrackerMatrixForm_CustomInitializeComponent_Name;
            nameColumn.DataBind.Bind(_dataTable.UniqueID, "Name");
            nameColumn.Width = 100;

            ColumnInfo.Add("#", (indexDataColumn, indexColumn));
            ColumnInfo.Add("Code", (codeDataColumn, codeColumn));
            ColumnInfo.Add("Name", (nameDataColumn, nameColumn));
            ColumnToProperty.Add("Code", typeof(T).GetProperty("Code", BindingFlags.Public | BindingFlags.Instance));
            ColumnToProperty.Add("Name", typeof(T).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance));

            List<(PropertyInfo Property, IUserTableField Field)> itemInfo = UserTableMetadataCache.GetUserFields(typeof(T));

            int i = 1;
            foreach ((PropertyInfo property, IUserTableField field) in itemInfo) {
                string fieldName = property.Name;
                if (field is DateTimeFieldAttribute dt) {
                    string dateColumnUid = $"_C{i}D";
                    string timeColumnUid = $"_C{i}T";

                    DataColumn dateColumn =
                        _dataTable.Columns.Add(dateColumnUid, FormUtils.GetFieldType(BoFieldTypes.db_Date, BoFldSubTypes.st_None), 10);
                    DataColumn timeColumn =
                        _dataTable.Columns.Add(timeColumnUid, FormUtils.GetFieldType(BoFieldTypes.db_Date, BoFldSubTypes.st_Time), 5);

                    (Column date, Column time) = _matrix.AddDateTimeColumnsFromUserTableField(ref i, dt, property);
                    date.DataBind.Bind(_dataTable.UniqueID, dateColumnUid);
                    time.DataBind.Bind(_dataTable.UniqueID, timeColumnUid);

                    ColumnInfo.Add(fieldName + "Date", (dateColumn, date));
                    ColumnInfo.Add(fieldName + "Time", (timeColumn, time));
                    ColumnToProperty.Add(time.UniqueID, property);
                    ColumnToProperty.Add(date.UniqueID, property);

                }
                else {
                    string columnId = $"_C{i}";
                    int size = field.SubType == BoFldSubTypes.st_Time ? 5 : field.Size;
                    DataColumn dataColumn = _dataTable.Columns.Add(columnId, FormUtils.GetFieldType(field.FieldType, field.SubType), size);

                    Column column = _matrix.AddColumnFromUserTableField(columnId, field, property, this, Application);
                    column.DataBind.Bind(_dataTable.UniqueID, columnId);

                    ColumnInfo.Add(fieldName, (dataColumn, column));
                    ColumnToProperty.Add(column.UniqueID, property);

                    string cflId = $"_CFL{columnId}";
                    ChooseFromList cfl = null;
                    try {
                        cfl = UIAPIRawForm.ChooseFromLists.Item(cflId);
                    }
                    catch {
                        // ignored
                    }
                    finally {
                        ChooseFromListInfo.Add(fieldName, cfl);
                    }
                }
                i++;
            }

            _dataTable.Columns.Add("_S_T_A_T_E", BoFieldsType.ft_AlphaNumeric);
            _stateColumn = _matrix.Columns.Add("_S_T_A_T_E", BoFormItemTypes.it_EDIT);
            _stateColumn.TitleObject.Caption = Texts.ChangeTrackerMatrixForm_CustomInitializeComponent__State_;
            _stateColumn.Editable = false;
            _stateColumn.Width = 32;
            _stateColumn.DataBind.Bind(_dataTable.UniqueID, "_S_T_A_T_E");

            _matrix.SelectionMode = BoMatrixSelect.ms_Single;

        }
    }
}