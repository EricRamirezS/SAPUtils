using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Enums;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Extensions;
using SAPUtils.Models.UserTables;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private void EventSubscriber() {
            Application.RightClickEvent += ApplicationOnRightClickEvent;
            Application.MenuEvent += Application_MenuEvent;
            _observableData.CollectionChanged += DataChanged;
            Application.ItemEvent += Application_ItemEvent;

            if (_saveButton != null) {
                _saveButton.ClickAfter += SaveButtonOnClickAfter;
            }
        }
        private void EventUnsubscriber() {
            Application.RightClickEvent -= ApplicationOnRightClickEvent;
            Application.MenuEvent -= Application_MenuEvent;
            _observableData.CollectionChanged -= DataChanged;
            Application.ItemEvent -= Application_ItemEvent;

        }

        private void Application_ItemEvent(string formUid, ref ItemEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;
            if (pVal.FormUID != UniqueID) return;

            if (pVal.EventType != BoEventTypes.et_VALIDATE || pVal.BeforeAction || pVal.ItemUID != _matrix.Item.UniqueID || _dataReload) return;

            int rowIndex = pVal.Row - 1;
            string coluid = pVal.ColUID;
            Cell cell = _matrix.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row);

            if (rowIndex < 0 || rowIndex >= _data.Count) return;

            (T item, Status status) = _data[rowIndex];
            if (coluid == "Code" || coluid == "Name") {
                string value;
                bool changed = false;
                switch (coluid) {
                    case "Code":
                        value = cell.GetValue().ToString();
                        if (value == item.Code) break;
                        changed = true;
                        item.Code = value;
                        break;
                    case "Name":
                        value = cell.GetValue().ToString();
                        if (value == item.Name) break;
                        changed = true;
                        item.Name = value;
                        break;
                }
                if (!changed) return;
                if (status == Status.Normal) {
                    _data[rowIndex] = (item, Status.Modified);
                    ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modified.GetReadableName();
                }
                UpdateMatrixColors();
            }
            else if (coluid.EndsWith("D") || coluid.EndsWith("T")) {
                // DateTimeUserTableField Handler
                KeyValuePair<string, (DataColumn DataTableColumn, Column MatrixColumn)> columnInfo =
                    ColumnInfo.FirstOrDefault(e => e.Value.MatrixColumn.UniqueID == coluid);
                if (columnInfo.Key == null) return;
                string propertyName = columnInfo.Key.Substring(0, columnInfo.Key.Length - 4);
                (PropertyInfo Property, IUserTableField Field) propertyField = UserTableMetadataCache.GetUserField(typeof(T), propertyName);
                if (propertyField == default) return;
                if (!(propertyField.Field is DateTimeFieldAttribute dtf)) return;
                object cellValue = cell.GetValue();
                object oldValue = propertyField.Property.GetValue(item);
                DateTime oldDateTime = (DateTime)oldValue;

                if (coluid.EndsWith("D")) {
                    DateTime newDate = dtf.ParseDateValue(cellValue);
                    if (oldDateTime.Date == newDate.Date) return;
                    DateTime newFinalValue = new DateTime(
                        newDate.Year,
                        newDate.Month,
                        newDate.Day,
                        oldDateTime.Hour,
                        oldDateTime.Minute,
                        oldDateTime.Second
                    );
                    propertyField.Property.SetValue(item, newFinalValue);
                    if (status == Status.Normal) {
                        _data[rowIndex] = (item, Status.Modified);
                        ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modified.GetReadableName();
                    }
                    UpdateMatrixColors();
                }
                else if (coluid.EndsWith("T")) {
                    DateTime newTime = dtf.ParseTimeValue(cellValue);
                    if (oldDateTime.Hour == newTime.Hour && oldDateTime.Minute == newTime.Minute) return;
                    DateTime newFinalValue = new DateTime(
                        oldDateTime.Year,
                        oldDateTime.Month,
                        oldDateTime.Day,
                        newTime.Hour,
                        newTime.Minute,
                        newTime.Second
                    );
                    propertyField.Property.SetValue(item, newFinalValue);
                    if (status == Status.Normal) {
                        _data[rowIndex] = (item, Status.Modified);
                        ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modified.GetReadableName();
                    }
                    UpdateMatrixColors();
                }
            }
            else {
                KeyValuePair<string, (DataColumn DataTableColumn, Column MatrixColumn)> columnInfo =
                    ColumnInfo.FirstOrDefault(e => e.Value.MatrixColumn.UniqueID == coluid);
                if (columnInfo.Key == null) return;
                (PropertyInfo Property, IUserTableField Field) propertyField = UserTableMetadataCache.GetUserField(typeof(T), columnInfo.Key);
                if (propertyField == default) return;

                object cellValue = cell.GetValue();
                object newValue = propertyField.Field.ParseValue(cellValue);
                object oldValue = propertyField.Property.GetValue(item);

                if (Equals(oldValue, newValue)) return;
                propertyField.Property.SetValue(item, newValue);
                if (status == Status.Normal) {
                    _data[rowIndex] = (item, Status.Modified);
                    ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modified.GetReadableName();
                }
                UpdateMatrixColors();
            }
        }
        private void Application_MenuEvent(ref MenuEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;

            if (pVal.BeforeAction) return;
            if (Application.Forms.ActiveForm.UniqueID != UniqueID) return;
            if (pVal.MenuUID == "1282" || pVal.MenuUID == _addRowMenuUid) {
                T it = new T();
                if (it is ISoftDeletable itsd) itsd.Active = true;
                _observableData.Add(it);
                UpdateMatrix();
            }
            else if (pVal.MenuUID == _deleteRowMenuUid) {
                int rowIndex = _matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                if (rowIndex <= 0) return;
                (T item, Status status) = _data[rowIndex - 1];
                switch (item) {
                    case ISoftDeletable sd when sd.Active == false && status == Status.Normal:
                        sd.Active = true;
                        _data[rowIndex - 1] = (item, Status.ModifiedRestored);
                        ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = Status.ModifiedRestored.GetReadableName();
                        break;
                    case ISoftDeletable sd when sd.Active && status == Status.ModifiedRestored:
                        sd.Active = false;
                        _data[rowIndex - 1] = (item, Status.Delete);
                        ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = Status.Delete.GetReadableName();
                        break;
                    case ISoftDeletable sd when sd.Active && status == Status.New:
                        sd.Active = false;
                        _data[rowIndex - 1] = (item, Status.Discard);
                        ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = Status.Discard.GetReadableName();
                        break;
                    case ISoftDeletable sd when sd.Active == false && status == Status.Discard:
                        sd.Active = false;
                        _data[rowIndex - 1] = (item, Status.New);
                        ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = Status.New.GetReadableName();
                        break;
                    case ISoftDeletable sd when sd.Active == false && item.OriginalActive == false && status == Status.Delete:
                        sd.Active = false;
                        _data[rowIndex - 1] = (item, Status.ModifiedRestored);
                        ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = Status.ModifiedRestored.GetReadableName();
                        break;
                    default:
                        if (status == Status.New || status == Status.Discard) {
                            Status updated = status == Status.Discard ? Status.New : Status.Discard;
                            _data[rowIndex - 1] = (item, updated);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = updated.GetReadableName();
                        }
                        else {
                            Status updated = status == Status.Delete ? Status.Modified : Status.Delete;
                            _data[rowIndex - 1] = (item, updated);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = updated.GetReadableName();
                        }
                        break;
                }
                UpdateMatrixColors();
                _matrix.SelectRow(rowIndex, false, false);
            }
            else if (pVal.MenuUID == "1304") {
                if (UnsavedChanges()) {
                    int messageBox = Application.MessageBox(
                        "Hay cambios sin guardar.\n¿Desea recargar y descartar los cambios?", 2,
                        "Sí",
                        "Cancelar");

                    if (messageBox == 2) return;
                }
                LoadData();
                UpdateMatrix();
            }

        }
        private void ApplicationOnRightClickEvent(ref ContextMenuInfo eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (eventInfo.FormUID != UniqueID) return;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (eventInfo.EventType) {
                case BoEventTypes.et_RIGHT_CLICK when eventInfo.BeforeAction:
                {
                    if (eventInfo.ItemUID == _matrix.Item.UniqueID) {
                        AddContextMenuItems();
                    }
                    break;
                }
                default:
                    RemoveContextMenuItems();
                    break;
            }
        }
        private void SaveButtonOnClickAfter(object sboObject, SBOItemEventArg pVal) {
            int messageBox = Application.MessageBox(
                "Los cambios serán guardados\n¿Desea continuar?", 2,
                "Sí", "No");
            if (messageBox == 2) return;

            SaveChanges();

            LoadData();
            UpdateMatrix();
        }

        /// <inheritdoc />
        override protected void OnFormCloseBefore(SBOItemEventArg pVal, out bool bubbleEvent) {
            base.OnFormCloseBefore(pVal, out bubbleEvent);
            bubbleEvent = true;
            if (!UnsavedChanges()) return;

            int messageBox = Application.MessageBox(
                "Hay cambios sin guardar.\n¿Desea guardar antes de cerrar?", 3,
                "Sí",
                "No",
                "Cancelar");

            switch (messageBox) {
                case 1:
                    SaveChanges();
                    break;
                case 2:
                    break;
                case 3:
                    bubbleEvent = false;
                    return;
            }
        }
        /// <inheritdoc />
        override protected void OnFormCloseAfter(SBOItemEventArg pVal) {
            base.OnFormCloseAfter(pVal);
            EventUnsubscriber();
        }
    }
}