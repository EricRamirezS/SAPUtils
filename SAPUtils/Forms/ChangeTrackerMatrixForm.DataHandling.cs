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
            try {
                if (rowIndex < 0 || rowIndex >= _data.Count) return;
                if (freeze) Freeze(true);

                bool manualCode = _tableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual;
                int matrixRow = rowIndex + 1;
                int columnCount = _matrix.Columns.Count;

                (T item, Status status) = _data[rowIndex];

                if (!IsEditable(item)) {
                    _matrix.CommonSetting.SetRowBackColor(matrixRow, SapColors.DisabledCellSapGray);
                }
                else {
                    (int rowColor, int rowDisabledColor) = GetRowColors(status, item);

                    // Skip "#" (0) and "_S_T_A_T_E" (last)
                    for (int j = 1; j < columnCount - 1; j++) {
                        int cellColor = !_matrix.CommonSetting.GetCellEditable(matrixRow, j)
                            ? rowDisabledColor
                            : rowColor;

                        _matrix.CommonSetting.SetCellBackColor(matrixRow, j, cellColor);
                    }


                    bool cellEditable = _matrix.CommonSetting.GetCellEditable(matrixRow, 1);
                    if (manualCode && cellEditable) {
                        if (status == Status.New || status == Status.Discard)
                            return;
                    }

                    _matrix.CommonSetting.SetCellBackColor(matrixRow, 1, rowDisabledColor);
                    _matrix.CommonSetting.SetCellFontStyle(matrixRow, 1, BoFontStyle.fs_Bold);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            finally {
                if (freeze) Freeze(false);
            }
        }

        private static (int normal, int dark) GetRowColors(Status status, T item) {
            if (status != Status.Normal) {
                return MatrixColors.StatusColors.TryGetValue(status, out (int Normal, int Dark) colors)
                    ? colors
                    : throw new ArgumentOutOfRangeException();
            }
            if (item is ISoftDeletable sd && !sd.Active) {
                return MatrixColors.InactiveColors;
            }
            return MatrixColors.StatusColors.TryGetValue(Status.Normal, out (int Normal, int Dark) normalColors)
                ? normalColors
                : throw new ArgumentOutOfRangeException();


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
                int observableIndex = _observableData.IndexOf(_observableData.FirstOrDefault(x => x.Code == failed.Item.Code));
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

            List<(T Item, Status Status)> modifiedData = _data.Where(x => x.Status != Status.Normal).ToList();


            int success = 0;
            int failed = 0;
            foreach (Status status in new[] { Status.Modified, Status.ModifiedRestored, Status.Delete, Status.New, }) {

                T[] items = modifiedData
                    .Where(x => x.Status == status)
                    .Select(x => x.Item)
                    .ToArray();
                foreach (T i in items) {
                    bool ok = false;
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (status) {
                        case Status.Modified: ok = i.Update(); break;
                        case Status.ModifiedRestored: ok = i.Update(true); break;
                        case Status.Delete: ok = i.Delete(); break;
                        case Status.New: ok = i.Add(); break;
                    }
                    if (ok) success++;
                    else {
                        failed++;
                        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                        switch (status) {
                            case Status.Modified:
                            case Status.ModifiedRestored: _failedUpdate.Add((i, status)); break;
                            case Status.Delete: _failedDelete.Add((i, status)); break;
                            case Status.New: _failedAdd.Add((i, status)); break;
                        }
                    }
                }
            }

            if (failed > 0 && success == 0) {
                SetStatusBarMessage("No se han podido guardar los cambios.", type: BoStatusBarMessageType.smt_Error);
            }
            if (failed > 0) {
                SetStatusBarMessage($"Se han guardado {success} cambios. Han fallado {failed} cambios.", type: BoStatusBarMessageType.smt_Warning);
            }
            else {
                SetStatusBarMessage($"Se han guardado {success} cambios.", type: BoStatusBarMessageType.smt_Success);
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

                            (T Item, Status Status) tuple = _data.First(x => EqualityComparer<T>.Default.Equals(x.Item, item));
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
        private static readonly int DarkSoftDeletedColor = SapColors.ColorToInt(SapColors.DarkenColor(Color.LightSlateGray));

        internal static (int Normal, int Dark) InactiveColors = (SoftDeletedColor, DarkSoftDeletedColor);

        internal static readonly Dictionary<Status, (int Normal, int Dark)> StatusColors = new Dictionary<Status, (int Normal, int Dark)> {
            [Status.Modified] = (KhakiColor, DarkKhakiColor),
            [Status.ModifiedRestored] = (BlueColor, DarkBlueColor),
            [Status.New] = (GreenColor, DarkGreenColor),
            [Status.Discard] = (SalmonColor, DarkSalmonColor),
            [Status.Delete] = (RedColor, DarkRedColor),
            [Status.Normal] = (WhiteColor, GrayColor),
        };
    }
}