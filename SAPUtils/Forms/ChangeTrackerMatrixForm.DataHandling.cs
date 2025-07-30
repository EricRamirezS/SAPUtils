using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Enums;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;
using SAPUtils.Utils;

namespace SAPUtils.Forms {
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract partial class ChangeTrackerMatrixForm<T> {

        /// <summary>
        /// Loads custom data for the matrix. The implementation of this method can be overridden in derived classes
        /// to provide specific data handling logic.
        /// Typically used for fetching and returning a customized list of data items to be displayed in the matrix.
        /// </summary>
        /// <returns>
        /// A list of data items of type <typeparamref name="T"/>. Returns null by default if not implemented.
        /// </returns>
        /// <seealso cref="UserTableObjectModel.GetAll{T}"/>
        virtual protected List<T> LoadCustomData() {
            return null;
        }

        private void AddRowToMatrix(int index, T item, Status status) {
            if (index < 0) {
                index = _dataTable.Rows.Count;
            }
            _dataTable.Rows.Add();
            _dataTable.SetValue("#", index, index + 1);
            _dataTable.SetValue("Code", index, item.Code ?? "");
            _dataTable.SetValue("Name", index, item.Name ?? "");
            foreach ((PropertyInfo property, IUserTableField field) in UserTableMetadataCache.GetUserFields(typeof(T))) {
                if (field is DateTimeFieldAttribute dtf) {
                    if (!(property.GetValue(item) is DateTime value)) continue;
                    (DataColumn _, Column dateColumn) = ColumnInfo[$"{field.Name ?? property.Name}Date"];
                    (DataColumn _, Column timeColumn) = ColumnInfo[$"{field.Name ?? property.Name}Time"];
                    _dataTable.SetValue(dateColumn.UniqueID, index, dtf.DateToColumnData(value));
                    _dataTable.SetValue(timeColumn.UniqueID, index, dtf.TimeToColumnData(value));
                }
                else {
                    (DataColumn _, Column matrixColumn) = ColumnInfo[field.Name ?? property.Name];
                    _dataTable.SetValue(matrixColumn.UniqueID, index, field.ToColumnData(property.GetValue(item)));
                }
            }
            _dataTable.SetValue("_S_T_A_T_E", index, status.GetReadableName());
        }

        /// <summary>
        /// Updates the matrix control by refreshing its data and applying necessary configurations.
        /// This includes populating the data table, adjusting column values, handling custom properties,
        /// updating the matrix layout, and setting status column widths.
        /// </summary>
        protected void UpdateMatrix() {
            Freeze(true);
            int index = 0;
            _dataTable.Rows.Clear();
            foreach ((T item, Status status) in _data) {
                AddRowToMatrix(index, item, status);
                ;
                index++;
            }

            _matrix.LoadFromDataSourceEx();
            _matrix.AutoResizeColumns();

            index = 0;
            foreach ((T data, Status status) in _data) {
                if (IsEditable(data)) {
                    for (int j = 1; j < _matrix.Columns.Count - 1; j++) {
                        bool editableCol = _matrix.Columns.Item(j).Editable;
                        _matrix.CommonSetting.SetCellEditable(index + 1, 1, editableCol);
                    }
                    if (_tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual) {
                        if (status == Status.New || status == Status.Discard) {
                            _matrix.CommonSetting.SetCellEditable(index + 1, 1, true);
                        }
                        else {
                            _matrix.CommonSetting.SetCellEditable(index + 1, 1, false);
                        }
                    }
                }
                else {
                    _matrix.CommonSetting.SetRowEditable(index + 1, false);
                }
                index++;
            }


            UpdateMatrixColors();
            Freeze(false);
        }

        /// <summary>
        /// Updates the color formatting of the matrix rows based on specific conditions,
        /// such as the item's current state or primary key strategy.
        /// This method ensures visual distinction for different data states.
        /// </summary>
        /// <exception cref="Exception">
        /// Logs any unhandled exceptions that occur during the color update process.
        /// </exception>
        /// <remarks>
        /// This method is typically invoked after modifications to the matrix data to
        /// reflect changes visually in the user interface.
        /// </remarks>
        /// <seealso cref="ChangeTrackerMatrixForm{T}"/>
        protected void UpdateMatrixColors() {
            try {
                Freeze(true);
                bool manualCode = _tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual;
                int rowCount = _data.Count;
                int columnCount = _matrix.Columns.Count;

                int grayColor = SapColors.ColorToInt(SapColors.DisabledCellGray);
                int whiteColor = SapColors.ColorToInt(Color.WhiteSmoke);
                int khakiColor = SapColors.ColorToInt(Color.Khaki);
                int blueColor = SapColors.ColorToInt(Color.LightBlue);
                int greenColor = SapColors.ColorToInt(Color.PaleGreen);
                int salmonColor = SapColors.ColorToInt(Color.LightSalmon);
                int redColor = SapColors.ColorToInt(Color.IndianRed);
                int softDeletedColor = SapColors.ColorToInt(Color.LightSlateGray);
                for (int i = 0; i < rowCount; i++) {
                    (T Item, Status Status) row = _data[i];
                    int rowIndex = i + 1;

                    if (!IsEditable(row.Item)) {
                        _matrix.CommonSetting.SetRowBackColor(rowIndex, grayColor);
                        continue;
                    }

                    int color;
                    switch (row.Status) {
                        case Status.Normal:
                            if (row.Item is ISoftDeletable sd && !sd.Active)
                                color = softDeletedColor;
                            else
                                color = whiteColor;
                            break;
                        case Status.Modified: color = khakiColor; break;
                        case Status.ModifiedRestored: color = blueColor; break;
                        case Status.New: color = greenColor; break;
                        case Status.Discard: color = salmonColor; break;
                        case Status.Delete: color = redColor; break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    _matrix.CommonSetting.SetRowBackColor(rowIndex, color);

                    // Solo si hay más de 2 columnas vale la pena entrar aquí
                    if (columnCount <= 2) continue;
                    for (int j = 1; j < columnCount - 1; j++) {
                        if (!_matrix.CommonSetting.GetCellEditable(rowIndex, j)) {
                            _matrix.CommonSetting.SetCellBackColor(rowIndex, j, grayColor);
                        }
                    }
                }

                for (int i = 0; i < rowCount; i++) {
                    int rowIndex = i + 1;

                    bool cellEditable = _matrix.CommonSetting.GetCellEditable(rowIndex, 1);

                    if (manualCode && cellEditable) {
                        Status status = _data[i].Status;
                        if (status == Status.New || status == Status.Discard)
                            continue;
                    }

                    _matrix.CommonSetting.SetCellBackColor(rowIndex, 1, grayColor);
                    _matrix.CommonSetting.SetCellFontStyle(rowIndex, 1, BoFontStyle.fs_Bold);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            finally {
                Freeze(false);
            }
        }

        private void LoadData() {
            _dataReload = true;
            _observableData.Clear();
            List<T> items = LoadCustomData() ?? UserTableObjectModel.GetAll<T>();

            items.ForEach(e => _observableData.Add(e));
            for (int i = 0; i < _data.Count; i++) {
                (T item, _) = _data[i];
                _data[i] = (item, Status.Normal);
            }

            foreach ((T Item, Status Status) failed in _failedUpdate.Concat(_failedDelete)) {
                int observableIndex = _observableData.ToList().FindIndex(item => item.Code == failed.Item.Code);
                if (observableIndex < 0) continue;

                _observableData[observableIndex] = failed.Item;

                int dataIndex = _data.FindIndex(item => item.Item.Code == failed.Item.Code);
                if (dataIndex < 0) continue;

                _data[dataIndex] = (failed.Item, failed.Status);
            }

            _failedAdd.ForEach(e => _observableData.Add(e.Item));
            _failedAdd.Clear();
            _failedUpdate.Clear();
            _failedDelete.Clear();
            _dataReload = false;
        }

        private void SaveChanges() {
            _helper.Item.Click();

            List<(T Item, Status Status)> modifiedData = _data.Where(x => x.Status != Status.Normal).ToList();

            T[] updateItems = modifiedData.Where(x => x.Status == Status.Modified).Select(x => x.Item).ToArray();
            T[] restoredItems = modifiedData.Where(x => x.Status == Status.ModifiedRestored).Select(x => x.Item).ToArray();
            T[] deleteItems = modifiedData.Where(x => x.Status == Status.Delete).Select(x => x.Item).ToArray();
            T[] addItems = modifiedData.Where(x => x.Status == Status.New).Select(x => x.Item).ToArray();


            foreach (T item in updateItems) {
                if (item.Update()) continue;
                _failedUpdate.Add((item, Status.Modified));
            }
            foreach (T item in restoredItems) {
                if (item.Update(true)) continue;
                _failedUpdate.Add((item, Status.ModifiedRestored));
            }
            foreach (T item in deleteItems) {
                if (item.Delete()) continue;
                _failedDelete.Add((item, Status.Delete));
            }
            foreach (T item in addItems) {
                if (item.Add()) continue;
                _failedAdd.Add((item, Status.New));
            }
        }

        private void DataChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null) {
                        for (int i = 0; i < e.NewItems.Count; i++) {
                            T newItem = (T)e.NewItems[i];
                            int insertIndex = e.NewStartingIndex + i;
                            _data.Insert(insertIndex, (newItem, Status.New));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null) {
                        foreach (T oldItem in e.OldItems) {
                            int index = _data.FindIndex(x => EqualityComparer<T>.Default.Equals(x.Item, oldItem));
                            if (index >= 0)
                                _data.RemoveAt(index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null && e.NewItems != null) {
                        for (int i = 0; i < e.NewItems.Count; i++) {
                            T newItem = (T)e.NewItems[i];
                            int index = e.OldStartingIndex + i;

                            Status oldStatus = _data[index].Status;

                            _data[index] = (newItem, oldStatus);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldItems != null) {
                        for (int i = 0; i < e.OldItems.Count; i++) {
                            T item = (T)e.OldItems[i];
                            int newIndex = e.NewStartingIndex + i;

                            (T Item, Status Status) tuple = _data.First(x => EqualityComparer<T>.Default.Equals(x.Item, item));
                            _data.Remove(tuple);
                            _data.Insert(newIndex, tuple);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _data.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (!_dataReload) {
                UpdateMatrix();
            }
        }
    }
}