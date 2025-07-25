using System;
using System.Collections.Generic;
using System.Linq;
using SAPbouiCOM;
using SAPUtils.Models.UserTables;
using SAPUtils.Utils;

namespace SAPUtils.Forms {
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
        private readonly Button _okButton;
        private readonly Button _searchButton;
        private readonly Button _updateButton;
        private Item _helper;

        private T _item;

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
                    T prev = _item.GetNextRecord<T>();
                    if (prev != null) {
                        EditMode(prev);
                    }
                    break;

                case FirstRecordMenuUid: // First Record
                    _item = _item ?? new T();
                    T first = _item.GetNextRecord<T>();
                    if (first != null) {
                        EditMode(first);
                    }
                    break;

                case LastRecordMenuUid: // Last Record
                    _item = _item ?? new T();
                    T last = _item.GetNextRecord<T>();
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
            //TODO: No hacer nada si solo se abrio el Combobox, pero nada se seleccionó, actualmente llama a la opción ya seleccionada
            bool validationSuccess = ValidateForm();
            if (!validationSuccess) return;
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
            _addButtonCombo.Item.Visible = false;
            _cancelButton.Item.Visible = true;
            _searchButton.Item.Visible = false;
            _okButton.Item.Visible = true;
            _updateButton.Item.Visible = false;
            UIAPIRawForm.DefButton = _okButton.Item.UniqueID;
            UIAPIRawForm.Mode = BoFormMode.fm_UPDATE_MODE;
            OnEditMode();
            LoadFoundItem(_item);
            ChangeFormMode(BoFormMode.fm_UPDATE_MODE);
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
            OnNewMode();
            ChangeFormMode(BoFormMode.fm_ADD_MODE);
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
            OnFindMode();
            ChangeFormMode(BoFormMode.fm_FIND_MODE);
            Freeze(false);
        }


        protected virtual void ChangeFormMode(BoFormMode mode) {
            UIAPIRawForm.Mode = mode;
            int originalPane = PaneLevel;
            PaneLevel = originalPane == 1 ? 2 : 1; // Pane arbitrario
            PaneLevel = originalPane;
        }

        private void SaveButtonOnClickBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent) {
            bubbleEvent = ValidateForm();
        }

        abstract protected bool ValidateForm();
        abstract protected bool SaveNew(T item);
        abstract protected bool SaveUpdate(T item);
        abstract protected void OnNewMode();
        abstract protected void OnEditMode();
        abstract protected void OnFindMode();
        abstract protected void LoadFoundItem(T item);
        abstract protected List<T> SearchItems();
        abstract protected ButtonCombo GetAddButtonCombo();
        abstract protected Button GetAddButton();
        abstract protected Button GetCancelButton();
        abstract protected Button GetSearchButton();
        abstract protected Button GetOkButton();
        abstract protected Button GetUpdateButton();
    }
}