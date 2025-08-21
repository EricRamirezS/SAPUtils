using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SAPbouiCOM;
using SAPUtils.Models.UserTables;
using SAPUtils.Utils;

namespace SAPUtils.Forms {
    /// <summary>
    /// Abstract base class for creating object viewer forms for specific types of user table objects.
    /// </summary>
    /// <typeparam name="T">The type derived from <c cref="SAPUtils.Models.UserTables.UserTableObjectModel"/> that represents the user table object model.</typeparam>
    /// <remarks>
    /// Provides a set of abstract methods and customizable behavior for interacting with forms in SAP B1.
    /// </remarks>
    /// <seealso cref="SAPUtils.Forms.UserForm"/>
    /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel"/>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public abstract class ObjectViewerForm<T> : UserForm where T : UserTableObjectModel, new() {

        private const string SearchRecordMenuUid = "1281";
        private const string NewRecordMenuUid = "1282";
        private const string NextRecordMenuUid = "1288";
        private const string PreviousRecordMenuUid = "1289";
        private const string FirstRecordMenuUid = "1290";
        private const string LastRecordMenuUid = "1291";

        private readonly Button _addButton;
        private readonly ButtonCombo _addButtonCombo;
        private readonly Button _cancelButton;
        private readonly Item _helper;
        private readonly Button _okButton;
        private readonly Button _searchButton;
        private readonly Button _updateButton;

        private T _item;

        /// <summary>
        /// Represents an abstract base form for viewing and interacting with objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to be viewed, which must inherit from <see cref="UserTableObjectModel"/> and have a parameterless constructor.</typeparam>
        /// <param name="uid">The unique identifier for the form. If not specified, a default identifier will be generated.</param>
        /// <param name="item">The object of type <typeparamref name="T"/> to be displayed in the form. If null, the form is initialized in "New Mode".</param>
        /// <seealso cref="UserForm" />
        /// <seealso cref="UserTableObjectModel" />
        protected ObjectViewerForm(string uid = null, T item = null) : base(uid) {
            EnableMenu(SearchRecordMenuUid, true);
            EnableMenu(NewRecordMenuUid, true);
            EnableMenu(NextRecordMenuUid, true);
            EnableMenu(PreviousRecordMenuUid, true);
            EnableMenu(FirstRecordMenuUid, true);
            EnableMenu(LastRecordMenuUid, true);

            // ReSharper disable VirtualMemberCallInConstructor
            _addButton = GetAddButton();
            _cancelButton = GetCancelButton();
            _searchButton = GetSearchButton();
            _okButton = GetOkButton();
            _updateButton = GetUpdateButton();
            _addButtonCombo = GetAddButtonCombo();

            _addButtonCombo.ValidValues.Add("1", "Agregar y Nuevo");
            _addButtonCombo.ValidValues.Add("2", "Agregar y ver");
            _addButtonCombo.ValidValues.Add("3", "Agregar y cerrar");
            _addButtonCombo.Select("1");
            _addButtonCombo.ExpandType = BoExpandType.et_DescriptionOnly;

            _addButton.ClickBefore += SaveButtonOnClickBefore;
            _addButton.ClickAfter += AddButtonClickAfter;

            _updateButton.ClickBefore += SaveButtonOnClickBefore;
            _updateButton.ClickAfter += UpdateButtonClickAfter;

            _addButtonCombo.ClickAfter += AddButtonClickAfter;

            _okButton.ClickAfter += (_, __) => Close();

            _cancelButton.ClickAfter += (_, __) => Close();

            _searchButton.ClickAfter += SearchButtonOnClickAfter;

            Application.MenuEvent += ApplicationOnMenuEvent;
            Application.ItemEvent += ApplicationOnItemEvent;

            _item = item;

            _helper = Items.Add(SapUtils.GenerateUniqueId(), BoFormItemTypes.it_EDIT);
            _helper.Left = -1000;
            _helper.Top = -1000;
            _helper.Width = 1;
            _helper.Height = 1;

            if (_item == null) {
                NewMode();
            }
            else {
                EditMode(_item);
            }
        }

        private void ApplicationOnItemEvent(string formUid, ref ItemEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;
            if (pVal.FormUID != UniqueID) return;
            if (pVal.EventType != BoEventTypes.et_VALIDATE) return;
            if (!_okButton.Item.Visible) return;
            if (UIAPIRawForm.Mode != BoFormMode.fm_UPDATE_MODE) return;
            if (!pVal.ItemChanged) return;
            _okButton.Item.Visible = false;
            _updateButton.Item.Visible = true;
            UIAPIRawForm.DefButton = _updateButton.Item.UniqueID;
        }

        private void ApplicationOnMenuEvent(ref MenuEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;

            Form activeForm = Application.Forms.ActiveForm;
            if (activeForm.UniqueID != UniqueID) return;
            if (!pVal.BeforeAction) return;
            bubbleEvent = false;
            switch (pVal.MenuUID) {
                case SearchRecordMenuUid:
                    SearchMode();
                    break;
                case NewRecordMenuUid:
                    NewMode();
                    break;

                case NextRecordMenuUid: // Next Record
                    _item = _item ?? new T();
                    T next = _item.GetNextRecord<T>();
                    if (next != null) {
                        EditMode(next);
                    }
                    break;

                case PreviousRecordMenuUid: // Previous Record
                    _item = _item ?? new T();
                    T prev = _item.GetPreviousRecord<T>();
                    if (prev != null) {
                        EditMode(prev);
                    }
                    break;

                case FirstRecordMenuUid: // First Record
                    _item = _item ?? new T();
                    T first = _item.GetFirstRecord<T>();
                    if (first != null) {
                        EditMode(first);
                    }
                    break;

                case LastRecordMenuUid: // Last Record
                    _item = _item ?? new T();
                    T last = _item.GetLastRecord<T>();
                    if (last != null) {
                        EditMode(last);
                    }
                    break;

                default:
                    bubbleEvent = true;
                    break;
            }
        }

        /// <inheritdoc />
        override protected void OnFormCloseAfter(SBOItemEventArg pVal) {
            Application.MenuEvent -= ApplicationOnMenuEvent;
            Application.ItemEvent -= ApplicationOnItemEvent;
        }

        private void SearchButtonOnClickAfter(object sboObject, SBOItemEventArg pVal) {
            List<T> results = SearchItems();
            if (results == null || results.Count == 0) return;
            if (results.Count == 1) {
                EditMode(results[0]);
            }
            else {
                ShowObjectSelector(results);
            }
        }

        private void ShowObjectSelector(List<T> data) {
            Application app = SapAddon.Instance().Application;
            FormCreationParams creationParams =
                (FormCreationParams)app.CreateObject(BoCreatableObjectType.cot_FormCreationParams);
            creationParams.FormType = "MY_CFL_FORM";
            string uid = SapUtils.GenerateUniqueId();
            creationParams.UniqueID = uid;
            creationParams.BorderStyle = BoFormBorderStyle.fbs_Sizable;
            creationParams.Modality = BoFormModality.fm_Modal;

            Form form = app.Forms.AddEx(creationParams);
            form.Title = "Seleccionar Objeto";
            form.Width = 400;
            form.Height = 300;

            DataTable dt = form.DataSources.DataTables.Add("DT_OBJECTS");
            dt.Columns.Add("Code", BoFieldsType.ft_AlphaNumeric, 50);
            dt.Columns.Add("Name", BoFieldsType.ft_AlphaNumeric, 200);

            foreach (T item in data) {
                dt.Rows.Add();
                int row = dt.Rows.Count - 1;
                dt.SetValue("Code", row, item.Code);
                dt.SetValue("Name", row, item.DisplayName);
            }

            Item matrixItem = form.Items.Add("mat", BoFormItemTypes.it_MATRIX);
            matrixItem.Left = 10;
            matrixItem.Top = 10;
            matrixItem.Width = 360;
            matrixItem.Height = 200;

            Matrix matrix = (Matrix)matrixItem.Specific;
            matrix.Layout = BoMatrixLayoutType.mlt_Normal;

            Column colCode = matrix.Columns.Add("colCode", BoFormItemTypes.it_EDIT);
            colCode.TitleObject.Caption = "Código";
            colCode.Width = 100;
            colCode.Editable = false;
            colCode.DataBind.Bind("DT_OBJECTS", "Code");

            Column colName = matrix.Columns.Add("colName", BoFormItemTypes.it_EDIT);
            colName.TitleObject.Caption = "Nombre";
            colName.Width = 240;
            colName.Editable = false;
            colName.DataBind.Bind("DT_OBJECTS", "Name");

            matrix.LoadFromDataSource();

            Item btnSelectItem = form.Items.Add("btnSel", BoFormItemTypes.it_BUTTON);
            btnSelectItem.Left = 10;
            btnSelectItem.Top = 220;
            btnSelectItem.Width = 100;
            ((Button)btnSelectItem.Specific).Caption = "Seleccionar";

            Item btnCancelItem = form.Items.Add("btnCancel", BoFormItemTypes.it_BUTTON);
            btnCancelItem.Left = 120;
            btnCancelItem.Top = 220;
            btnCancelItem.Width = 100;
            ((Button)btnCancelItem.Specific).Caption = "Cancelar";

            ((Button)btnSelectItem.Specific).ClickAfter += (s, e) =>
            {
                try {
                    int selectedRow = matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                    if (selectedRow > 0) {
                        string code = ((EditText)matrix.Columns.Item("colCode").Cells.Item(selectedRow).Specific).Value;

                        form.Close();
                        EditMode(data.FirstOrDefault(item => item.Code == code));
                    }
                    else {
                        app.MessageBox("Debes seleccionar una fila.");
                    }
                }
                catch (Exception ex) {
                    app.MessageBox("Error: " + ex.Message);
                }
            };

            ((Button)btnCancelItem.Specific).ClickAfter += (s, e) => form.Close();

            form.Visible = true;
        }

        private void UpdateButtonClickAfter(object sboObject, SBOItemEventArg pVal) {
            bool validationSuccess = ValidateForm();
            if (!validationSuccess) return;
            bool success = SaveUpdate(_item);
            if (!success) return;
            EditMode(_item);
        }

        private void AddButtonClickAfter(object sboObject, SBOItemEventArg pVal) {
            bool validationSuccess = ValidateForm();
            if (!validationSuccess) return;
            int option = ShowMessageBox("Este documento no puede modificarse tras la creación. ¿Continuar?", 1, "Sí", "No");
            if (option != 1) return;
            _item = new T();
            bool success = SaveNew(_item);
            if (!success) return;

            string value = _addButtonCombo.Selected.Value;
            switch (value) {
                case "1":
                    NewMode();
                    break;
                case "2":
                    EditMode(_item);
                    break;
                case "3":
                    Close();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void EditMode(T item) {
            Freeze(true);
            _item = item;
            _helper.Click();
            if (IsEditable(item)) {
                _addButtonCombo.Item.Visible = false;
                _cancelButton.Item.Visible = true;
                _searchButton.Item.Visible = false;
                _okButton.Item.Visible = true;
                _updateButton.Item.Visible = false;
                UIAPIRawForm.DefButton = _okButton.Item.UniqueID;
                UIAPIRawForm.Mode = BoFormMode.fm_UPDATE_MODE;
                ChangeFormMode(BoFormMode.fm_UPDATE_MODE);
                _cancelButton.Item.Enabled = true;
                _okButton.Item.Enabled = true;
                OnEditMode();
            }
            else {
                _addButtonCombo.Item.Visible = false;
                _cancelButton.Item.Visible = true;
                _searchButton.Item.Visible = false;
                _okButton.Item.Visible = true;
                _updateButton.Item.Visible = false;
                UIAPIRawForm.DefButton = _okButton.Item.UniqueID;
                UIAPIRawForm.Mode = BoFormMode.fm_VIEW_MODE;
                ChangeFormMode(BoFormMode.fm_VIEW_MODE);
                _cancelButton.Item.Enabled = true;
                _okButton.Item.Enabled = true;
                OnViewMode();
            }
            LoadFoundItem(_item);
            Freeze(false);
        }

        private void NewMode() {
            Freeze(true);
            _item = null;
            _helper.Click();
            _addButtonCombo.Item.Visible = true;
            _cancelButton.Item.Visible = true;
            _searchButton.Item.Visible = false;
            _okButton.Item.Visible = false;
            _updateButton.Item.Visible = false;
            UIAPIRawForm.DefButton = _addButton.Item.UniqueID;
            UIAPIRawForm.Mode = BoFormMode.fm_ADD_MODE;
            ChangeFormMode(BoFormMode.fm_ADD_MODE);
            _addButtonCombo.Item.Enabled = true;
            _cancelButton.Item.Enabled = true;
            OnNewMode();
            Freeze(false);
        }
        private void SearchMode() {
            Freeze(true);
            _item = null;
            _helper.Click();
            _addButtonCombo.Item.Visible = false;
            _cancelButton.Item.Visible = true;
            _searchButton.Item.Visible = true;
            _okButton.Item.Visible = false;
            _updateButton.Item.Visible = false;
            UIAPIRawForm.DefButton = _searchButton.Item.UniqueID;
            ChangeFormMode(BoFormMode.fm_FIND_MODE);
            _cancelButton.Item.Enabled = true;
            _searchButton.Item.Enabled = true;
            OnFindMode();
            Freeze(false);
        }


        /// <summary>
        /// Changes the mode of the form to the specified mode.
        /// </summary>
        /// <param name="mode">The mode to set for the form, represented by <see cref="SAPbouiCOM.BoFormMode"/>.</param>
        /// <seealso cref="SAPbouiCOM.BoFormMode"/>
        virtual protected void ChangeFormMode(BoFormMode mode) {
            UIAPIRawForm.Mode = mode;
            Refresh();
        }

        private void SaveButtonOnClickBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent) {
            bubbleEvent = ValidateForm();
        }

        /// <summary>
        /// Validates the current form to ensure its data meets the required criteria.
        /// </summary>
        /// <returns>
        /// A boolean value indicating whether the form validation was successful.
        /// <c>true</c> if the form is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method should be implemented in inheriting classes to define
        /// specific validation logic for each form.
        /// </remarks>
        /// <seealso cref="SAPUtils.Forms.UserForm" />
        /// <seealso cref="UserTableObjectModel" />
        abstract protected bool ValidateForm();
        /// <summary>
        /// Saves a new instance of the specified item, performing any necessary logic prior to persistence.
        /// </summary>
        /// <param name="item">The instance of <see cref="T"/> to be saved as a new record.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the save operation was successful.
        /// </returns>
        /// <seealso cref="UserTableObjectModel"/>
        abstract protected bool SaveNew(T item);
        /// <summary>
        /// Updates an existing item in the system.
        /// </summary>
        /// <param name="item">The item of type <see cref="T"/> to be updated.</param>
        /// <returns>
        /// A boolean value indicating whether the update operation was successful.
        /// </returns>
        /// <seealso cref="UserTableObjectModel"/>
        abstract protected bool SaveUpdate(T item);
        /// <summary>
        /// Invoked when the form enters a new mode state (Add Mode).
        /// Typically used to configure necessary fields, visibility states, or initialize information
        /// specific to creating a new record in the form.
        /// </summary>
        abstract protected void OnNewMode();
        /// <summary>
        /// This method is invoked during the edit mode transition for the form.
        /// It allows customization of the behavior or appearance of the form when it enters edit mode.
        /// </summary>
        abstract protected void OnEditMode();

        /// <summary>
        /// Triggered when the form enters view mode. This method provides an extension point for defining custom behavior
        /// that should occur specifically when the form is switched to view mode.
        /// </summary>
        /// <remarks>
        /// View mode is typically used for reviewing or displaying information with edits disabled.
        /// This method must be implemented by derived classes to handle any additional functionality required.
        /// </remarks>
        abstract protected void OnViewMode();
        /// <summary>
        /// Invoked during the find mode activation in the form.
        /// Allows customizing the behavior for enabling/disabling UI components,
        /// clearing fields, and preparing the form for search operations.
        /// </summary>
        /// <remarks>
        /// This method should be overridden in derived classes to provide specific
        /// implementation for handling the find mode activation.
        /// </remarks>
        abstract protected void OnFindMode();
        /// <summary>
        /// Loads the information of the specified item into the form. This method is used to populate
        /// the form fields based on the provided item's data.
        /// </summary>
        /// <param name="item">
        /// The item of type <see cref="T"/> to load into the form. This object contains the information
        /// that will be used to update the form fields.
        /// </param>
        /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel" />
        abstract protected void LoadFoundItem(T item);
        /// <summary>
        /// Searches for items based on the current form's field values.
        /// </summary>
        /// <returns>A list of items of type <c>T</c> that match the search criteria.</returns>
        /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel" />
        abstract protected List<T> SearchItems();
        /// <summary>
        /// Retrieves the ButtonCombo control used for the "Add" actions in the form.
        /// </summary>
        /// <returns>
        /// A <see cref="ButtonCombo"/> instance representing the combo box for "Add" actions.
        /// </returns>
        /// <seealso cref="ButtonCombo"/>
        abstract protected ButtonCombo GetAddButtonCombo();
        /// <summary>
        /// Retrieves the button used for the "Add" operation on the form.
        /// </summary>
        /// <returns>A <see cref="Button"/> object representing the "Add" button on the form.</returns>
        /// <seealso cref="Button" />
        abstract protected Button GetAddButton();
        /// <summary>
        /// Retrieves the cancel button associated with the form.
        /// </summary>
        /// <returns>A <see cref="Button"/> object representing the cancel button.</returns>
        /// <seealso cref="Button" />
        abstract protected Button GetCancelButton();
        /// <summary>
        /// Retrieves the search button for the form.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="SAPbouiCOM.Button"/> representing the search button.
        /// </returns>
        /// <seealso cref="Button" />
        abstract protected Button GetSearchButton();
        /// <summary>
        /// Retrieves the "OK" button instance within the form.
        /// </summary>
        /// <returns>A <see cref="SAPbouiCOM.Button"/> object representing the "OK" button.</returns>
        /// <seealso cref="Button" />
        abstract protected Button GetOkButton();
        /// <summary>
        /// Retrieves the update button component of the form.
        /// </summary>
        /// <returns>The <see cref="Button"/> instance representing the update button of the form.</returns>
        /// <remarks>
        /// Derived classes must implement this method to provide the specific button component.
        /// </remarks>
        /// <seealso cref="Button" />
        abstract protected Button GetUpdateButton();

        abstract protected bool IsEditable(T item);
    }
}