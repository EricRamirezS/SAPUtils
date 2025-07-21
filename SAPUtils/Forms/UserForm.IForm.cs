using System;
using SAPbouiCOM;

namespace SAPUtils.Forms {
    public abstract partial class UserForm : IForm {
        /// <summary>
        /// Sets the focus to the form.
        /// </summary>
        public void Select() {
            Logger.Trace("Selecting form with UID: {0}", UniqueID);
            UIAPIRawForm?.Select();
        }

        /// <summary>
        /// Closes an open form.
        /// </summary>
        public void Close() {
            Logger.Debug("Closing form with UID: {0}", UniqueID);
            UIAPIRawForm?.Close();
        }

        /// <summary>
        /// <p><b>Deprecated in UI API 2004.</b></p>
        /// The method is supported in the next two releases for backward compatibility.
        /// In the current release, the form is always updated so there is no need for refreshing the form.
        /// </summary>
        public void Refresh() {
            Logger.Debug("Refreshing form with UID: {0}", UniqueID);
            UIAPIRawForm?.Refresh();
        }

        /// <summary>
        /// Returns an XML string representation of the object.
        /// </summary>
        /// <returns></returns>
        public string GetAsXML() {
            Logger.Trace("Getting XML representation of form with UID: {0}", UniqueID);
            return UIAPIRawForm?.GetAsXML();
        }

        /// <summary>
        /// Reloads the form from the application. 
        /// Use this method to update an object from the database. 
        /// </summary>
        /// <exception cref="Exception">Thrown when the update operation fails.</exception>
        public void Update() {
            Logger.Debug("Updating form with UID: {0}", UniqueID);
            try {
                UIAPIRawForm?.Update();
                Logger.Info("Form updated successfully");
            }
            catch (Exception ex) {
                Logger.Error("Error updating form: {0}", ex);
                throw;
            }
        }

        /// <summary>
        /// Freezes or unfreezes the form. When frozen, no visual changes are displayed on the form.
        /// </summary>
        /// <p name="newVal">
        /// Indicates whether the form should be frozen:
        /// <list type="bullet">
        /// <item><description><c>true</c>: The form is frozen (visual updates are suspended).</description></item>
        /// <item><description><c>false</c>: The form is unfrozen (visual updates resume).</description></item>
        /// </list>
        /// </p>
        /// <remarks>
        /// Use this method to prevent flickering when performing multiple visual changes to a form,
        /// such as adding or moving items, or modifying a matrix.
        /// 
        /// After completing all changes, unfreeze the form by calling <c>Freeze(false)</c>,
        /// and then call <c>Update()</c> to apply the changes.
        /// 
        /// Do not perform data updates while the form is frozen.
        /// 
        /// <p>
        /// Important: If you call <c>Freeze(true)</c> multiple times,
        /// you must call <c>Freeze(false)</c> the same number of times.
        /// Otherwise, the form will remain frozen.
        /// </p>
        /// </remarks>
        /// <example>
        /// The following code freezes a form, performs some layout changes, then unfreezes and updates it:
        /// <code>
        /// public void UpdateFormLayout(SAPbouiCOM.Form form)
        /// {
        ///     form.Freeze(true);
        ///     try
        ///     {
        ///         // Perform visual changes here
        ///         form.Items.Item("txtCode").Visible = false;
        ///         form.Items.Item("txtCode").Left += 50;
        ///     }
        ///     finally
        ///     {
        ///         form.Freeze(false);
        ///         form.Update();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        public void Freeze(bool newVal) {
            Logger.Debug("Setting form freeze state to {0} for form with UID: {1}", newVal, UniqueID);
            UIAPIRawForm?.Freeze(newVal);
        }

        /// <summary>
        /// Enables or disables a toolbar menu item when the form is in focus.
        /// </summary>
        /// <param name="menuUid">
        /// The unique ID of the menu item you want to enable or disable.
        /// </param>
        /// <param name="enableFlag">
        /// Indicates whether the menu item is enabled:
        /// <list type="bullet">
        /// <item><description><c>true</c>: Enabled</description></item>
        /// <item><description><c>false</c>: Disabled</description></item>
        /// </list>
        /// </param>
        /// <remarks>
        /// In the SAP Business One application, the active form controls the state of toolbar items.
        /// During the form lifecycle, toolbar items are automatically enabled or disabled depending
        /// on the form currently in focus.
        ///
        /// You cannot enable or disable toolbar items for built-in SAP Business One forms.
        ///
        /// To enable application navigation icons for a custom form,
        /// use the <c>it_BROWSE_BUTTON</c> item type when calling <c>Items.Add</c>.
        /// </remarks>
        /// <example>
        /// The following sample code shows how to enable standard navigation toolbar items:
        /// <code>
        /// public void EnableNavigationToolbar(SAPbouiCOM.Form form)
        /// {
        ///     form.EnableMenu("1288", true); // First record
        ///     form.EnableMenu("1289", true); // Previous record
        ///     form.EnableMenu("1290", true); // Next record
        ///     form.EnableMenu("1291", true); // Last record
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        public void EnableMenu(string menuUid, bool enableFlag) {
            Logger.Trace("Setting menu {0} enabled state to {1} for form with UID: {2}",
                menuUid, enableFlag, UniqueID);
            UIAPIRawForm?.EnableMenu(menuUid, enableFlag);
        }

        /// <summary>
        /// Clears all user-defined actions on the application toolbar by resetting the menu status.
        /// </summary>
        /// <remarks>
        /// When using <see cref="EnableMenu(string, bool)"/> to enable or disable toolbar icons,
        /// those changes are retained with the current toolbar. 
        /// 
        /// This method resets the toolbar to its default state, disabling all icons. 
        /// It is useful when changing the form mode or when loading a new form.
        /// 
        /// <para>
        /// Note: This method is only relevant for system forms. It has no effect on custom forms.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to reset toolbar actions on a system form:
        /// <code>
        /// public void ResetToolbar(SAPbouiCOM.Form form)
        /// {
        ///     form.ResetMenuStatus();
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="EnableMenu(string, bool)"/>
        public void ResetMenuStatus() {
            Logger.Debug("Resetting menu status for form with UID: {0}", UniqueID);
            UIAPIRawForm?.ResetMenuStatus();
        }

        /// <summary>
        /// Resizes the form. 
        /// Use this method to resize the form and keep the relative locations of items on the form.
        /// </summary>
        /// <param name="width">The new width of the form.</param>
        /// <param name="height">The new height of the form.</param>
        public void Resize(int width, int height) {
            Logger.Debug("Resizing form to Width: {0}, Height: {1}", width, height);
            UIAPIRawForm?.Resize(width, height);
        }

        /// <summary>
        /// Enables format search.
        /// </summary>
        public void EnableFormatSearch() {
            Logger.Debug("Enabling format search for form with UID: {0}", UniqueID);
            UIAPIRawForm?.EnableFormatSearch();
        }

        /// <summary>
        /// The form's title.
        /// </summary>
        public string Title
        {
            get => UIAPIRawForm.Title;
            set
            {
                if (Alive) UIAPIRawForm.Title = value;
            }
        }

        /// <summary>
        /// The window state of the form, such as whether the form in minimized.
        /// </summary>
        public BoFormStateEnum State
        {
            get => UIAPIRawForm.State;
            set
            {
                if (Alive) UIAPIRawForm.State = value;
            }
        }


        /// <summary>
        /// Indicates whether the form is visible.
        /// </summary>
        public bool Visible
        {
            get => UIAPIRawForm.Visible;
            set
            {
                if (Alive) UIAPIRawForm.Visible = value;
            }
        }

        /// <summary>
        /// The default item of the form.
        /// </summary>
        public string DefButton
        {
            get => UIAPIRawForm.DefButton;
            set
            {
                if (Alive) UIAPIRawForm.DefButton = value;
            }
        }

        /// <summary>
        /// Gets or sets the current mode of the form.
        /// </summary>
        /// <value>
        /// A <see cref="SAPbouiCOM.BoFormMode"/> value indicating the current mode of the form.
        /// </value>
        /// <remarks>
        /// The mode defines the type of actions users can perform on the form. 
        /// 
        /// <p>
        /// For example, <c>ADD_MODE</c> is used to add new records, while <c>OK_MODE</c> is used to display existing data.
        /// </p>
        /// 
        /// <p>
        /// You can choose to manage mode changes manually in your add-on or let the application handle them automatically using <see cref="AutoManaged"/>.
        /// </p>
        /// 
        /// <p>
        /// Mode changes are typically triggered by clicking on items whose <see cref="SAPbouiCOM.Item.AffectsFormMode"/> property is set to <c>true</c>.
        /// </p>
        /// </remarks>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="AutoManaged"/>
        /// <seealso cref="SAPbouiCOM.Item.AffectsFormMode"/>
        public BoFormMode Mode
        {
            get => UIAPIRawForm.Mode;
            set
            {
                if (Alive) UIAPIRawForm.Mode = value;
            }
        }

        /// <summary>
        /// Gets or sets the current pane level of the form.
        /// </summary>
        /// <value>
        /// A <see cref="long"/> value representing the active pane level.
        /// </value>
        /// <remarks>
        /// This property determines which items are visible on the form, based on each item's <c>FromPane</c> and <c>ToPane</c> settings.
        /// 
        /// <p>
        /// Items with <c>FromPane = 1</c> and <c>ToPane = 1</c> will be visible only when <c>PaneLevel</c> is set to 1.
        /// </p>
        /// 
        /// <p>
        /// Items with <c>FromPane = 0</c> and <c>ToPane = 0</c> are always visible, regardless of the form's current pane level.
        /// </p>
        /// </remarks>
        /// <seealso cref="UserForm"/>
        public int PaneLevel
        {
            get => UIAPIRawForm.PaneLevel;
            set
            {
                if (Alive) UIAPIRawForm.PaneLevel = value;
            }
        }

        /// <summary>
        /// The form's top position.
        /// </summary>
        public int Top
        {
            get => UIAPIRawForm.Top;
            set
            {
                if (Alive) UIAPIRawForm.Top = value;
            }
        }

        /// <summary>
        /// The form's left position.
        /// </summary>
        public int Left
        {
            get => UIAPIRawForm.Left;
            set
            {
                if (Alive) UIAPIRawForm.Left = value;
            }
        }

        /// <summary>
        /// The height of the form.
        /// </summary>
        /// <remarks>
        /// The height includes the title area.
        /// </remarks>
        public int Height
        {
            get => UIAPIRawForm.Height;
            set
            {
                if (Alive) UIAPIRawForm.Height = value;
            }
        }

        /// <summary>
        /// The width of the form.
        /// </summary>
        /// <remarks>
        /// The width  includes the title area.
        /// </remarks>
        public int Width
        {
            get => UIAPIRawForm.Width;
            set
            {
                if (Alive) UIAPIRawForm.Width = value;
            }
        }

        /// <summary>
        /// The width of the form's client rectangle, which is the form area that is available for adding items.
        /// This area does not include the title area of the form, on which you cannot add items.
        /// </summary>
        public int ClientHeight
        {
            get => UIAPIRawForm.ClientHeight;
            set
            {
                if (Alive) UIAPIRawForm.ClientHeight = value;
            }
        }

        /// <summary>
        /// The height of the form's client rectangle, which is the form area that is available for adding items.
        /// This area does not include the title area of the form, on which you cannot add items.
        /// </summary>
        public int ClientWidth
        {
            get => UIAPIRawForm.ClientWidth;
            set
            {
                if (Alive) UIAPIRawForm.ClientWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the range of modes available to the form.
        /// </summary>
        /// <value>
        /// A <see cref="long"/> bitmask representing one or more <see cref="SAPbouiCOM.BoAutoFormMode"/> enumeration values.
        /// </value>
        /// <remarks>
        /// <p>
        /// In SAP Business One, forms can operate in several modes that determine the actions users can perform.
        /// For example, Add mode is used to add new records, while OK mode is used to display form information.
        /// </p>
        /// 
        /// <p>
        /// You can specify a single mode, multiple modes combined with a bitwise OR, or all modes supported by the application.
        /// </p>
        /// 
        /// <p>
        /// This property is relevant only for system forms.
        /// </p>
        /// </remarks>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SAPbouiCOM.BoAutoFormMode"/>
        public int SupportedModes
        {
            get => UIAPIRawForm.SupportedModes;
            set
            {
                if (Alive) UIAPIRawForm.SupportedModes = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application automatically manages the form modes for the add-on form.
        /// </summary>
        /// <value>
        /// <c>true</c> if the application manages the form modes automatically; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When set to <c>true</c>, SAP Business One handles mode transitions (such as Add, Find, OK)
        /// and manages the status of items and menus relevant to the current mode.
        /// 
        /// <p>
        /// Use the <see cref="SupportedModes"/> property to specify which modes the application should manage.
        /// </p>
        /// 
        /// In SAP Business One, forms can operate in different modes to control what actions users can perform:
        /// for example, Add mode allows adding new records, while OK mode displays existing records.
        /// 
        /// <p>
        /// Mode transitions are typically triggered by clicking on items marked with the <i>"AffectsFormMode"</i> property.
        /// </p>
        ///
        /// <p>
        /// For system forms, you can either let the application manage form modes automatically using <c>AutoManaged = true</c>,
        /// or handle mode transitions manually in your add-on.
        /// </p>
        /// </remarks>
        /// <example>
        /// The following example enables automatic mode management for a system form:
        /// <code>
        /// public void ConfigureFormModes(SAPbouiCOM.Form form)
        /// {
        ///     form.AutoManaged = true;
        ///     form.SupportedModes = SAPbouiCOM.BoFormMode.fm_ADD_MODE | 
        ///                           SAPbouiCOM.BoFormMode.fm_FIND_MODE | 
        ///                           SAPbouiCOM.BoFormMode.fm_OK_MODE;
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SupportedModes"/>
        public bool AutoManaged
        {
            get => UIAPIRawForm.AutoManaged;
            set
            {
                if (Alive) UIAPIRawForm.AutoManaged = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique ID of the item currently in focus on the form.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the unique ID of the item currently in focus.
        /// </value>
        /// <remarks>
        /// When setting this property, the form sets the focus on the specified item,
        /// which must be capable of receiving focus (such as an <c>EditText</c> or <c>ComboBox</c>).
        ///
        /// If no item has focus, this property returns an empty string.
        ///
        /// <p>Exceptions:</p>
        /// <list type="bullet">
        /// <item><description><c>UIApiItemFormNotActive</c>: Thrown if the form is not active.</description></item>
        /// <item><description><c>Invalid item</c>: Thrown if the specified item ID does not exist on the form.</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to get and set the active item in a form,
        /// and how to check if a specific item (e.g., a ComboBox) is currently focused:
        /// <code>
        /// public void HandleItemFocus(SAPbouiCOM.Form form)
        /// {
        ///     // Get the unique ID of the currently active item
        ///     string activeItemId = form.ActiveItem;
        ///
        ///     // Set focus to a specific item by UID
        ///     form.ActiveItem = "Combo1";
        ///
        ///     // Retrieve the item and cast it to ComboBox
        ///     var item = form.Items.Item("Combo1");
        ///     var comboBox = (SAPbouiCOM.ComboBox)item.Specific;
        ///
        ///     // Check if the ComboBox has focus
        ///     bool isActive = comboBox.Active;
        ///
        ///     // Remove focus from the ComboBox
        ///     comboBox.Active = false;
        ///     isActive = comboBox.Active;
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        public string ActiveItem
        {
            get => UIAPIRawForm.ActiveItem;
            set
            {
                if (Alive) UIAPIRawForm.ActiveItem = value;
            }
        }

        /// <summary>
        /// Gets or sets the report type code associated with the form.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the report type code.
        /// </value>
        /// <remarks>
        /// Setting this property links the form with a specific report type.
        /// </remarks>
        /// <example>
        /// The following example shows how to set the <c>ReportType</c> property for a form by searching a list of report types:
        /// <code>
        /// SAPbobsCOM.ReportTypesParams reportTypes = rptTypeService.GetReportTypeList();
        /// for (int i = 0; i &lt; reportTypes.Count; i++)
        /// {
        ///     if (reportTypes.Item(i).TypeName == "Addon Demo Type 3" &&
        ///         reportTypes.Item(i).AddonName == "SimpleForm" &&
        ///         reportTypes.Item(i).MenuID == "MySubMenu01" &&
        ///         reportTypes.Item(i).AddonFormType == "MySimpleForm")
        ///     {
        ///         form.ReportType = reportTypes.Item(i).TypeCode;
        ///         break;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        public string ReportType
        {
            get => UIAPIRawForm.ReportType;
            set
            {
                if (Alive) UIAPIRawForm.ReportType = value;
            }
        }

        /// <summary>
        /// Indicates whether the form is visible.
        /// </summary>

        public bool VisibleEx
        {
            get => UIAPIRawForm.VisibleEx;
            set
            {
                if (Alive) UIAPIRawForm.VisibleEx = value;
            }
        }

        /// <summary>
        /// The maximum width of the form.
        /// </summary>
        public int MaxWidth
        {
            get => UIAPIRawForm.MaxWidth;
            set
            {
                if (Alive) UIAPIRawForm.MaxWidth = value;
            }
        }

        /// <summary>
        /// The maximum height of the form.
        /// </summary>
        public int MaxHeight
        {
            get => UIAPIRawForm.MaxHeight;
            set
            {
                if (Alive) UIAPIRawForm.MaxHeight = value;
            }
        }

        /// <summary>
        /// A collection of all the items in the form.
        /// </summary>
        public Items Items => UIAPIRawForm?.Items;

        /// <summary>
        /// The data source of the form.
        /// </summary>
        public DataSource DataSources => UIAPIRawForm?.DataSources;

        /// <summary>
        /// Gets the number of open forms of the same type.
        /// </summary>
        /// <value>
        /// A <see cref="long"/> indicating how many instances of this form type are currently open.
        /// </value>
        /// <remarks>
        /// <p>
        /// Each SAP Business One form has a unique type ID. For example, the Purchase Order form has a type ID of 142.
        /// </p>
        /// 
        /// <p>
        /// Multiple instances of the same form type can be open simultaneously. The <c>TypeCount</c> property returns the count
        /// of open forms of the same type, shared among all instances of that form type.
        /// </p>
        /// 
        /// <p>
        /// To access a specific form instance by type and instance number, use the <see cref="SAPbouiCOM.Forms.GetForm"/> method.
        /// </p>
        /// </remarks>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SAPbouiCOM.Forms.GetForm"/>
        public int TypeCount => UIAPIRawForm?.TypeCount ?? -1;

        /// <summary>
        /// <p><b>Deprecated in UI API 2004.</b></p>
        /// The property is supported for backward compatibility in the next two releases. Use TypeEX instead.
        /// </summary>
        public int Type => UIAPIRawForm?.Type ?? -1;

        /// <summary>
        /// Indicates whether the form is a modal window.
        /// </summary>
        public bool Modal => UIAPIRawForm?.Modal == false;

        /// <summary>
        /// Indicates whether the form has focus.
        /// </summary>
        public bool Selected => UIAPIRawForm?.Selected ?? false;

        /// <summary>
        /// The form's unique ID.
        /// </summary>
        public string UniqueID => UIAPIRawForm?.UniqueID;

        /// <summary>
        /// Gets the <see cref="SAPbouiCOM.Menus"/> collection associated with the form,
        /// allowing you to add dynamic menu items specific to the form.
        /// </summary>
        /// <value>
        /// A read-only <see cref="SAPbouiCOM.Menus"/> collection representing the form's menu structure.
        /// </value>
        /// <remarks>
        /// The form's menu is displayed under the application's "Goto" menu when the form is in focus.
        /// This enables your add-on to integrate with SAP Business One's native UI behavior
        /// while maintaining its look and feel.
        /// 
        /// <p>
        /// When the form is closed, its associated menu is automatically removed from the "Goto" menu,
        /// so manual cleanup is not required.
        /// </p>
        /// 
        /// <p>
        /// It is recommended to define form menus using XML, particularly when your add-on needs to support
        /// opening multiple instances of the same form type. Using XML avoids conflicts from duplicated menu UIDs.
        /// For more information, see the &lt;FormMenu&gt; element in the SAP Business One SDK documentation on Adding Forms with XML.
        /// </p>
        /// </remarks>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SAPbouiCOM.Menus"/>
        public Menus Menu => UIAPIRawForm?.Menu;

        /// <summary>
        /// The form's border style.
        /// </summary>
        public BoFormBorderStyle BorderStyle => UIAPIRawForm?.BorderStyle ?? default;

        /// <summary>
        /// Gets the type of the form as a unique string identifier.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the form's unique type ID.
        /// </value>
        /// <remarks>
        /// <p>
        /// Each SAP Business One form has a unique type ID. For example, the Purchase Order form's type is "142".
        /// </p>
        /// 
        /// <p>
        /// Multiple instances of the same form type can be open at the same time.
        /// The <see cref="TypeCount"/> property returns how many forms of this type are currently open.
        /// </p>
        /// 
        /// <p>
        /// To access a specific instance of a form type, use the <see cref="SAPbouiCOM.Forms.GetForm"/> method.
        /// </p>
        /// </remarks>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="TypeCount"/>
        /// <seealso cref="SAPbouiCOM.Forms.GetForm"/>
        public string TypeEx => UIAPIRawForm?.TypeEx;

        /// <summary>
        /// Gets a reference to the <see cref="SAPbouiCOM.BusinessObject"/> associated with the user-defined object connected to the form.
        /// </summary>
        /// <value>
        /// A <see cref="SAPbouiCOM.BusinessObject"/> instance representing the user-defined object tied to the form.
        /// </value>
        /// <remarks>
        /// This property is relevant only for user-defined forms (UDFs).
        /// The associated object type must be specified using <see cref="SAPbouiCOM.FormCreationParams.ObjectType"/>
        /// when creating the form.
        /// 
        /// <p>
        /// The returned <c>BusinessObject</c> provides access to the underlying data and behavior of the UDO (User-Defined Object)
        /// linked to the form.
        /// </p>
        /// </remarks>
        /// <example>
        /// The following example shows how to access the business object from a user-defined form:
        /// <code>
        /// public void AccessBusinessObject(SAPbouiCOM.Form form)
        /// {
        ///     var businessObject = form.BusinessObject;
        ///
        ///     // Example: Get the ObjectType of the UDO
        ///     string objectType = businessObject.Type;
        ///
        ///     // Cast to the appropriate interface if needed (e.g., GeneralService, Documents, etc.)
        ///     // var doc = (SAPbobsCOM.Documents)businessObject; // if applicable
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SAPbouiCOM.FormCreationParams.ObjectType"/>
        public BusinessObject BusinessObject => UIAPIRawForm?.BusinessObject;

        /// <summary>
        /// Gets the <see cref="SAPbouiCOM.DataBrowser"/> object associated with the form,
        /// allowing navigation between records.
        /// </summary>
        /// <value>
        /// A <see cref="SAPbouiCOM.DataBrowser"/> instance that provides functionality for navigating
        /// through data records displayed in the form.
        /// </value>
        /// <remarks>
        /// This property is relevant only for custom (non-system) forms.
        /// The <c>DataBrowser</c> enables the display and use of navigation buttons and menus,
        /// such as moving to the first, previous, next, or last record in the dataset.
        /// </remarks>
        /// <example>
        /// The following example shows how to navigate to the next record using the DataBrowser:
        /// <code>
        /// public void NavigateToNextRecord(SAPbouiCOM.Form form)
        /// {
        ///     if (form.TypeEx != "UDO_FT_MyCustomForm") return; // Ensure it's a custom form
        ///
        ///     var dataBrowser = form.DataBrowser;
        ///     if (dataBrowser.CanGoNext)
        ///     {
        ///         dataBrowser.GoNext();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SAPbouiCOM.DataBrowser"/>
        public DataBrowser DataBrowser => UIAPIRawForm?.DataBrowser;

        /// <summary>
        /// A collection of the ChooseFromList items on the form.
        /// </summary>
        public ChooseFromListCollection ChooseFromLists => UIAPIRawForm?.ChooseFromLists;

        /// <summary>
        /// Gets the <see cref="SAPbouiCOM.FormSettings"/> object associated with the form.
        /// </summary>
        /// <value>
        /// A read-only <see cref="SAPbouiCOM.FormSettings"/> instance that holds the settings for the form.
        /// </value>
        /// <remarks>
        /// This property is relevant only for user-defined forms.
        /// 
        /// <p>Accessing this property on system forms will throw an exception.</p>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to access and modify the settings for a form's grid (matrix):
        /// <code>
        /// SAPbouiCOM.Form form = Application.SBO_Application.Forms.ActiveForm;
        /// SAPbouiCOM.FormSettings formSettings = form.Settings;
        /// formSettings.MatrixUID = "MyGrid";
        /// </code>
        /// </example>
        /// <seealso cref="UserForm"/>
        /// <seealso cref="SAPbouiCOM.FormSettings"/>
        public FormSettings Settings => UIAPIRawForm?.Settings;

        /// <summary>
        /// Indicates whether the form is a system form.
        /// </summary>
        public bool IsSystem => UIAPIRawForm?.IsSystem ?? false;

        /// <summary>
        /// The user-defined field form's unique ID.
        /// </summary>
        public string UDFFormUID => UIAPIRawForm?.UDFFormUID;
    }
}