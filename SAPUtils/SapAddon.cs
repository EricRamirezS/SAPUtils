using System;
using SAPbouiCOM;
using SAPUtils.Internal.Utils;
using SAPUtils.Utils;
using Application = SAPbouiCOM.Framework.Application;
using Company = SAPbobsCOM.Company;

namespace SAPUtils
{
    public partial class SapAddon
    {
        public SAPbouiCOM.Application Application => _application;
        public Company Company => _company;
        public Application MainApplication => _MainApplication;
        public ILogger Logger { get; }
        internal static SAPbouiCOM.Application _application;
        internal static Company _company;
        internal static Application _MainApplication;

        public const string SapConnectionString =
            "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";

        private static SapAddon _instance;

        public static SapAddon Instance()
        {
            return Instance(new string[] { });
        }

        public static SapAddon Instance(string[] args)
        {
            return _instance ?? (_instance = new SapAddon(args));
        }


        private SapAddon(string[] args)
        {
            try {
                ConsoleLogger.Info("SAPbouiCOM.Framework.Application");
                _MainApplication = args.Length > 0 ? new Application(args[0]) : new Application();
                ConsoleLogger.Debug("Retrieving SboGuiApi");
                SboGuiApi oSboGuiApi = new SboGuiApi();
                ConsoleLogger.Info("Connecting SboGuiApi");
                oSboGuiApi.Connect(SapConnectionString);
                ConsoleLogger.Info("Retrieving SAPbouiCOM.Application");
                _application = oSboGuiApi.GetApplication();
                ConsoleLogger.Info("Retrieving SAPbobsCOM.Company");
                _company = Application.Company.GetDICompany() as Company;
                ConsoleLogger.Debug("Initilizing Logger");
                Logger = Internal.Utils.Logger.Instance;
                Logger.Info("Sap Addon started");
            }
            catch (Exception ex) {
                ConsoleLogger.Critical("Addon could not be initialized", ex);
            }
        }

        public void Run()
        {
            Logger.Info("Run SAP Addon");
            Application.SetStatusBarMessage(
                "Finalizando inicializacion add-on", SAPbouiCOM.BoMessageTime.bmt_Medium, false);
            MainApplication.Run();
        }
    }
}