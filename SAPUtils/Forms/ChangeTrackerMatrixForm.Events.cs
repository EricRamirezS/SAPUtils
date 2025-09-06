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
using SAPUtils.I18N;
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

            if (_exportButton != null) {
                _exportButton.ClickAfter += ExportToExcel;
            }

            if (_importButton != null) {
                _importButton.ClickAfter += ImportFromExcel;
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

            if (pVal.EventType != BoEventTypes.et_VALIDATE || pVal.BeforeAction ||
                pVal.ItemUID != _matrix.Item.UniqueID || _dataReload) return;

            int rowIndex = pVal.Row - 1;
            string coluid = pVal.ColUID;
            Cell cell = _matrix.Columns.Item(pVal.ColUID).Cells.Item(pVal.Row);

            if (rowIndex < 0 || rowIndex >= _data.Count) return;

            (T item, Status status) = _data[rowIndex];
            // ReSharper disable LocalizableElement
            if (coluid == "Code" || coluid == "Name") {
                // ReSharper restore LocalizableElement
                string value = "";
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
                _dataTable.SetValue(coluid, rowIndex, value);
                if (status != Status.Normal) return;
                _data[rowIndex] = (item, Status.Modify);
                ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modify.GetReadableName();
                UpdateMatrixColors(pVal.Row - 1);
            }
            // ReSharper disable LocalizableElement
            else if (coluid.EndsWith("D") || coluid.EndsWith("T")) {
                // ReSharper restore LocalizableElement
                // DateTimeUserTableField Handler
                KeyValuePair<string, (DataColumn DataTableColumn, Column MatrixColumn)> columnInfo =
                    ColumnInfo.FirstOrDefault(e => e.Value.MatrixColumn.UniqueID == coluid);
                if (columnInfo.Key == null) return;
                string propertyName = columnInfo.Key.Substring(0, columnInfo.Key.Length - 4);
                (PropertyInfo Property, IUserTableField Field) propertyField =
                    UserTableMetadataCache.GetUserField(typeof(T), propertyName);
                if (propertyField == default) return;
                if (!(propertyField.Field is DateTimeFieldAttribute dtf)) return;
                object cellValue = cell.GetValue();
                object oldValue = propertyField.Property.GetValue(item);
                DateTime oldDateTime = (DateTime)oldValue;

                // ReSharper disable LocalizableElement
                if (coluid.EndsWith("D")) {
                    // ReSharper restore LocalizableElement
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
                    _dataTable.SetValue(coluid, rowIndex, dtf.DateToColumnData(newDate));
                    if (status != Status.Normal) return;
                    _data[rowIndex] = (item, Status.Modify);
                    ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modify.GetReadableName();
                    UpdateMatrixColors(pVal.Row - 1);
                }
                // ReSharper disable LocalizableElement
                else if (coluid.EndsWith("T")) {
                    // ReSharper restore LocalizableElement
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
                    _dataTable.SetValue(coluid, rowIndex, dtf.TimeToColumnData(newTime));
                    if (status != Status.Normal) return;
                    _data[rowIndex] = (item, Status.Modify);
                    ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modify.GetReadableName();
                    UpdateMatrixColors(pVal.Row - 1);
                }
            }
            else {
                KeyValuePair<string, (DataColumn DataTableColumn, Column MatrixColumn)> columnInfo =
                    ColumnInfo.FirstOrDefault(e => e.Value.MatrixColumn.UniqueID == coluid);
                if (columnInfo.Key == null) return;
                (PropertyInfo Property, IUserTableField Field) propertyField =
                    UserTableMetadataCache.GetUserField(typeof(T), columnInfo.Key);
                if (propertyField == default) return;

                object cellValue = cell.GetValue();
                object newValue = propertyField.Field.ParseValue(cellValue);
                object oldValue = propertyField.Property.GetValue(item);

                if (Equals(oldValue, newValue)) return;
                propertyField.Property.SetValue(item, newValue);
                _dataTable.SetValue(coluid, rowIndex, propertyField.Field.ToColumnData(newValue));

                if (status != Status.Normal) return;
                _data[rowIndex] = (item, Status.Modify);
                ((EditText)_stateColumn.Cells.Item(pVal.Row).Specific).Value = Status.Modify.GetReadableName();
                UpdateMatrixColors(pVal.Row - 1);
            }
        }

        private void Application_MenuEvent(ref MenuEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;

            if (pVal.BeforeAction) return;
            if (Application.Forms.ActiveForm.UniqueID != UniqueID) return;
            try {
                Freeze(true);
                // ReSharper disable LocalizableElement
                if (pVal.MenuUID == "1282" || pVal.MenuUID == _addRowMenuUid) {
                    // ReSharper restore LocalizableElement
                    T it = new T();
                    if (it is ISoftDeletable itsd) itsd.Active = true;
                    _observableData.Add(it);
                }
                else if (pVal.MenuUID == _deleteRowMenuUid) {
                    int rowIndex = _matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                    if (rowIndex <= 0) return;
                    (T item, Status status) = _data[rowIndex - 1];
                    switch (item) {
                        // ReSharper disable once RedundantBoolCompare
                        case ISoftDeletable sd when sd.Active == false && status == Status.Normal:
                            sd.Active = true;
                            _data[rowIndex - 1] = (item, Status.Restore);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value =
                                Status.Restore.GetReadableName();
                            break;
                        case ISoftDeletable sd when sd.Active && status == Status.Restore:
                            sd.Active = false;
                            _data[rowIndex - 1] = (item, Status.Delete);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value =
                                Status.Delete.GetReadableName();
                            break;
                        case ISoftDeletable sd when sd.Active && status == Status.New:
                            sd.Active = false;
                            _data[rowIndex - 1] = (item, Status.Discard);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value =
                                Status.Discard.GetReadableName();
                            break;
                        // ReSharper disable once RedundantBoolCompare
                        case ISoftDeletable sd when sd.Active == false && status == Status.Discard:
                            sd.Active = false;
                            _data[rowIndex - 1] = (item, Status.New);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value = Status.New.GetReadableName();
                            break;
                        case ISoftDeletable sd
                            // ReSharper disable once RedundantBoolCompare
                            when sd.Active == false && item is UserTableObjectModel utom &&
                                 utom.OriginalActive == false &&
                                 status == Status.Delete:
                            sd.Active = false;
                            _data[rowIndex - 1] = (item, Status.Restore);
                            ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value =
                                Status.Restore.GetReadableName();
                            break;
                        default:
                            if (status == Status.New || status == Status.Discard) {
                                Status updated = status == Status.Discard ? Status.New : Status.Discard;
                                _data[rowIndex - 1] = (item, updated);
                                ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value =
                                    updated.GetReadableName();
                            }
                            else {
                                Status updated = status == Status.Delete ? Status.Modify : Status.Delete;
                                _data[rowIndex - 1] = (item, updated);
                                ((EditText)_stateColumn.Cells.Item(rowIndex).Specific).Value =
                                    updated.GetReadableName();
                            }

                            break;
                    }

                    UpdateMatrixColors(rowIndex - 1);
                    _matrix.SelectRow(rowIndex, false, false);
                }
                // ReSharper disable LocalizableElement
                else if (pVal.MenuUID == "1304") {
                    // ReSharper restore LocalizableElement
                    if (UnsavedChanges()) {
                        int messageBox = Application.MessageBox(
                            Texts.ChangeTrackerMatrixForm_Application_MenuEvent_There_are_unsaved_changes__Do_you_want_to_reload_and_discard_the_changes_, 2,
                            Texts.ChangeTrackerMatrixForm_Application_MenuEvent_Yes,
                            Texts.ChangeTrackerMatrixForm_Application_MenuEvent_Cancel);

                        if (messageBox == 2) return;
                    }

                    LoadData();
                    UpdateMatrix();
                }
            }
            finally {
                Freeze(false);
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
                Texts.ChangeTrackerMatrixForm_SaveButtonOnClickAfter_The_changes_will_be_saved__Do_you_want_to_continue_, 2,
                Texts.ChangeTrackerMatrixForm_SaveButtonOnClickAfter_Yes, Texts.ChangeTrackerMatrixForm_SaveButtonOnClickAfter_No);
            if (messageBox == 2) return;

            SaveChanges();

            LoadData();
            UpdateMatrix();
        }

        /// <inheritdoc />
        protected override void OnFormCloseBefore(SBOItemEventArg pVal, out bool bubbleEvent) {
            base.OnFormCloseBefore(pVal, out bubbleEvent);
            bubbleEvent = true;
            if (!UnsavedChanges()) return;

            int messageBox = Application.MessageBox(
                Texts.ChangeTrackerMatrixForm_OnFormCloseBefore_There_are_unsaved_changes__Do_you_want_to_save_before_closing_, 3,
                Texts.ChangeTrackerMatrixForm_OnFormCloseBefore_Yes,
                Texts.ChangeTrackerMatrixForm_SaveButtonOnClickAfter_No,
                Texts.ChangeTrackerMatrixForm_OnFormCloseBefore_Cancel);

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
        protected override void OnFormCloseAfter(SBOItemEventArg pVal) {
            base.OnFormCloseAfter(pVal);
            RemoveContextMenuItems();
            EventUnsubscriber();
        }
    }
}