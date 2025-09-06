using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.__Internal.Events;
using SAPUtils.__Internal.I18N;
using SAPUtils.__Internal.Utils;
using SAPUtils.I18N;
using SAPUtils.Utils;
using Application = SAPbouiCOM.Framework.Application;
using Company = SAPbobsCOM.Company;
using Form = System.Windows.Forms.Form;

// ReSharper disable MemberCanBePrivate.Global

namespace SAPUtils {
    /// <summary>
    /// Represents the core functionality for a SAP Add-On integration.
    /// </summary>
    /// <remarks>
    /// The <c>SapAddon</c> class serves as the primary entry point for
    /// handling interactions with SAP Business One, including managing
    /// sessions, application lifecycle, and logging.
    /// </remarks>
    public partial class SapAddon {

        /// <summary>
        /// Represents the connection string used to establish a connection
        /// with the SAP Business One application through the SboGuiApi.
        /// </summary>
        /// <remarks>
        /// This connection string is a constant value, utilized internally
        /// by the SAP add-on to initialize and communicate with the
        /// SAP Business One environment.
        /// </remarks>
        private const string SapConnectionString =
            "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";

        /// <summary>
        /// Represents the internal static instance of the SAPbouiCOM.Application
        /// that provides access to the SAP Business One UI API.
        /// </summary>
        /// <remarks>
        /// This variable is initialized during the instantiation of the SapAddon class,
        /// and its lifecycle is tied to the SAP Addon's execution.
        /// It is intended for internal use only within the application.
        /// </remarks>
        // ReSharper disable once InconsistentNaming
        internal static SAPbouiCOM.Application __application;

        /// <summary>
        /// Represents a static instance of the SapAddon class, ensuring only a single instance
        /// of the SapAddon can exist during the application lifecycle. This is used to manage
        /// and maintain the singleton pattern throughout the application.
        /// </summary>
        private static SapAddon _instance;


        /// <summary>
        /// Represents the main class for an SAP Addon, providing core functionalities such as
        /// application initialization, company connection management, logging, and addon lifecycle control.
        /// </summary>
        private SapAddon(string[] args) {
            try {
                // ReSharper disable LocalizableElement
                ConsoleLogger.Info("SAPbouiCOM.Framework.Application");
                MainApplication = args.Length > 0 ? new Application(args[0]) : new Application();
                ConsoleLogger.Debug("SAPbouiCOM.SboGuiApi");
                SboGuiApi oSboGuiApi = new SboGuiApi();
                ConsoleLogger.Info("SAPbouiCOM.SboGuiApi.Connect()");
                oSboGuiApi.Connect(SapConnectionString);
                ConsoleLogger.Info("Connect.GetApplication()");
                __application = oSboGuiApi.GetApplication();
                ConsoleLogger.Info("SAPbouiCOM.Application.Company.GetDICompany()");
                __Company = __application.Company.GetDICompany() as Company;
                ConsoleLogger.Debug("Utils.Logger.Instance");
                // ReSharper restore LocalizableElement
                Texts.Culture = L10N.GetCulture(__Company?.language ?? BoSuppLangs.ln_Null);
                Logger = __Internal.Utils.Logger.Instance;
                Logger.Info(Texts.SapAddon_SapAddon_SAP_Add_on_Initialized);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                System.Windows.Forms.Application.ThreadException += ApplicationOnThreadException;
                EventSubscriber.Subscribe();
            }
            catch (Exception ex) {
                ConsoleLogger.Critical(Texts.SapAddon_SapAddon_Add_on_could_not_be_initialized, ex);
            }
        }

        /// <summary>
        /// Provides access to the SAP Business One application instance.
        /// This property represents the active SAPbouiCOM.Application object,
        /// which serves as the main interface for interacting with the SAP Business One UI API.
        /// </summary>
        /// <remarks>
        /// The Application object is used to perform various operations within the SAP Business One environment,
        /// such as creating menus, handling events, and accessing the system's UI components.
        /// </remarks>
        /// <value>
        /// The current SAPbouiCOM.Application instance associated with the add-on.
        /// </value>
        public SAPbouiCOM.Application Application => __application;

        /// <summary>
        /// Represents the SAP B1 company object used to manage database connections and execute SAP Business One-specific operations.
        /// </summary>
        /// <remarks>
        /// This property provides access to the global SAP B1 company context used throughout the application.
        /// It is an instance of the SAPbobsCOM.Company class.
        /// </remarks>
        public Company Company => __Company;

        // ReSharper disable once InconsistentNaming
        internal static Company __Company { get; private set; }

        /// <summary>
        /// Gets the main application instance for the SAP Addon.
        /// </summary>
        /// <remarks>
        /// The property provides access to the central application object
        /// which serves as the entry point for executing and interacting with
        /// the SAP Addon environment.
        /// </remarks>
        public Application MainApplication { get; }

        /// <summary>
        /// Provides an interface for logging various levels of messages and tracing objects within the SapAddon.
        /// </summary>
        /// <remarks>
        /// The Logger property can be utilized to log tracing, debugging, informational,
        /// warning, error, and critical messages, as well as object states. It is fundamental
        /// for debugging and tracking application state during runtime.
        /// </remarks>
        /// <value>
        /// An instance of an object implementing the <see cref="SAPUtils.Utils.ILogger"/> interface.
        /// </value>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets an instance of the <see cref="SapAddon"/> class. If an instance has not been created,
        /// it initializes a new one with default arguments and returns it.
        /// </summary>
        /// <returns>An instance of the <see cref="SapAddon"/>.</returns>
        public static SapAddon Instance() {
            return _instance ?? throw new InvalidOperationException(Texts.SapAddon_Instance_SapAddon_instance_not_found);
        }

        /// <summary>
        /// Retrieves the singleton instance of the SapAddon class. If the instance does not already exist, it initializes a new one with the provided arguments.
        /// </summary>
        /// <param name="args">An array of strings containing the arguments used for initialization of the SapAddon instance.</param>
        /// <returns>The singleton instance of the SapAddon class.</returns>
        public static SapAddon Instance(string[] args) {
            return _instance ?? (_instance = new SapAddon(args));
        }

        [SuppressMessage("ReSharper", "LocalizableElement")]
        private void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs e) {
            Logger.Critical("Unhandled Exception", e.Exception);
        }

        [SuppressMessage("ReSharper", "LocalizableElement")]
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Logger.Critical("Unhandled Exception", e.ExceptionObject as Exception);
        }

        /// <summary>
        /// Executes the main entry point logic for the SAP Addon application.
        /// </summary>
        /// <remarks>
        /// This method sets up certain SAP addon behaviors, including logging an informational message
        /// and updating the SAP status bar during initialization. It then invokes the main application's run method.
        /// </remarks>
        public void Run() {
            Logger.Info(Texts.SapAddon_Run_Running_SAP_Add_on);
            Application.SetStatusBarMessage(Texts.SapAddon_Run_Finishing_add_on_initialization, BoMessageTime.bmt_Medium, false);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new FrmMain(() => MainApplication.Run(), Logger));
        }
    }

    [SuppressMessage("ReSharper", "LocalizableElement")]
    internal class FrmMain : Form {
        private readonly ILogger _log;

        internal FrmMain(Action action, ILogger logger) {
            _log = logger;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
            InitializeComponent();
            action.Invoke();
        }

        internal void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            _log.Critical($"CurrentDomain_UnhandledException {((Exception)e.ExceptionObject).Message} Trace {((Exception)e.ExceptionObject).StackTrace}");

            MessageBox.Show($"CurrentDomain_UnhandledException {((Exception)e.ExceptionObject).Message} Trace {((Exception)e.ExceptionObject).StackTrace}", "Unhandled UI Exception");
        }

        internal void Application_ThreadException(object sender, ThreadExceptionEventArgs e) {
            _log.Critical($"Application_ThreadException {e.Exception.Message} Trace {e.Exception.StackTrace}");

            MessageBox.Show($"Application_ThreadException {e.Exception.Message} Trace {e.Exception.StackTrace}", "Unhandled Thread Exception");
        }

        private void InitializeComponent() {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 262);
            Name = "FrmMain";
            Text = "";
            Load += Form1_Load;
            ResumeLayout(false);
        }


        private void Form1_Load(object sender, EventArgs e) {
            ShowInTaskbar = false;
            Hide();
        }
    }
}