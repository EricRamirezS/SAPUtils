using System.Collections.Generic;
using System.Reflection;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Internal.Attributes.UserTables;
using SAPUtils.Internal.Models;
using SAPUtils.Utils;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        /// <inheritdoc />
        public override void OnInitializeComponent() {
            _saveButton = GetSaveButton();
            _matrix = GetMatrix();

            _dataTable = DataSources.DataTables.Add("_DataTable");
        }
        private void CustomInitializeComponent() {
            _dataTable.Columns.Add("#", BoFieldsType.ft_Integer);
            _dataTable.Columns.Add("Code", BoFieldsType.ft_AlphaNumeric);
            _dataTable.Columns.Add("Name", BoFieldsType.ft_AlphaNumeric);

            Column col = _matrix.Columns.Add("#", BoFormItemTypes.it_EDIT);
            col.Editable = false;
            col.DataBind.Bind(_dataTable.UniqueID, "#");
            col = _matrix.Columns.Add("Code", BoFormItemTypes.it_EDIT);
            col.Editable = false;
            col.TitleObject.Caption = "Código";
            col.DataBind.Bind(_dataTable.UniqueID, "Code");

            col = _matrix.Columns.Add("Name", BoFormItemTypes.it_EDIT);
            col.Editable = true;
            col.TitleObject.Caption = "Nombre";
            col.DataBind.Bind(_dataTable.UniqueID, "Name");


            List<(PropertyInfo Property, IUserTableField Field)> itemInfo = UserTableMetadataCache.GetUserFields(typeof(T));

            int i = 1;
            foreach ((PropertyInfo property, IUserTableField field) in itemInfo) {
                string fieldName = property.Name;
                if (field is DateTimeUserTableFieldAttribute dt) {
                    string dateColumnUid = $"_C{i}";
                    string timeColumnUid = $"_C{i + 1}";

                    DataColumn dateColumn =
                        _dataTable.Columns.Add(dateColumnUid, FormUtils.GetFieldType(BoFieldTypes.db_Date, BoFldSubTypes.st_None), 10);
                    DataColumn timeColumn =
                        _dataTable.Columns.Add(timeColumnUid, FormUtils.GetFieldType(BoFieldTypes.db_Date, BoFldSubTypes.st_Time), 4);

                    (Column date, Column time) = _matrix.AddDateTimeColumnsFromUserTableField(ref i, dt);
                    date.DataBind.Bind(_dataTable.UniqueID, dateColumnUid);
                    time.DataBind.Bind(_dataTable.UniqueID, timeColumnUid);

                    ColumnInfo.Add(fieldName + "Date", (dateColumn, date));
                    ColumnInfo.Add(fieldName + "Time", (timeColumn, time));

                }
                else {
                    string columnId = $"_C{i++}";

                    DataColumn dataColumn = _dataTable.Columns.Add(columnId, FormUtils.GetFieldType(field.FieldType, field.SubType), field.Size);

                    Column column = _matrix.AddColumnFromUserTableField(columnId, field, UIAPIRawForm, Application);
                    column.DataBind.Bind(_dataTable.UniqueID, columnId);

                    ColumnInfo.Add(fieldName, (dataColumn, column));
                }
            }

            _matrix.SelectionMode = BoMatrixSelect.ms_Single;
        }
    }
}