using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Enums;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;
using SAPUtils.Utils;

namespace SAPUtils.Forms {
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private static readonly ConcurrentDictionary<PropertyInfo, Func<T, object>> GetterCache =
            new ConcurrentDictionary<PropertyInfo, Func<T, object>>();

        private static readonly
            ConcurrentDictionary<Type, List<(string FieldKey, PropertyInfo Property, Func<T, object> Getter,
                IUserTableField Field)>> FieldCache =
                new ConcurrentDictionary<Type, List<(string FieldKey, PropertyInfo Property, Func<T, object> Getter,
                    IUserTableField Field)>>();

        private Dictionary<string, (DataColumn DataColumn, Column MatrixColumn)> _columnsCache;


        /// <summary>
        /// Loads custom data for the matrix. The implementation of this method can be overridden in derived classes
        /// to provide specific data handling logic.
        /// Typically used for fetching and returning a customized list of data items to be displayed in the matrix.
        /// </summary>
        /// <returns>
        /// A list of data items of type <typeparamref name="T"/>. Returns null by default if not implemented.
        /// </returns>
        /// <seealso cref="UserTableObjectModel.GetAll{T}"/>
        protected virtual List<T> LoadCustomData() {
            return null;
        }

        [Localizable(false)]
        private void AddRowToMatrix(int index, T item, Status status) {
            if (index < 0) index = _dataTable.Rows.Count;

            _dataTable.Rows.Add();
            _dataTable.SetValue("#", index, index + 1);
            _dataTable.SetValue("Code", index, item.Code ?? "");
            _dataTable.SetValue("Name", index, item.Name ?? "");


            if (_columnsCache == null) PrepareColumnsCache();
            if (_columnsCache == null) return;
            List<(string FieldKey, PropertyInfo Property, Func<T, object> Getter, IUserTableField Field)> fields =
                GetCachedFields();

            foreach ((string fieldKey, PropertyInfo _, Func<T, object> getter, IUserTableField field) in fields) {
                if (field is DateTimeFieldAttribute dtf) {
                    if (!(getter(item) is DateTime value)) continue;
                    (DataColumn _, Column dateColumn) = _columnsCache[$"{fieldKey}Date"];
                    (DataColumn _, Column timeColumn) = _columnsCache[$"{fieldKey}Time"];
                    _dataTable.SetValue(dateColumn.UniqueID, index, dtf.DateToColumnData(value));
                    _dataTable.SetValue(timeColumn.UniqueID, index, dtf.TimeToColumnData(value));
                }
                else {
                    (DataColumn _, Column matrixColumn) = _columnsCache[fieldKey];
                    _dataTable.SetValue(matrixColumn.UniqueID, index, field.ToColumnData(getter(item)));
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
                index++;
            }

            _matrix.LoadFromDataSourceEx();
            _matrix.AutoResizeColumns();

            index = 0;
            foreach ((T data, Status status) in _data) {
                SetEditablesInRow(data, status, index);
                index++;
            }


            UpdateMatrixColors();
            Freeze(false);
        }

        private void SetEditablesInRow(T data, Status status, int index) {
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
                else {
                    _matrix.CommonSetting.SetCellEditable(index + 1, 1, false);
                }
            }
            else {
                _matrix.CommonSetting.SetRowEditable(index + 1, false);
            }
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
                for (int i = 0; i < _data.Count; i++) {
                    UpdateMatrixColors(i, freeze: false);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            finally {
                Freeze(false);
            }
        }

        /// <summary>
        /// Updates the visual colors of the matrix rows and cells based on their current data and status.
        /// This method is particularly useful for providing visual feedback on editable or non-editable rows
        /// or indicating specific statuses through color coding.
        /// </summary>
        /// <param name="rowIndex">The index of the matrix row to be updated.</param>
        /// <param name="freeze">A boolean indicating whether the matrix should be frozen during the update
        /// process to prevent UI flickering and improve performance. Defaults to true.</param>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        /// <seealso cref="SAPbouiCOM.CommonSetting"/>
        protected void UpdateMatrixColors(int rowIndex, bool freeze = true) {
            if (rowIndex < 0 || rowIndex >= _data.Count) return;

            try {
                if (freeze) Freeze(true);

                bool manualCode = _tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual;
                int matrixRow = rowIndex + 1;
                int lastColumn = _matrix.Columns.Count - 1;

                (T item, Status status) = _data[rowIndex];

                if (!IsEditable(item)) {
                    _matrix.CommonSetting.SetRowBackColor(matrixRow, SapColors.DisabledCellSapGray);
                    return;
                }

                (int rowColor, int rowDisabledColor) = GetRowColors(status, item);

                // Skip "#" (0) and "_S_T_A_T_E" (last)
                bool[] editableFlags = new bool[lastColumn + 1];
                bool allEditable = true;
                for (int j = 1; j < lastColumn; j++) {
                    editableFlags[j] = _matrix.CommonSetting.GetCellEditable(matrixRow, j);
                    if (!editableFlags[j]) allEditable = false;
                }

                if (allEditable) {
                    _matrix.CommonSetting.SetRowBackColor(matrixRow, rowColor);
                }
                else {
                    _matrix.CommonSetting.SetRowBackColor(matrixRow, rowDisabledColor);

                    for (int j = 1; j < lastColumn; j++) {
                        if (editableFlags[j])
                            _matrix.CommonSetting.SetCellBackColor(matrixRow, j, rowColor);
                    }
                }

                if (!manualCode || !editableFlags[1] || status == Status.New || status == Status.Discard) return;
                _matrix.CommonSetting.SetCellBackColor(matrixRow, 1, rowDisabledColor);
                _matrix.CommonSetting.SetCellFontStyle(matrixRow, 1, BoFontStyle.fs_Bold);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            finally {
                if (freeze) Freeze(false);
            }
        }


        private static (int normal, int dark) GetRowColors(Status status, T item) {
            if (status != Status.Normal)
                return MatrixColors.StatusColors[status];

            if (item is ISoftDeletable sd && !sd.Active)
                return MatrixColors.InactiveColors;

            return MatrixColors.NormalColors;
        }

        private void LoadData(List<T> items) {
            _dataReload = true;
            _observableData.Clear();

            items.ForEach(e => _observableData.Add(e));
            for (int i = 0; i < _data.Count; i++) {
                (T item, _) = _data[i];
                _data[i] = (item, Status.Normal);
            }

            foreach ((T Item, Status Status) failed in _failedUpdate.Concat(_failedDelete)) {
                int observableIndex =
                    _observableData.IndexOf(_observableData.FirstOrDefault(x => x.Code == failed.Item.Code));
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

        private void LoadData() {
            LoadData(LoadCustomData() ?? UserTableObjectModel.GetAll<T>());
        }

        private void SaveChanges() {
            _helper.Item.Click();
            ShowWaitCursor();
            List<(T Item, Status Status)> modifiedData = _data.Where(x => x.Status != Status.Normal).ToList();


            int success = 0;
            int failed = 0;
            foreach (Status status in new[] { Status.Modify, Status.Restore, Status.Delete, Status.New }) {
                T[] items = modifiedData
                    .Where(x => x.Status == status)
                    .Select(x => x.Item)
                    .ToArray();
                foreach (T i in items) {
                    bool ok = false;
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (status) {
                        case Status.Modify: ok = i.Update(); break;
                        case Status.Restore: ok = i.Update(true); break;
                        case Status.Delete: ok = i.Delete(); break;
                        case Status.New: ok = i.Add(); break;
                    }

                    if (ok) success++;
                    else {
                        failed++;
                        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                        switch (status) {
                            case Status.Modify:
                            case Status.Restore: _failedUpdate.Add((i, status)); break;
                            case Status.Delete: _failedDelete.Add((i, status)); break;
                            case Status.New: _failedAdd.Add((i, status)); break;
                        }
                    }
                }
            }

            if (failed > 0 && success == 0) {
                SetStatusBarMessage(Texts.ChangeTrackerMatrixForm_SaveChanges_Changes_could_not_be_saved_, type: BoStatusBarMessageType.smt_Error);
            }

            if (failed > 0) {
                SetStatusBarMessage(string.Format(Texts.ChangeTrackerMatrixForm_SaveChanges__0__changes_have_been_saved___1__changes_have_failed_, success, failed),
                    type: BoStatusBarMessageType.smt_Warning);
            }
            else {
                SetStatusBarMessage(string.Format(Texts.ChangeTrackerMatrixForm_SaveChanges__0__changes_have_been_saved_, success), type: BoStatusBarMessageType.smt_Success);
            }

            ShowArrowCursor();
        }

        private void DataChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null) {
                        for (int i = 0; i < e.NewItems.Count; i++) {
                            T newItem = (T)e.NewItems[i];
                            int insertIndex = e.NewStartingIndex + i;
                            _data.Insert(insertIndex, (newItem, Status.New));
                            if (_dataReload) continue;
                            Freeze(true);
                            AddRowToMatrix(insertIndex, newItem, Status.New);
                            _matrix.LoadFromDataSourceEx();
                            SetEditablesInRow(newItem, Status.New, insertIndex);
                            UpdateMatrixColors(insertIndex, false);
                            Freeze(false);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null) {
                        foreach (T oldItem in e.OldItems) {
                            int index = _data.FindIndex(x => EqualityComparer<T>.Default.Equals(x.Item, oldItem));
                            if (index < 0) continue;
                            _data.RemoveAt(index);
                            if (_dataReload) continue;
                            UpdateMatrix();
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
                            if (_dataReload) continue;
                            UpdateMatrixColors(index);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldItems != null) {
                        for (int i = 0; i < e.OldItems.Count; i++) {
                            T item = (T)e.OldItems[i];
                            int newIndex = e.NewStartingIndex + i;

                            (T Item, Status Status) tuple =
                                _data.First(x => EqualityComparer<T>.Default.Equals(x.Item, item));
                            _data.Remove(tuple);
                            _data.Insert(newIndex, tuple);
                            if (_dataReload) continue;
                            UpdateMatrix();
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    _data.Clear();
                    if (_dataReload) return;
                    UpdateMatrix();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static object GetPropertyValue(PropertyInfo prop, T item) {
            Func<T, object> getter = GetterCache.GetOrAdd(prop,
                p => {
                    ParameterExpression param = Expression.Parameter(typeof(T));
                    UnaryExpression body = Expression.Convert(Expression.Property(param, p), typeof(object));
                    return Expression.Lambda<Func<T, object>>(body, param)
                        .Compile();
                });
            return getter(item);
        }

        private void PrepareColumnsCache() {
            _columnsCache = ColumnInfo.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static List<(string FieldKey, PropertyInfo Property, Func<T, object> Getter, IUserTableField Field)>
            GetCachedFields() {
            return FieldCache.GetOrAdd(typeof(T), type => {
                List<(string, PropertyInfo, Func<T, object>, IUserTableField)> list =
                    new List<(string, PropertyInfo, Func<T, object>, IUserTableField)>();
                foreach ((PropertyInfo prop, IUserTableField field) in UserTableMetadataCache.GetUserFields(type)) {
                    string fieldKey = field.Name ?? prop.Name;
                    // ReSharper disable once LocalizableElement
                    ParameterExpression param = Expression.Parameter(typeof(T), "x");
                    UnaryExpression body = Expression.Convert(Expression.Property(param, prop), typeof(object));
                    Func<T, object> getter = Expression.Lambda<Func<T, object>>(body, param).Compile();

                    list.Add((fieldKey, prop, getter, field));
                }

                return list;
            });
        }
    }

    internal static class MatrixColors {
        private static readonly int WhiteColor = SapColors.ColorToInt(Color.WhiteSmoke);
        private static readonly int GrayColor = SapColors.ColorToInt(SapColors.DisabledCellGray);
        private static readonly int KhakiColor = SapColors.ColorToInt(Color.Khaki);
        private static readonly int DarkKhakiColor = SapColors.ColorToInt(SapColors.DarkenColor(Color.Khaki));
        private static readonly int BlueColor = SapColors.ColorToInt(Color.LightBlue);
        private static readonly int DarkBlueColor = SapColors.ColorToInt(SapColors.DarkenColor(Color.LightBlue));
        private static readonly int GreenColor = SapColors.ColorToInt(Color.PaleGreen);
        private static readonly int DarkGreenColor = SapColors.ColorToInt(SapColors.DarkenColor(Color.PaleGreen));
        private static readonly int SalmonColor = SapColors.ColorToInt(Color.LightSalmon);
        private static readonly int DarkSalmonColor = SapColors.ColorToInt(SapColors.DarkenColor(Color.LightSalmon));
        private static readonly int RedColor = SapColors.ColorToInt(Color.IndianRed);
        private static readonly int DarkRedColor = SapColors.ColorToInt(SapColors.DarkenColor(Color.IndianRed));
        private static readonly int SoftDeletedColor = SapColors.ColorToInt(Color.LightSlateGray);

        private static readonly int DarkSoftDeletedColor =
            SapColors.ColorToInt(SapColors.DarkenColor(Color.LightSlateGray));

        internal static (int Normal, int Dark) InactiveColors = (SoftDeletedColor, DarkSoftDeletedColor);
        internal static readonly (int Normal, int Dark) NormalColors = (WhiteColor, GrayColor);

        internal static readonly Dictionary<Status, (int Normal, int Dark)> StatusColors =
            new Dictionary<Status, (int Normal, int Dark)> {
                [Status.Modify] = (KhakiColor, DarkKhakiColor),
                [Status.Restore] = (BlueColor, DarkBlueColor),
                [Status.New] = (GreenColor, DarkGreenColor),
                [Status.Discard] = (SalmonColor, DarkSalmonColor),
                [Status.Delete] = (RedColor, DarkRedColor),
                [Status.Normal] = (WhiteColor, GrayColor),
            };
    }
}