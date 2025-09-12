using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using SAPUtils.I18N;
using SAPUtils.Utils;
using Application = SAPbouiCOM.Application;
using Company = SAPbobsCOM.Company;
using ProgressBar = SAPbouiCOM.ProgressBar;

namespace SAPUtils.Forms {
    /// <summary>
    /// Base class for SAP Business One user forms that provides common functionality and integration with the SAP UI API.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract partial class UserForm : FormBase {
        private static ProgressBar _progressBar;

        /// <summary>
        /// Base class for SAP Business One user forms that provides common functionality
        /// and integration with the SAP UI API.
        /// </summary>
        /// <remarks>
        /// This class serves as a foundation for custom user forms within the SAP Business One
        /// environment. It simplifies common tasks such as form initialization, logging, and integration
        /// with the SAP system.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon"/>
        /// <seealso cref="SAPUtils.Utils.ILogger"/>
        protected UserForm() {
            Logger.Debug(Texts.UserForm_UserForm_Starting_the_construction_of_the_UserForm);
            try {
                ShowWaitCursor();
                LoadForm();
                if (!Alive) return;
                Logger.Critical(UniqueID);
                // ReSharper disable once VirtualMemberCallInConstructor
                OnInitializeComponent();
                InitializedSetter = true;
                Logger.Info(Texts.UserForm_UserForm_UserForm_successfully_initialized);
            }
            catch (Exception ex) {
                Logger.Error(Texts.UserForm_UserForm_Error_during_UserForm_initialization_, ex);
                throw;
            }
            finally {
                ShowArrowCursor();
            }
        }

        /// <summary>
        /// Gets the current SAP Business One Company instance.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        protected Company Company => SapAddon.Instance().Company;

        /// <summary>
        /// Gets the current SAP Business One Application instance.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        protected Application Application => SapAddon.Instance().Application;

        /// <summary>
        /// Logger instance for recording form-related events and errors.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        protected ILogger Logger => SapAddon.Instance().Logger;

        /// <summary>
        /// Loads an XML-based form definition into the SAP Business One application environment.
        /// </summary>
        /// <exception cref="InvalidDataException">Thrown if the form XML is invalid or does not contain a valid form node.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during form loading.</exception>
        /// <seealso cref="SAPbouiCOM.Framework.Application" />
        private void LoadForm() {
            Logger.Debug(Texts.UserForm_LoadForm_Starting_form_loading);
            try {
                string formPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FormResource);
                Logger.Trace(Texts.UserForm_LoadForm_Loading_form_file_from___0_, formPath);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(formPath);

                XmlNode formNode = xmlDoc.SelectSingleNode("//form");
                if (formNode == null) {
                    Logger.Error(Texts.UserForm_LoadForm_Form_node_not_found_in_XML_file);
                    throw new InvalidDataException(Texts.UserForm_LoadForm_Form_is_not_valid);
                }

                if (formNode.Attributes == null) {
                    Logger.Warning(Texts.UserForm_LoadForm_Form_node_has_no_attributes);
                    return;
                }

                formNode.Attributes["FormType"].Value = FormType;
                Logger.Trace(Texts.UserForm_LoadForm_Form_type_set_to___0_, FormType);

                XmlAttribute attrb = formNode.Attributes?["uid"];
                if (attrb != null) {
                    string uidValue = formNode.Attributes["uid"].Value;

                    if (!string.IsNullOrEmpty(uidValue) && FormUtils.ExistForm(uidValue, Application)) {
                        Logger.Trace(Texts.UserForm_LoadForm_Closing_existing_form_with_UID___0_, uidValue);
                        Application.Forms.Item(uidValue).Close();
                        if (FormUtils.ExistForm(uidValue, Application)) {
                            Logger.Info(Texts.UserForm_LoadForm_Could_not_close_form__0_, uidValue);
                            return;
                        }
                    }
                }

                Logger.Trace(Texts.UserForm_LoadForm_Loading_batch_actions_into_application);
                Application.LoadBatchActions(xmlDoc.InnerXml);
                Logger.Info(Texts.UserForm_LoadForm_Form_loaded_successfully);
            }
            catch (Exception ex) {
                Logger.Error(Texts.UserForm_LoadForm_Error_during_form_loading__, ex);
                throw;
            }
        }

        /// <summary>
        /// Shows the form if it is alive (initialized and valid).
        /// If the form is not alive, the operation is logged and ignored.
        /// </summary>
        public void Show() {
            Logger.Trace(Texts.UserForm_Show_Attempting_to_show_form);
            if (!Alive) {
                Logger.Warning(Texts.UserForm_Show_Cannot_show_form_because_it_is_not_alive);
                return;
            }

            UIAPIRawForm.Visible = true;
            Logger.Trace(Texts.UserForm_Show_Form_displayed_successfully);
        }

        /// <summary>
        /// Displays a custom message box using the SAP Business One UI API with configurable buttons and default option.
        /// </summary>
        /// <param name="text">The message text to display in the message box.</param>
        /// <param name="defaultButton">The button to be set as default (e.g., 1 for the first button, 2 for the second, etc.).</param>
        /// <param name="btn1Caption">Caption for the first button. Defaults to "OK".</param>
        /// <param name="btn2Caption">Optional caption for the second button. If empty, the button is hidden.</param>
        /// <param name="btn3Caption">Optional caption for the third button. If empty, the button is hidden.</param>
        /// <returns>
        /// The value of the pressed button (e.g., 1 for the first button, 2 for the second, etc.).
        /// </returns>
        /// <seealso cref="SAPbouiCOM.Application"/>
        protected int ShowMessageBox(string text, int defaultButton = 1, string btn1Caption = "OK",
            string btn2Caption = "", string btn3Caption = "") {
            return Application.MessageBox(text, defaultButton, btn1Caption, btn2Caption, btn3Caption);
        }

        /// <summary>
        /// Displays a status bar message in the SAP Business One application.
        /// </summary>
        /// <param name="text">The message text to display in the status bar.</param>
        /// <param name="seconds">The duration (in enumeration format) for which the message will appear on the screen.</param>
        /// <param name="type">The type of status message, such as error, warning, or success.</param>
        /// <seealso cref="SAPbouiCOM.Application"/>
        protected void SetStatusBarMessage(string text, BoMessageTime seconds = BoMessageTime.bmt_Medium,
            BoStatusBarMessageType type = BoStatusBarMessageType.smt_Error) {
            Application.StatusBar.SetText(text, seconds, type);
        }

        /// <summary>
        /// Displays an informational message in the SAP Business One status bar.
        /// </summary>
        /// <param name="text">The message text to display in the status bar.</param>
        /// <param name="seconds">The duration (in enumeration format) for which the message will appear on the screen.</param>
        protected void SetStatusBarInformationMessage(string text, BoMessageTime seconds = BoMessageTime.bmt_Medium) {
            SetStatusBarMessage(text, seconds, type: BoStatusBarMessageType.smt_None);
        }

        /// <summary>
        /// Displays an error message in the SAP Business One status bar.
        /// </summary>
        /// <param name="text">The message text to display in the status bar.</param>
        /// <param name="seconds">The duration (in enumeration format) for which the message will appear on the screen.</param>
        protected void SetStatusBarErrorMessage(string text, BoMessageTime seconds = BoMessageTime.bmt_Medium) {
            SetStatusBarMessage(text, seconds, BoStatusBarMessageType.smt_Error);
        }

        /// <summary>
        /// Displays a success/confirmation message in the SAP Business One status bar.
        /// </summary>
        /// <param name="text">The message text to display in the status bar.</param>
        /// <param name="seconds">The duration (in enumeration format) for which the message will appear on the screen.</param>
        protected void SetStatusBarSuccessMessage(string text, BoMessageTime seconds = BoMessageTime.bmt_Medium) {
            SetStatusBarMessage(text, seconds, BoStatusBarMessageType.smt_Success);
        }

        /// <summary>
        /// Displays a warning message in the SAP Business One status bar.
        /// </summary>
        /// <param name="text">The message text to display in the status bar.</param>
        /// <param name="seconds">The duration (in enumeration format) for which the message will appear on the screen.</param>
        protected void SetStatusBarWarningMessage(string text, BoMessageTime seconds = BoMessageTime.bmt_Medium) {
            SetStatusBarMessage(text, seconds, BoStatusBarMessageType.smt_Warning);
        }


        /// <summary>
        /// Toggles between the wait cursor and the default arrow cursor depending on the operation state.
        /// </summary>
        /// <param name="loading">
        /// <c>true</c> to display the wait cursor (start showing progress);
        /// <c>false</c> to restore the default arrow cursor (stop showing progress).
        /// </param>
        /// <remarks>
        /// This method calls <see cref="ShowWaitCursor"/> when a long-running operation starts
        /// and <see cref="ShowArrowCursor"/> when the operation ends.
        /// Use it to wrap code that might take time to execute.
        /// </remarks>
        /// <seealso cref="ShowWaitCursor"/>
        /// <seealso cref="ShowArrowCursor"/>
        protected void Loading(bool loading) {
            if (loading) ShowWaitCursor();
            else ShowArrowCursor();
        }
        /// <summary>
        /// Displays the wait cursor while a long-running operation is in progress.
        /// </summary>
        /// <remarks>
        /// This method checks if a progress bar already exists. If it does, the method immediately returns.
        /// Otherwise, it attempts to create a progress bar on the application's status bar to indicate
        /// that an operation is loading. Exceptions during the creation of the progress bar are suppressed.
        /// </remarks>
        /// <seealso cref="ShowArrowCursor"/>
        protected void ShowWaitCursor() {
            if (_progressBar != null) return;
            try {
                _progressBar = Application.StatusBar.CreateProgressBar(Texts.UserForm_ShowWaitCursor_Loading___, 100, false);
            }
            catch {
                // ignored
            }
        }

        /// <summary>
        /// Resets the cursor to the default arrow cursor and cleans up resources associated with the progress bar.
        /// </summary>
        /// <remarks>
        /// Use this method after ShowWaitCursor() to restore the user interface to its normal state.
        /// This method ensures that the progress bar is completed and properly released.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon"/>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        protected void ShowArrowCursor() {
            if (_progressBar == null) return;
            try {
                _progressBar.Value = 100;
                _progressBar.Stop();
            }
            catch {
                // ignored
            }
            finally {
                Marshal.ReleaseComObject(_progressBar);
                _progressBar = null;
            }
        }
    }
}