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

                index++;
            }

            _matrix.LoadFromDataSourceEx();
            _matrix.AutoResizeColumns();

            if (_tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual) {
                index = 0;
                foreach ((T _, Status status) in _data) {
                    if (status == Status.New || status == Status.Discard) {
                        _matrix.CommonSetting.SetCellEditable(index + 1, 1, true);
                    }
                    else {
                        _matrix.CommonSetting.SetCellEditable(index + 1, 1, false);
                    }

                    index++;
                }
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
                bool manualCode = _tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual;
                for (int i = 0; i < _data.Count; i++) {
                    switch (_data[i].Status) {
                        case Status.Normal:
                            if (_data[i].Item is ISoftDeletable sd && sd.Active == false) {
                                _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.LightSlateGray));
                            }
                            else {
                                _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.WhiteSmoke));
                            }
                            break;
                        case Status.Modified:
                            _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.Khaki));
                            break;
                        case Status.ModifiedRestored:
                            _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.LightBlue));
                            break;
                        case Status.New:
                            _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.PaleGreen));
                            break;
                        case Status.Discard:
                            _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.LightSalmon));
                            break;
                        case Status.Delete:
                            _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.IndianRed));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    for (int j = 1; j < _matrix.Columns.Count - 1; j++) {
                        bool cellEditable = _matrix.CommonSetting.GetCellEditable(i + 1, j);
                        if (!cellEditable) {
                            _matrix.CommonSetting.SetCellBackColor(i + 1, j, SapColors.ColorToInt(SapColors.DisabledCellGray));
                        }
                    }
                }

                for (int i = 0; i < _data.Count; i++) {
                    if (manualCode) {
                        if (_data[i].Status == Status.New || _data[i].Status == Status.Discard) continue;
                    }
                    _matrix.CommonSetting.SetCellBackColor(i + 1, 1, SapColors.ColorToInt(SapColors.DisabledCellGray));
                    _matrix.CommonSetting.SetCellFontStyle(i + 1, 1, BoFontStyle.fs_Bold);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
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

            _failedData.ForEach(e => _observableData.Add(e.Item));
            for (int i = 0; i < _failedData.Count; i++) {
                (T item, Status status) = _failedData[_data.Count + i];
                _data[_data.Count + i] = (item, status);
            }

            _failedData.Clear();
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
                _failedData.Add((item, Status.Modified));
                ;
            }
            foreach (T item in restoredItems) {
                if (item.Update(true)) continue;
                _failedData.Add((item, Status.ModifiedRestored));
                ;
            }
            foreach (T item in deleteItems) {
                if (item.Delete()) continue;
                _failedData.Add((item, Status.Delete));
                ;
            }
            foreach (T item in addItems) {
                if (item.Add()) continue;
                _failedData.Add((item, Status.New));
                ;
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

                            _data[index] = (newItem, oldStatus); // O Status.New si prefieres
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