using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private void LoadData() {
            _dataReload = true;
            _observableData.Clear();
            List<T> items = UserTableObjectModel.GetAll<T>();
            items.ForEach(e => _observableData.Add(e));
            for (int i = 0; i < _data.Count; i++) {
                (T item, _) = _data[i];
                _data[i] = (item, Status.Normal);
            }
            _dataReload = false;
        }
        private void SaveChanges() {
            List<(T Item, Status Status)> modifiedData = _data.Where(x => x.Status != Status.Normal).ToList();

            T[] updateItems = modifiedData.Where(x => x.Status == Status.Modified).Select(x => x.Item).ToArray();
            T[] deleteItems = modifiedData.Where(x => x.Status == Status.Delete).Select(x => x.Item).ToArray();
            T[] addItems = modifiedData.Where(x => x.Status == Status.New).Select(x => x.Item).ToArray();

            foreach (T item in updateItems) {
                item.Update();
            }
            foreach (T item in deleteItems) {
                item.Delete(true);
            }
            foreach (T item in addItems) {
                item.Add();
            }
        }
        private void UpdateMatrix() {
            Freeze(true);
            int index = 0;
            _dataTable.Rows.Clear();
            foreach ((T item, Status _) in _data) {
                _dataTable.Rows.Add();
                _dataTable.SetValue("#", index, index + 1);
                _dataTable.SetValue("Code", index, item.Code ?? "");
                _dataTable.SetValue("Name", index, item.Name ?? "");
                foreach ((PropertyInfo property, IUserTableField field) in UserTableMetadataCache.GetUserFields(typeof(T))) {
                    if (field is DateTimeUserTableFieldAttribute dt) {
                        if (!(property.GetValue(item) is DateTime value)) continue;
                        (DataColumn _, Column dateColumn) = ColumnInfo[$"{field.Name}Date"];
                        (DataColumn _, Column timeColumn) = ColumnInfo[$"{field.Name}Time"];
                        _dataTable.SetValue(dateColumn.UniqueID, index, value.ToString("yyyyMMdd"));
                        _dataTable.SetValue(timeColumn.UniqueID, index, value.ToString("HHmm"));

                    }
                    else {
                        (DataColumn _, Column matrixColumn) = ColumnInfo[field.Name];
                        _dataTable.SetValue(matrixColumn.UniqueID, index, field.ToSapData(property.GetValue(item)));
                    }
                }
                index++;
            }

            _matrix.LoadFromDataSourceEx();
            _matrix.AutoResizeColumns();

            if (_tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual) {
                index = 0;
                foreach ((T _, Status status) in _data) {
                    if (status == Status.New || status == Status.NewDelete) {
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
        private void UpdateMatrixColors() {
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
                    case Status.New:
                        _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.PaleGreen));
                        break;
                    case Status.NewDelete:
                        _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.LightSalmon));
                        break;
                    case Status.Delete:
                        _matrix.CommonSetting.SetRowBackColor(i + 1, SapColors.ColorToInt(Color.IndianRed));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (manualCode) {
                    if (!(_data[i].Status == Status.New || _data[i].Status == Status.NewDelete)) {
                        _matrix.CommonSetting.SetCellBackColor(i + 1, 1, SapColors.ColorToInt(SapColors.DisabledCellGray));
                    }
                }
                else {
                    _matrix.CommonSetting.SetCellBackColor(i + 1, 1, SapColors.ColorToInt(SapColors.DisabledCellGray));
                }
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