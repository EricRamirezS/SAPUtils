using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Reflection;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Internal.Attributes.UserTables;
using SAPUtils.Internal.Models;
using SAPUtils.Models.UserTables;
using SAPUtils.Utils;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> : UserForm where T : IUserTableObjectModel, new() {

        private Button _saveButton;
        private Matrix _matrix;
        private DataTable _dataTable;
        private readonly bool _useAddContextButton;
        private readonly bool _userDeleteContextButton;
        private readonly UserTableAttribute _tableAttribute;
        private readonly List<(T Item, Status Status)> _data = new List<(T Item, Status Status)>();
        private readonly ObservableCollection<T> _observableData = new ObservableCollection<T>();

        private bool _dataReload;

        protected readonly Dictionary<string, (DataColumn DataTableColumn, Column MatrixColumn)> ColumnInfo =
            new Dictionary<string, (DataColumn DataTableColumn, Column MatrixColumn)>();

        protected ICollection<T> Data => _observableData;

        protected ChangeTrackerMatrixForm(
            bool useAddContextButton = true,
            bool userDeleteContextButton = true) {
            _useAddContextButton = useAddContextButton;
            _userDeleteContextButton = userDeleteContextButton;
            _tableAttribute = UserTableMetadataCache.GetUserTableAttribute(typeof(T));

            CustomInitializeComponent();

            EventSubscriber();

            LoadData();

            UpdateMatrix();

            (DataColumn DataTableColumn, Column MatrixColumn) value;
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T))) {
                if (ColumnInfo.TryGetValue("Active", out value)) value.MatrixColumn.Visible = true;
            }
            if (typeof(IAuditableDate).IsAssignableFrom(typeof(T))) {
                if (ColumnInfo.TryGetValue("CreatedAt", out value)) value.MatrixColumn.Visible = true;
                if (ColumnInfo.TryGetValue("UpdatedAt", out value)) value.MatrixColumn.Visible = true;
            }
            if (typeof(IAuditableUser).IsAssignableFrom(typeof(T))) {
                if (ColumnInfo.TryGetValue("CreatedBy", out value)) value.MatrixColumn.Visible = true;
                if (ColumnInfo.TryGetValue("UpdatedBy", out value)) value.MatrixColumn.Visible = true;
            }
        }

        abstract protected Matrix GetMatrix();
        abstract protected Button GetSaveButton();


        public bool UnsavedChanges() => _data.Any(x => x.Status != Status.Normal);

    }

}