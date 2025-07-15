using System;
using System.Collections.Generic;
using System.Reflection;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Enums;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private void EventSubscriber() {
            Application.RightClickEvent += ApplicationOnRightClickEvent;
            Application.MenuEvent += Application_MenuEvent;
            _observableData.CollectionChanged += DataChanged;
            Application.ItemEvent += Application_ItemEvent;

            if (_saveButton != null) {
                _saveButton.ClickBefore += SaveButtonOnClickBefore;
            }
        }
        private void Application_ItemEvent(string formUid, ref ItemEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;
            if (pVal.FormUID != UniqueID) return;

            if (pVal.EventType != BoEventTypes.et_VALIDATE || pVal.BeforeAction || pVal.ItemUID != _matrix.Item.UniqueID) return;

            int rowIndex = pVal.Row - 1;
            string coluid = pVal.ColUID;

            if (rowIndex < 0 || rowIndex >= _data.Count) return;

            (T item, Status status) = _data[rowIndex];

            foreach ((PropertyInfo property, IUserTableField field) in UserTableMetadataCache.GetUserFields(typeof(T))) {
                if (field is DateTimeUserTableFieldAttribute dt) { }
                else {
                    if (!ColumnInfo.TryGetValue(field.Name, out (DataColumn DataTableColumn, Column MatrixColumn) value)) continue;

                    (DataColumn _, Column matrixColumn) = value;
                    if (matrixColumn.UniqueID != coluid) continue;
                    object newValue = field.ParseValue(_dataTable.GetValue(coluid, rowIndex));
                    object oldValue = property.GetValue(item);
                    if (Equals(oldValue, newValue)) continue;

                    property.SetValue(item, newValue);

                    if (status != Status.Delete && status != Status.NewDelete) {
                        _data[rowIndex] = (item, Status.Modified);
                    }
                    UpdateMatrixColors();
                }
            }
            foreach (KeyValuePair<string, (DataColumn DataTableColumn, Column MatrixColumn)> entry in ColumnInfo) {
                if (entry.Value.MatrixColumn.UniqueID != coluid) continue;

                PropertyInfo property = typeof(T).GetProperty(entry.Key);
                if (property == null) continue;

                object newValue = _dataTable.GetValue(coluid, rowIndex);
                object parsedValue = Convert.ChangeType(newValue, property.PropertyType);

                object oldValue = property.GetValue(item);
                if (!Equals(oldValue, parsedValue)) {
                    property.SetValue(item, parsedValue);

                    if (status == Status.Normal) {
                        _data[rowIndex] = (item, Status.Modified);
                        UpdateMatrixColors();
                    }
                }

                break;
            }
        }
        private void EventUnsubscriber() {
            Application.RightClickEvent -= ApplicationOnRightClickEvent;
            Application.MenuEvent -= Application_MenuEvent;
            _observableData.CollectionChanged -= DataChanged;
            Application.ItemEvent -= Application_ItemEvent;

        }
        private void Application_MenuEvent(ref MenuEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;

            if (pVal.BeforeAction) return;
            if (Application.Forms.ActiveForm.UniqueID != UniqueID) return;
            switch (pVal.MenuUID) {
                case "My_AddRow":
                    _observableData.Add(new T());
                    UpdateMatrix();
                    break;
                case "My_DeleteRow":
                {
                    int rowIndex = _matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                    if (rowIndex <= 0) return;
                    (T item, Status status) = _data[rowIndex - 1];

                    if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T))) {
                        (T Item, Status Status) data = _data[rowIndex - 1];
                        ISoftDeletable t = (ISoftDeletable)data.Item;
                        t.Active = !((ISoftDeletable)_data[rowIndex - 1].Item).Active;
                        _matrix.SelectRow(rowIndex, false, false);
                        if (status == Status.New || status == Status.NewDelete) {
                            _data[rowIndex - 1] = (item, t.Active ? Status.New : Status.NewDelete);
                        }
                        else {
                            _data[rowIndex - 1] = (item, t.Active ? Status.Modified : Status.Delete);
                        }
                        UpdateMatrix();
                    }
                    else {
                        if (status == Status.New || status == Status.NewDelete) {
                            _data[rowIndex - 1] = (item, status == Status.NewDelete ? Status.New : Status.NewDelete);
                        }
                        else {
                            _data[rowIndex - 1] = (item, status == Status.Delete ? Status.Modified : Status.Delete);
                        }
                        UpdateMatrixColors();
                        _matrix.SelectRow(rowIndex, false, false);
                    }
                    break;
                }
            }
        }
        private void ApplicationOnRightClickEvent(ref ContextMenuInfo eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (eventInfo.EventType) {
                case BoEventTypes.et_RIGHT_CLICK when eventInfo.BeforeAction:
                {
                    if (eventInfo.ItemUID == _matrix.Item.UniqueID) {
                        AddContextMenuItems();
                    }
                    break;
                }
                case BoEventTypes.et_FORM_UNLOAD when !eventInfo.BeforeAction:
                    RemoveContextMenuItems();
                    break;
            }
        }
        private void SaveButtonOnClickBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent) {
            bubbleEvent = false;
            int messageBox = Application.MessageBox(
                "Los cambios serán guardados\n¿Desea continuar?", 2,
                "Sí", "No");
            if (messageBox == 2) return;
            bubbleEvent = true;

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