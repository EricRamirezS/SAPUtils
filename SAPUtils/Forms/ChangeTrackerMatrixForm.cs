using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Enums;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SAPUtils.Forms {
    /// <summary>
    /// Provides a base class for a matrix form in SAP with change tracking functionality.
    /// This class is designed for managing user table object models within a matrix structure.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the user table object model, which must implement the <see cref="IUserTableObjectModel"/> interface and have a parameterless constructor.
    /// </typeparam>
    /// <remarks>
    /// This class simplifies the management of changes in the SAP matrix elements, including support for observing changes in data,
    /// soft deletion, auditing user and date-based fields, and handling add/delete context buttons.
    /// </remarks>
    /// <seealso cref="IUserTableObjectModel"/>
    /// <seealso cref="Status"/>
    /// <seealso cref="UserForm"/>
    public abstract partial class ChangeTrackerMatrixForm<T> : UserForm where T : UserTableObjectModel, new() {


        private readonly string _addRowMenuUid;

        /// <summary>
        /// Represents a collection of data items and their associated status within the <see cref="ChangeTrackerMatrixForm{T}"/>.
        /// The items in the collection are tracked for changes such as creation, modification, or deletion.
        /// </summary>
        /// <remarks>
        /// Each item in the collection is accompanied by a <see cref="Status"/> enumeration value indicating its state,
        /// such as "Normal", "Modified", "New", or "Deleted".
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the items in the collection, constrained to objects implementing <see cref="IUserTableObjectModel"/>.
        /// </typeparam>
        /// <seealso cref="IUserTableObjectModel"/>
        /// <seealso cref="Status"/>
        private readonly List<(T Item, Status Status)> _data = new List<(T Item, Status Status)>();

        private readonly string _deleteRowMenuUid;

        private readonly List<(T Item, Status Status)> _failedData = new List<(T Item, Status Status)>();

        /// <summary>
        /// Represents a collection of observable data of type <typeparamref name="T"/> used to track changes in the form.
        /// </summary>
        /// <remarks>
        /// The collection notifies any changes through the `CollectionChanged` event. It is primarily used in scenarios
        /// where the form must handle dynamic updates to its underlying data.
        /// </remarks>
        /// <typeparam name="T">The type of objects stored in the collection, constrained to implement <see cref="IUserTableObjectModel"/>.</typeparam>
        /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{T}"/>
        /// <seealso cref="IUserTableObjectModel"/>
        private readonly ObservableCollection<T> _observableData = new ObservableCollection<T>();

        /// <summary>
        /// Represents the <see cref="UserTableAttribute"/> associated with the generic type parameter <typeparamref name="T"/>
        /// in the <see cref="ChangeTrackerMatrixForm{T}"/> class. This attribute provides metadata about the underlying SAP user table
        /// and is retrieved during the instantiation of the form.
        /// </summary>
        /// <remarks>
        /// This attribute is used to access metadata such as primary key strategies or other configurations
        /// tied to the SAP user table associated with the form's generic type.
        /// </remarks>
        /// <seealso cref="UserTableAttribute"/>
        private readonly IUserTable _tableAttribute;

        /// <summary>
        /// Determines whether an "Add Context" button should be displayed in the context menu of the form's matrix.
        /// </summary>
        /// <remarks>
        /// This variable controls the visibility of a menu item allowing users to add a new row in the matrix
        /// through the context menu. The value is configured during the form's initialization through the constructor.
        /// </remarks>
        /// <seealso cref="SAPUtils.Forms.ChangeTrackerMatrixForm{T}"/>
        /// <seealso cref="SAPUtils.Forms.UserForm"/>
        private readonly bool _useAddContextButton;

        /// <summary>
        /// Indicates whether the "Delete Row" context menu option should be added for the form's matrix.
        /// </summary>
        /// <remarks>
        /// This variable is used in the <c>AddContextMenuItems</c> method to determine whether
        /// a "Delete Row" context menu option should be available. The value is set during the
        /// construction of the <see cref="ChangeTrackerMatrixForm{T}"/>.
        /// </remarks>
        /// <seealso cref="ChangeTrackerMatrixForm{T}"/>
        private readonly bool _userDeleteContextButton;

        /// <summary>
        /// Represents a mapping between the column information of a data table and its corresponding matrix column.
        /// </summary>
        /// <remarks>
        /// This dictionary is used to establish a connection between data table columns and matrix columns in SAP forms.
        /// The key is a string identifier for the column, and the value is a tuple containing the corresponding
        /// <see cref="DataColumn"/> from the data table and the <see cref="Column"/> object representing
        /// the matrix column in the UI.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.Column"/>
        /// <seealso cref="SAPbouiCOM.DataColumn"/>
        protected readonly Dictionary<string, (DataColumn DataTableColumn, Column MatrixColumn)> ColumnInfo =
            new Dictionary<string, (DataColumn DataTableColumn, Column MatrixColumn)>();

        /// <summary>
        /// Indicates whether the data reload operation is currently in progress.
        /// </summary>
        /// <remarks>
        /// This variable is used to flag operations where the data is being reloaded
        /// to avoid unintended behavior or unnecessary processing, particularly
        /// in event handlers that listen for data changes, such as <c>DataChanged</c>.
        /// </remarks>
        /// <seealso cref="UserTableObjectModel"/>
        /// <seealso cref="ChangeTrackerMatrixForm{T}" />
        private bool _dataReload;

        /// <summary>
        /// Represents the data table connected to the SAP B1 Matrix for data manipulation and display.
        /// The table serves as the primary data source for the Matrix in the <c cref="ChangeTrackerMatrixForm{T}"/> class.
        /// </summary>
        /// <remarks>
        /// This table is populated with the custom data model objects of type <c cref="IUserTableObjectModel"/>
        /// and is synchronized with the Matrix via data binding. It is used during runtime for operations like adding,
        /// updating, or deleting rows within the Matrix while maintaining consistency across the UI and backend storage.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.DataTable"/>
        /// <seealso cref="SAPUtils.Models.UserTables.IUserTableObjectModel"/>
        private DataTable _dataTable;

        private EditText _helper;

        /// <summary>
        /// Stores the <see cref="Matrix"/> that is utilized in the form to display and manage data.
        /// This variable is initialized during the component initialization phase using the
        /// <see cref="GetMatrix"/> method and is bound to a data table that manages the underlying data.
        /// </summary>
        /// <remarks>
        /// The matrix is dynamically configured based on user table metadata and is used to provide
        /// an interactive UI for data manipulation.
        /// </remarks>
        /// <seealso cref="Matrix"/>
        /// <seealso cref="ChangeTrackerMatrixForm{T}.GetMatrix"/>
        private Matrix _matrix;

        /// <summary>
        /// Represents a save button control within the form. This button
        /// is used to trigger save actions or operations. The control
        /// is initialized during the component setup phase of the form.
        /// </summary>
        /// <remarks>
        /// The save button is a key component in capturing and saving
        /// changes made by the user within the form's matrix data.
        /// </remarks>
        /// <seealso cref="Button"/>
        private Button _saveButton;

        /// <summary>
        /// Represents an abstract base class for a matrix-style form designed to track changes with user table objects.
        /// </summary>
        /// <typeparam name="T">
        /// The type parameter representing the user table object model. Must implement <see cref="IUserTableObjectModel"/> and have a parameterless constructor.
        /// </typeparam>
        /// <remarks>
        /// This form provides functionality for managing and visualizing data stored in user-defined tables within SAP Business One.
        /// It supports optional addition and deletion context menu buttons as well as functionality for visibility of columns based on implemented interfaces like
        /// <see cref="ISoftDeletable"/>, <see cref="IAuditableDate"/>, and <see cref="IAuditableUser"/>.
        /// </remarks>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="IUserTableObjectModel"/>
        /// <seealso cref="ISoftDeletable"/>
        /// <seealso cref="IAuditableDate"/>
        /// <seealso cref="IAuditableUser"/>
        protected ChangeTrackerMatrixForm(
            bool useAddContextButton = true,
            bool userDeleteContextButton = true,
            string uid = null) : base(uid) {
            if (!Alive) return;
            _useAddContextButton = useAddContextButton;
            _userDeleteContextButton = userDeleteContextButton;
            _tableAttribute = UserTableMetadataCache.GetUserTableAttribute(typeof(T));

            _addRowMenuUid = $"{typeof(T).Name}{UniqueID}AddRow)";
            _deleteRowMenuUid = $"{typeof(T).Name}{UniqueID}DelRow)";
            CustomInitializeComponent();

            EventSubscriber();

            LoadData();

            UpdateMatrix();

            EnableMenu("1281", false); // find button
            EnableMenu("1282", _useAddContextButton); // add button
            EnableMenu("1304", true); //Enable Refresh

            (DataColumn DataTableColumn, Column MatrixColumn) value;
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T))) {
                if (ColumnInfo.TryGetValue("Active", out value)) value.MatrixColumn.Visible = false;
            }
            if (typeof(IAuditableDate).IsAssignableFrom(typeof(T))) {
                if (ColumnInfo.TryGetValue("CreatedAtDate", out value)) value.MatrixColumn.Visible = false;
                if (ColumnInfo.TryGetValue("UpdatedAtDate", out value)) value.MatrixColumn.Visible = false;
                if (ColumnInfo.TryGetValue("CreatedAtTime", out value)) value.MatrixColumn.Visible = false;
                if (ColumnInfo.TryGetValue("UpdatedAtTime", out value)) value.MatrixColumn.Visible = false;
            }
            // ReSharper disable once InvertIf, Kept for Readability
            if (typeof(IAuditableUser).IsAssignableFrom(typeof(T))) {
                if (ColumnInfo.TryGetValue("CreatedBy", out value)) value.MatrixColumn.Visible = false;
                if (ColumnInfo.TryGetValue("UpdatedBy", out value)) value.MatrixColumn.Visible = false;
            }
        }

        /// <summary>
        /// Provides a collection representing the data bound to the matrix in the form.
        /// This collection is observable and is updated based on data manipulations in the form.
        /// </summary>
        /// <remarks>
        /// The <c>Data</c> property is an observable collection of items of type <typeparamref name="T"/>,
        /// where <typeparamref name="T"/> must implement <see cref="IUserTableObjectModel"/>.
        /// Changes to this collection synchronize with the matrix display and vice versa.
        /// </remarks>
        /// <typeparam name="T">The type of the data items, which must implement <see cref="IUserTableObjectModel"/>.</typeparam>
        protected ICollection<T> Data => _observableData;

        /// <summary>
        /// Retrieves the matrix associated with the form. The matrix is used to display
        /// and interact with tabular data within the form context.
        /// </summary>
        /// <returns>The matrix object used in the form.</returns>
        /// <seealso cref="SAPbouiCOM.Matrix"/>
        abstract protected Matrix GetMatrix();
        /// <summary>
        /// Retrieves the save button from the form.
        /// </summary>
        /// <returns>The <see cref="SAPbouiCOM.Button"/> instance representing the save button in the form.</returns>
        /// <seealso cref="SAPbouiCOM.Button"/>
        abstract protected Button GetSaveButton();

        /// <summary>
        /// Determines if there are unsaved changes in the current data collection.
        /// </summary>
        /// <returns>
        /// A <see cref="bool"/> value indicating whether any items within the data collection have a status other than <see cref="Status.Normal"/>.
        /// </returns>
        /// <seealso cref="Status"/>
        public bool UnsavedChanges() => _data.Any(x => x.Status != Status.Normal);
    }

}