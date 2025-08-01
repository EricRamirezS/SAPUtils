using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace SAPUtils.Models.Menu {
    /// <summary>
    /// Represents a generic menu item interface in the SAP Business One menu structure.
    /// </summary>
    public interface IMenuItem {
        /// <summary>
        /// Gets the unique identifier (UID) of the menu item.
        /// </summary>
        string Uid { get; }

        /// <summary>
        /// Gets the display name of the menu item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the URL of the image associated with the menu item, if any.
        /// </summary>
        string ImageUrl { get; }

        /// <summary>
        /// Gets the list of submenus contained within this menu item.
        /// </summary>
        IList<IMenuItem> SubMenus { get; }

        /// <summary>
        /// Gets the optional position index of the menu item in the menu hierarchy.
        /// </summary>
        int? ItemPosition { get; set; }

        /// <summary>
        /// Gets the type of the menu item (e.g., string, popup) as defined by SAP Business One.
        /// </summary>
        BoMenuType MenuType { get; }


        /// <summary>
        /// Gets the action to be executed when the menu item is clicked.
        /// </summary>
        Action OnClick { get; }

        /// <summary>
        /// Gets the function that determines whether the menu item is currently available (visible/enabled).
        /// </summary>
        Func<bool> Available { get; }
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class MenuItem : IMenuItem {

        private string _imageUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class representing an actionable menu item (leaf).
        /// </summary>
        /// <param name="uid">The unique identifier of the menu item.</param>
        /// <param name="name">The display name of the menu item.</param>
        /// <param name="onClick">The action to be executed when the menu item is selected.</param>
        /// <param name="imageUrl">Optional image URL for the menu item icon.</param>
        /// <param name="available">Optional condition to determine whether the menu item is available. Defaults to always available.</param>
        public MenuItem(string uid, string name, Action onClick, string imageUrl = null, Func<bool> available = null) {
            Uid = uid;
            Name = name;
            _imageUrl = imageUrl;
            OnClick = onClick;
            MenuType = BoMenuType.mt_STRING;
            Available = available ?? (() => true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class representing a menu item with submenus (popup).
        /// </summary>
        /// <param name="uid">The unique identifier of the menu item.</param>
        /// <param name="name">The display name of the menu item.</param>
        /// <param name="subMenus">A list of submenu items associated with this menu item.</param>
        /// <param name="imageUrl">Optional image URL for the menu item icon.</param>
        /// <param name="available">Optional condition to determine whether the menu item is available. Defaults to always available.</param>
        public MenuItem(string uid, string name, IList<IMenuItem> subMenus, string imageUrl = null,
            Func<bool> available = null) {
            Uid = uid;
            Name = name;
            _imageUrl = imageUrl;
            SubMenus = subMenus;
            MenuType = BoMenuType.mt_POPUP;
            Available = available ?? (() => true);
        }

        /// <inheritdoc />
        public string Uid { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string ImageUrl
        {
            get => _imageUrl;
            internal set
            {
                if (_imageUrl == null) {
                    _imageUrl = value;
                }
            }
        }

        /// <inheritdoc />
        public IList<IMenuItem> SubMenus { get; }

        /// <inheritdoc />
        public int? ItemPosition { get; set; }

        /// <inheritdoc />
        public BoMenuType MenuType { get; }

        /// <inheritdoc />
        public Action OnClick { get; }

        /// <inheritdoc />
        public Func<bool> Available { get; }
    }

    internal class SapMenuItem : MenuItem {
        internal SapMenuItem(string name, SapMenuUid sapId, IList<IMenuItem> subMenus = null)
            : base(uid: ((int)sapId).ToString(), name: name, subMenus: subMenus ?? new List<IMenuItem>()) { }
    }

    /// <summary>
    /// Provides a static collection of SAP menu items, structured hierarchically.
    /// Each menu item is represented by a <see cref="SapMenuItem"/> object,
    /// which includes its display name and a unique ID from the <see cref="SapMenuUid"/> enum.
    /// </summary>
    public static class SapMenuItems {
        /// <summary>
        /// Represents the top-level "Administration" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Administration =
            new SapMenuItem("Administration", SapMenuUid.Administration, new List<IMenuItem> {
                new SapMenuItem("System Initialization", SapMenuUid.AdministrationSystemInitialization, new List<IMenuItem> {
                    new SapMenuItem("Authorizations", SapMenuUid.AdministrationSystemInitializationAuthorizations, new List<IMenuItem> {
                        new SapMenuItem("Data Ownership", SapMenuUid.AdministrationSystemInitializationAuthorizationsDataOwnership),
                    }),
                    new SapMenuItem("Opening Balances", SapMenuUid.AdministrationSystemInitializationOpeningBalances),
                    new SapMenuItem("Implementation Center", SapMenuUid.AdministrationSystemInitializationImplementationCenter),
                }),
                new SapMenuItem("Setup", SapMenuUid.AdministrationSetup, new List<IMenuItem> {
                    new SapMenuItem("General", SapMenuUid.AdministrationSetupGeneral, new List<IMenuItem> {
                        new SapMenuItem("Security", SapMenuUid.AdministrationSetupGeneralSecurity),
                    }),
                    new SapMenuItem("Financials", SapMenuUid.AdministrationSetupFinancials, new List<IMenuItem> {
                        new SapMenuItem("G/L Account Determination", SapMenuUid.AdministrationSetupFinancialsGlAccountDetermination),
                        new SapMenuItem("Tax", SapMenuUid.AdministrationSetupFinancialsTax),
                    }),
                    new SapMenuItem("Opportunities", SapMenuUid.AdministrationSetupOpportunities),
                    new SapMenuItem("Sales", SapMenuUid.AdministrationSetupSales),
                    new SapMenuItem("Purchasing", SapMenuUid.AdministrationSetupPurchasing),
                    new SapMenuItem("Business Partners", SapMenuUid.AdministrationSetupBusinessPartners),
                    new SapMenuItem("Banking", SapMenuUid.AdministrationSetupBanking),
                    new SapMenuItem("Inventory", SapMenuUid.AdministrationSetupInventory, new List<IMenuItem> {
                        new SapMenuItem("Bin Locations", SapMenuUid.AdministrationSetupInventoryBinLocations),
                    }),
                    new SapMenuItem("Resources", SapMenuUid.AdministrationSetupResources),
                    new SapMenuItem("Service", SapMenuUid.AdministrationSetupService),
                    new SapMenuItem("Human Resources", SapMenuUid.AdministrationSetupHumanResources, new List<IMenuItem> {
                        new SapMenuItem("Time Sheet", SapMenuUid.AdministrationSetupHumanResourcesTimeSheet),
                    }),
                    new SapMenuItem("Project Management", SapMenuUid.AdministrationSetupProjectManagement),
                    new SapMenuItem("Production", SapMenuUid.AdministrationSetupProduction),
                    new SapMenuItem("Electronic Documents", SapMenuUid.AdministrationSetupElectronicDocuments),
                }),
                new SapMenuItem("Data Import/Export", SapMenuUid.AdministrationDataImportExport, new List<IMenuItem> {
                    new SapMenuItem("Data Import", SapMenuUid.AdministrationDataImportExportDataImport),
                    new SapMenuItem("Data Export", SapMenuUid.AdministrationDataImportExportDataExport),
                }),
                new SapMenuItem("Utilities", SapMenuUid.AdministrationUtilities),
                new SapMenuItem("Approval Process", SapMenuUid.AdministrationApprovalProcess),
                new SapMenuItem("License", SapMenuUid.AdministrationLicense),
                new SapMenuItem("Integration Service", SapMenuUid.AdministrationIntegrationService),
                new SapMenuItem("Add-Ons", SapMenuUid.AdministrationAddOns),
                new SapMenuItem("Workflow", SapMenuUid.AdministrationWorkflow),
            });

        /// <summary>
        /// Represents the top-level "Financials" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Financials =
            new SapMenuItem("Financials", SapMenuUid.Financials, new List<IMenuItem> {
                new SapMenuItem("Internal Reconciliations", SapMenuUid.FinancialsInternalReconciliations),
                new SapMenuItem("Budget Setup", SapMenuUid.FinancialsBudgetSetup),
                new SapMenuItem("Cost Accounting", SapMenuUid.FinancialsCostAccounting),
                new SapMenuItem("Revaluation", SapMenuUid.FinancialsRevaluation),
                new SapMenuItem("Financial Reports", SapMenuUid.FinancialsFinancialReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.FinancialsFinancialReportsElectronicReports),
                    new SapMenuItem("Accounting", SapMenuUid.FinancialsFinancialReportsAccounting, new List<IMenuItem> {
                        new SapMenuItem("Aging", SapMenuUid.FinancialsFinancialReportsAccountingAging),
                        new SapMenuItem("Tax", SapMenuUid.FinancialsFinancialReportsAccountingTax),
                    }),
                    new SapMenuItem("Financial", SapMenuUid.FinancialsFinancialReportsFinancial),
                    new SapMenuItem("Comparison", SapMenuUid.FinancialsFinancialReportsComparison),
                    new SapMenuItem("Budget Reports", SapMenuUid.FinancialsFinancialReportsBudgetReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "CRM" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Crm =
            new SapMenuItem("CRM", SapMenuUid.Crm, new List<IMenuItem> {
                new SapMenuItem("CRM Report", SapMenuUid.CrmCrmReport, new List<IMenuItem> {
                    new SapMenuItem("Opportunities Reports", SapMenuUid.CrmCrmReportOpportunitiesReports, new List<IMenuItem> {
                        new SapMenuItem("Electronic Reports", SapMenuUid.CrmCrmReportOpportunitiesReportsElectronicReports),
                    }),
                }),
            });

        /// <summary>
        /// Represents the top-level "Opportunities" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Opportunities =
            new SapMenuItem("Opportunities", SapMenuUid.Opportunities, new List<IMenuItem> {
                new SapMenuItem("Opportunities Reports", SapMenuUid.OpportunitiesOpportunitiesReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.OpportunitiesOpportunitiesReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Sales - A/R" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem SalesAr =
            new SapMenuItem("Sales - A/R", SapMenuUid.SalesAr, new List<IMenuItem> {
                new SapMenuItem("Sales Reports", SapMenuUid.SalesArSalesReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.SalesArSalesReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Purchasing - A/P" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem PurchasingAp =
            new SapMenuItem("Purchasing - A/P", SapMenuUid.PurchasingAp, new List<IMenuItem> {
                new SapMenuItem("Purchasing Reports", SapMenuUid.PurchasingApPurchasingReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.PurchasingApPurchasingReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Business Partners" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem BusinessPartners =
            new SapMenuItem("Business Partners", SapMenuUid.BusinessPartners, new List<IMenuItem> {
                new SapMenuItem("Internal Reconciliations", SapMenuUid.BusinessPartnersInternalReconciliations),
                new SapMenuItem("Business Partner Reports", SapMenuUid.BusinessPartnersBusinessPartnerReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.BusinessPartnersBusinessPartnerReportsElectronicReports),
                    new SapMenuItem("Aging", SapMenuUid.BusinessPartnersBusinessPartnerReportsAging),
                    new SapMenuItem("Internal Reconciliation", SapMenuUid.BusinessPartnersBusinessPartnerReportsInternalReconciliation),
                }),
            });

        /// <summary>
        /// Represents the top-level "Banking" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Banking =
            new SapMenuItem("Banking", SapMenuUid.Banking, new List<IMenuItem> {
                new SapMenuItem("Incoming Payments", SapMenuUid.BankingIncomingPayments),
                new SapMenuItem("Deposits", SapMenuUid.BankingDeposits),
                new SapMenuItem("Outgoing Payments", SapMenuUid.BankingOutgoingPayments),
                new SapMenuItem("Bill of Exchange", SapMenuUid.BankingBillOfExchange),
                new SapMenuItem("Bank Statements and External Reconciliations", SapMenuUid.BankingBankStatementsAndExternalReconciliations),
                new SapMenuItem("Banking Reports", SapMenuUid.BankingBankingReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.BankingBankingReportsElectronicReports),
                    new SapMenuItem("External Reconciliation", SapMenuUid.BankingBankingReportsExternalReconciliation),
                    new SapMenuItem("Bill of Exchange", SapMenuUid.BankingBankingReportsBillOfExchange),
                }),
            });

        /// <summary>
        /// Represents the top-level "Inventory" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Inventory =
            new SapMenuItem("Inventory", SapMenuUid.Inventory, new List<IMenuItem> {
                new SapMenuItem("Bin Locations", SapMenuUid.InventoryBinLocations),
                new SapMenuItem("Item Management", SapMenuUid.InventoryItemManagement, new List<IMenuItem> {
                    new SapMenuItem("Item Serial Numbers", SapMenuUid.InventoryItemManagementItemSerialNumbers),
                    new SapMenuItem("Batches", SapMenuUid.InventoryItemManagementBatches),
                }),
                new SapMenuItem("Inventory Transactions", SapMenuUid.InventoryInventoryTransactions, new List<IMenuItem> {
                    new SapMenuItem("Inventory Counting Transactions", SapMenuUid.InventoryInventoryTransactionsInventoryCountingTransactions),
                }),
                new SapMenuItem("Price Lists", SapMenuUid.InventoryPriceLists, new List<IMenuItem> {
                    new SapMenuItem("Special Prices", SapMenuUid.InventoryPriceListsSpecialPrices),
                }),
                new SapMenuItem("Pick and Pack", SapMenuUid.InventoryPickAndPack),
                new SapMenuItem("Inventory Reports", SapMenuUid.InventoryInventoryReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.InventoryInventoryReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Resources" menu in SAP.
        /// </summary>
        public static IMenuItem Resources =
            new SapMenuItem("Resources", SapMenuUid.Resources);

        /// <summary>
        /// Represents the top-level "Production" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Production =
            new SapMenuItem("Production", SapMenuUid.Production, new List<IMenuItem> {
                new SapMenuItem("Production Std Cost Management", SapMenuUid.ProductionProductionStdCostManagement),
                new SapMenuItem("Production Reports", SapMenuUid.ProductionProductionReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ProductionProductionReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "MRP" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Mrp =
            new SapMenuItem("MRP", SapMenuUid.Mrp, new List<IMenuItem> {
                new SapMenuItem("MRP Reports", SapMenuUid.MrpMrpReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.MrpMrpReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Service" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Service =
            new SapMenuItem("Service", SapMenuUid.Service, new List<IMenuItem> {
                new SapMenuItem("Service Reports", SapMenuUid.ServiceServiceReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ServiceServiceReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Human Resources" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem HumanResources =
            new SapMenuItem("Human Resources", SapMenuUid.HumanResources, new List<IMenuItem> {
                new SapMenuItem("Human Resources Reports", SapMenuUid.HumanResourcesHumanResourcesReports, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.HumanResourcesHumanResourcesReportsElectronicReports),
                }),
            });

        /// <summary>
        /// Represents the top-level "Project Management" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem ProjectManagement =
            new SapMenuItem("Project Management", SapMenuUid.ProjectManagement, new List<IMenuItem> {
                new SapMenuItem("Project Reports", SapMenuUid.ProjectManagementProjectReports),
            });

        /// <summary>
        /// Represents the top-level "Reports" menu in SAP, along with its sub-menu items.
        /// </summary>
        public static IMenuItem Reports =
            new SapMenuItem("Reports", SapMenuUid.Reports, new List<IMenuItem> {
                new SapMenuItem("Financials", SapMenuUid.ReportsFinancials, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsFinancialsElectronicReports),
                    new SapMenuItem("Accounting", SapMenuUid.ReportsFinancialsAccounting, new List<IMenuItem> {
                        new SapMenuItem("Aging", SapMenuUid.ReportsFinancialsAccountingAging),
                        new SapMenuItem("Tax", SapMenuUid.ReportsFinancialsAccountingTax),
                    }),
                    new SapMenuItem("Financial", SapMenuUid.ReportsFinancialsFinancial),
                    new SapMenuItem("Comparison", SapMenuUid.ReportsFinancialsComparison),
                    new SapMenuItem("Budget Reports", SapMenuUid.ReportsFinancialsBudgetReports),
                }),
                new SapMenuItem("CRM", SapMenuUid.ReportsCrm, new List<IMenuItem> {
                    new SapMenuItem("Opportunities", SapMenuUid.ReportsCrmOpportunities, new List<IMenuItem> {
                        new SapMenuItem("Electronic Reports", SapMenuUid.ReportsCrmOpportunitiesElectronicReports),
                    }),
                }),
                new SapMenuItem("Opportunities", SapMenuUid.ReportsOpportunities, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsOpportunitiesElectronicReports),
                }),
                new SapMenuItem("Sales and Purchasing", SapMenuUid.ReportsSalesAndPurchasing, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsSalesAndPurchasingElectronicReports, new List<IMenuItem> {
                        new SapMenuItem("Sales Reports", SapMenuUid.ReportsSalesAndPurchasingElectronicReportsSalesReports),
                        new SapMenuItem("Purchasing Reports", SapMenuUid.ReportsSalesAndPurchasingElectronicReportsPurchasingReports),
                    }),
                }),
                new SapMenuItem("Business Partners", SapMenuUid.ReportsBusinessPartners, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsBusinessPartnersElectronicReports),
                    new SapMenuItem("Aging", SapMenuUid.ReportsBusinessPartnersAging),
                    new SapMenuItem("Internal Reconciliation", SapMenuUid.ReportsBusinessPartnersInternalReconciliation),
                }),
                new SapMenuItem("Banking", SapMenuUid.ReportsBanking, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsBankingElectronicReports),
                    new SapMenuItem("External Reconciliation", SapMenuUid.ReportsBankingExternalReconciliation),
                    new SapMenuItem("Bill of Exchange", SapMenuUid.ReportsBankingBillOfExchange),
                }),
                new SapMenuItem("Inventory", SapMenuUid.ReportsInventory, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsInventoryElectronicReports),
                }),
                new SapMenuItem("Production", SapMenuUid.ReportsProduction, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsProductionElectronicReports),
                }),
                new SapMenuItem("MRP", SapMenuUid.ReportsMrp, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsMrpElectronicReports),
                }),
                new SapMenuItem("Service", SapMenuUid.ReportsService, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsServiceElectronicReports),
                }),
                new SapMenuItem("Human Resources", SapMenuUid.ReportsHumanResources, new List<IMenuItem> {
                    new SapMenuItem("Electronic Reports", SapMenuUid.ReportsHumanResourcesElectronicReports),
                }),
                new SapMenuItem("Project Management", SapMenuUid.ReportsProjectManagement),
            });

        /// <summary>
        /// Represents the "Excel Report and Interactive Analysis" menu item in SAP.
        /// </summary>
        public static IMenuItem ExcelReportandInteractiveAnalysis =
            new SapMenuItem("Excel Report and Interactive Analysis", SapMenuUid.ExcelReportAndInteractiveAnalysis);
    }

    /// <summary>
    /// Defines unique identifiers (UIDs) for various SAP menu items.
    /// These UIDs are used to programmatically access or reference specific menus within the SAP Business One application.
    /// The naming convention uses prefixes to reflect the hierarchical structure of the menu.
    /// </summary>
    public enum SapMenuUid {
        /// <summary>
        /// Represents the "Administration" menu.
        /// </summary>
        Administration = 3328,

        /// <summary>
        /// Represents the "Administration > System Initialization" menu.
        /// </summary>
        AdministrationSystemInitialization = 8192,

        /// <summary>
        /// Represents the "Administration > System Initialization > Authorizations" menu.
        /// </summary>
        AdministrationSystemInitializationAuthorizations = 43521,

        /// <summary>
        /// Represents the "Administration > System Initialization > Authorizations > Data Ownership" menu.
        /// </summary>
        AdministrationSystemInitializationAuthorizationsDataOwnership = 3343,

        /// <summary>
        /// Represents the "Administration > System Initialization > Opening Balances" menu.
        /// </summary>
        AdministrationSystemInitializationOpeningBalances = 43522,

        /// <summary>
        /// Represents the "Administration > System Initialization > Implementation Center" menu.
        /// </summary>
        AdministrationSystemInitializationImplementationCenter = 43744,

        /// <summary>
        /// Represents the "Administration > Setup" menu.
        /// </summary>
        AdministrationSetup = 43525,

        /// <summary>
        /// Represents the "Administration > Setup > General" menu.
        /// </summary>
        AdministrationSetupGeneral = 8448,

        /// <summary>
        /// Represents the "Administration > Setup > General > Security" menu.
        /// </summary>
        AdministrationSetupGeneralSecurity = 1600,

        /// <summary>
        /// Represents the "Administration > Setup > Financials" menu.
        /// </summary>
        AdministrationSetupFinancials = 43526,

        /// <summary>
        /// Represents the "Administration > Setup > Financials > G/L Account Determination" menu.
        /// </summary>
        AdministrationSetupFinancialsGlAccountDetermination = 15648,

        /// <summary>
        /// Represents the "Administration > Setup > Financials > Tax" menu.
        /// </summary>
        AdministrationSetupFinancialsTax = 15616,

        /// <summary>
        /// Represents the "Administration > Setup > Opportunities" menu.
        /// </summary>
        AdministrationSetupOpportunities = 17152,

        /// <summary>
        /// Represents the "Administration > Setup > Sales" menu.
        /// </summary>
        AdministrationSetupSales = 17408,

        /// <summary>
        /// Represents the "Administration > Setup > Purchasing" menu.
        /// </summary>
        AdministrationSetupPurchasing = 43527,

        /// <summary>
        /// Represents the "Administration > Setup > Business Partners" menu.
        /// </summary>
        AdministrationSetupBusinessPartners = 43528,

        /// <summary>
        /// Represents the "Administration > Setup > Banking" menu.
        /// </summary>
        AdministrationSetupBanking = 11264,

        /// <summary>
        /// Represents the "Administration > Setup > Inventory" menu.
        /// </summary>
        AdministrationSetupInventory = 11520,

        /// <summary>
        /// Represents the "Administration > Setup > Inventory > Bin Locations" menu.
        /// </summary>
        AdministrationSetupInventoryBinLocations = 1615,

        /// <summary>
        /// Represents the "Administration > Setup > Resources" menu.
        /// </summary>
        AdministrationSetupResources = 11760,

        /// <summary>
        /// Represents the "Administration > Setup > Service" menu.
        /// </summary>
        AdministrationSetupService = 43529,

        /// <summary>
        /// Represents the "Administration > Setup > Human Resources" menu.
        /// </summary>
        AdministrationSetupHumanResources = 60961,

        /// <summary>
        /// Represents the "Administration > Setup > Human Resources > Time Sheet" menu.
        /// </summary>
        AdministrationSetupHumanResourcesTimeSheet = 60962,

        /// <summary>
        /// Represents the "Administration > Setup > Project Management" menu.
        /// </summary>
        AdministrationSetupProjectManagement = 47872,

        /// <summary>
        /// Represents the "Administration > Setup > Production" menu.
        /// </summary>
        AdministrationSetupProduction = 4378,

        /// <summary>
        /// Represents the "Administration > Setup > Electronic Documents" menu.
        /// </summary>
        AdministrationSetupElectronicDocuments = 53245,

        /// <summary>
        /// Represents the "Administration > Data Import/Export" menu.
        /// </summary>
        AdministrationDataImportExport = 43530,

        /// <summary>
        /// Represents the "Administration > Data Import/Export > Data Import" menu.
        /// </summary>
        AdministrationDataImportExportDataImport = 8960,

        /// <summary>
        /// Represents the "Administration > Data Import/Export > Data Export" menu.
        /// </summary>
        AdministrationDataImportExportDataExport = 9216,

        /// <summary>
        /// Represents the "Administration > Utilities" menu.
        /// </summary>
        AdministrationUtilities = 8704,

        /// <summary>
        /// Represents the "Administration > Approval Process" menu.
        /// </summary>
        AdministrationApprovalProcess = 14848,

        /// <summary>
        /// Represents the "Administration > License" menu.
        /// </summary>
        AdministrationLicense = 43524,

        /// <summary>
        /// Represents the "Administration > Integration Service" menu.
        /// </summary>
        AdministrationIntegrationService = 39716,

        /// <summary>
        /// Represents the "Administration > Add-Ons" menu.
        /// </summary>
        AdministrationAddOns = 43523,

        /// <summary>
        /// Represents the "Administration > Workflow" menu.
        /// </summary>
        AdministrationWorkflow = 2364,

        /// <summary>
        /// Represents the "Financials" menu.
        /// </summary>
        Financials = 1536,

        /// <summary>
        /// Represents the "Financials > Internal Reconciliations" menu.
        /// </summary>
        FinancialsInternalReconciliations = 9460,

        /// <summary>
        /// Represents the "Financials > Budget Setup" menu.
        /// </summary>
        FinancialsBudgetSetup = 10496,

        /// <summary>
        /// Represents the "Financials > Cost Accounting" menu.
        /// </summary>
        FinancialsCostAccounting = 1792,

        /// <summary>
        /// Represents the "Financials > Revaluation" menu.
        /// </summary>
        FinancialsRevaluation = 12544,

        /// <summary>
        /// Represents the "Financials > Financial Reports" menu.
        /// </summary>
        FinancialsFinancialReports = 43531,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Electronic Reports" menu.
        /// </summary>
        FinancialsFinancialReportsElectronicReports = 30338,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Accounting" menu.
        /// </summary>
        FinancialsFinancialReportsAccounting = 13056,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Accounting > Aging" menu.
        /// </summary>
        FinancialsFinancialReportsAccountingAging = 4096,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Accounting > Tax" menu.
        /// </summary>
        FinancialsFinancialReportsAccountingTax = 43532,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Financial" menu.
        /// </summary>
        FinancialsFinancialReportsFinancial = 9728,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Comparison" menu.
        /// </summary>
        FinancialsFinancialReportsComparison = 1648,

        /// <summary>
        /// Represents the "Financials > Financial Reports > Budget Reports" menu.
        /// </summary>
        FinancialsFinancialReportsBudgetReports = 10240,

        /// <summary>
        /// Represents the "CRM" menu.
        /// </summary>
        Crm = 43679,

        /// <summary>
        /// Represents the "CRM > CRM Report" menu.
        /// </summary>
        CrmCrmReport = 43680,

        /// <summary>
        /// Represents the "CRM > CRM Report > Opportunities Reports" menu.
        /// </summary>
        CrmCrmReportOpportunitiesReports = 43689,

        /// <summary>
        /// Represents the "CRM > CRM Report > Opportunities Reports > Electronic Reports" menu.
        /// </summary>
        CrmCrmReportOpportunitiesReportsElectronicReports = 43690,

        /// <summary>
        /// Represents the "Opportunities" menu.
        /// </summary>
        Opportunities = 2560,

        /// <summary>
        /// Represents the "Opportunities > Opportunities Reports" menu.
        /// </summary>
        OpportunitiesOpportunitiesReports = 43533,

        /// <summary>
        /// Represents the "Opportunities > Opportunities Reports > Electronic Reports" menu.
        /// </summary>
        OpportunitiesOpportunitiesReportsElectronicReports = 30339,

        /// <summary>
        /// Represents the "Sales - A/R" menu.
        /// </summary>
        SalesAr = 2048,

        /// <summary>
        /// Represents the "Sales - A/R > Sales Reports" menu.
        /// </summary>
        SalesArSalesReports = 12800,

        /// <summary>
        /// Represents the "Sales - A/R > Sales Reports > Electronic Reports" menu.
        /// </summary>
        SalesArSalesReportsElectronicReports = 30341,

        /// <summary>
        /// Represents the "Purchasing - A/P" menu.
        /// </summary>
        PurchasingAp = 2304,

        /// <summary>
        /// Represents the "Purchasing - A/P > Purchasing Reports" menu.
        /// </summary>
        PurchasingApPurchasingReports = 43534,

        /// <summary>
        /// Represents the "Purchasing - A/P > Purchasing Reports > Electronic Reports" menu.
        /// </summary>
        PurchasingApPurchasingReportsElectronicReports = 30344,

        /// <summary>
        /// Represents the "Business Partners" menu.
        /// </summary>
        BusinessPartners = 43535,

        /// <summary>
        /// Represents the "Business Partners > Internal Reconciliations" menu.
        /// </summary>
        BusinessPartnersInternalReconciliations = 9458,

        /// <summary>
        /// Represents the "Business Partners > Business Partner Reports" menu.
        /// </summary>
        BusinessPartnersBusinessPartnerReports = 43536,

        /// <summary>
        /// Represents the "Business Partners > Business Partner Reports > Electronic Reports" menu.
        /// </summary>
        BusinessPartnersBusinessPartnerReportsElectronicReports = 30346,

        /// <summary>
        /// Represents the "Business Partners > Business Partner Reports > Aging" menu.
        /// </summary>
        BusinessPartnersBusinessPartnerReportsAging = 43548,

        /// <summary>
        /// Represents the "Business Partners > Business Partner Reports > Internal Reconciliation" menu.
        /// </summary>
        BusinessPartnersBusinessPartnerReportsInternalReconciliation = 51199,

        /// <summary>
        /// Represents the "Banking" menu.
        /// </summary>
        Banking = 43537,

        /// <summary>
        /// Represents the "Banking > Incoming Payments" menu.
        /// </summary>
        BankingIncomingPayments = 2816,

        /// <summary>
        /// Represents the "Banking > Deposits" menu.
        /// </summary>
        BankingDeposits = 14592,

        /// <summary>
        /// Represents the "Banking > Outgoing Payments" menu.
        /// </summary>
        BankingOutgoingPayments = 43538,

        /// <summary>
        /// Represents the "Banking > Bill of Exchange" menu.
        /// </summary>
        BankingBillOfExchange = 43539,

        /// <summary>
        /// Represents the "Banking > Bank Statements and External Reconciliations" menu.
        /// </summary>
        BankingBankStatementsAndExternalReconciliations = 11008,

        /// <summary>
        /// Represents the "Banking > Banking Reports" menu.
        /// </summary>
        BankingBankingReports = 51197,

        /// <summary>
        /// Represents the "Banking > Banking Reports > Electronic Reports" menu.
        /// </summary>
        BankingBankingReportsElectronicReports = 30348,

        /// <summary>
        /// Represents the "Banking > Banking Reports > External Reconciliation" menu.
        /// </summary>
        BankingBankingReportsExternalReconciliation = 51195,

        /// <summary>
        /// Represents the "Banking > Banking Reports > Bill of Exchange" menu.
        /// </summary>
        BankingBankingReportsBillOfExchange = 51193,

        /// <summary>
        /// Represents the "Inventory" menu.
        /// </summary>
        Inventory = 3072,

        /// <summary>
        /// Represents the "Inventory > Bin Locations" menu.
        /// </summary>
        InventoryBinLocations = 49153,

        /// <summary>
        /// Represents the "Inventory > Item Management" menu.
        /// </summary>
        InventoryItemManagement = 15872,

        /// <summary>
        /// Represents the "Inventory > Item Management > Item Serial Numbers" menu.
        /// </summary>
        InventoryItemManagementItemSerialNumbers = 12032,

        /// <summary>
        /// Represents the "Inventory > Item Management > Batches" menu.
        /// </summary>
        InventoryItemManagementBatches = 12288,

        /// <summary>
        /// Represents the "Inventory > Inventory Transactions" menu.
        /// </summary>
        InventoryInventoryTransactions = 43540,

        /// <summary>
        /// Represents the "Inventory > Inventory Transactions > Inventory Counting Transactions" menu.
        /// </summary>
        InventoryInventoryTransactionsInventoryCountingTransactions = 43569,

        /// <summary>
        /// Represents the "Inventory > Price Lists" menu.
        /// </summary>
        InventoryPriceLists = 43541,

        /// <summary>
        /// Represents the "Inventory > Price Lists > Special Prices" menu.
        /// </summary>
        InventoryPriceListsSpecialPrices = 11776,

        /// <summary>
        /// Represents the "Inventory > Pick and Pack" menu.
        /// </summary>
        InventoryPickAndPack = 16640,

        /// <summary>
        /// Represents the "Inventory > Inventory Reports" menu.
        /// </summary>
        InventoryInventoryReports = 1760,

        /// <summary>
        /// Represents the "Inventory > Inventory Reports > Electronic Reports" menu.
        /// </summary>
        InventoryInventoryReportsElectronicReports = 30350,

        /// <summary>
        /// Represents the "Resources" menu.
        /// </summary>
        Resources = 13312,

        /// <summary>
        /// Represents the "Production" menu.
        /// </summary>
        Production = 4352,

        /// <summary>
        /// Represents the "Production > Production Std Cost Management" menu.
        /// </summary>
        ProductionProductionStdCostManagement = 43570,

        /// <summary>
        /// Represents the "Production > Production Reports" menu.
        /// </summary>
        ProductionProductionReports = 43542,

        /// <summary>
        /// Represents the "Production > Production Reports > Electronic Reports" menu.
        /// </summary>
        ProductionProductionReportsElectronicReports = 30352,

        /// <summary>
        /// Represents the "MRP" menu.
        /// </summary>
        Mrp = 43543,

        /// <summary>
        /// Represents the "MRP > MRP Reports" menu.
        /// </summary>
        MrpMrpReports = 39706,

        /// <summary>
        /// Represents the "MRP > MRP Reports > Electronic Reports" menu.
        /// </summary>
        MrpMrpReportsElectronicReports = 39707,

        /// <summary>
        /// Represents the "Service" menu.
        /// </summary>
        Service = 3584,

        /// <summary>
        /// Represents the "Service > Service Reports" menu.
        /// </summary>
        ServiceServiceReports = 7680,

        /// <summary>
        /// Represents the "Service > Service Reports > Electronic Reports" menu.
        /// </summary>
        ServiceServiceReportsElectronicReports = 30354,

        /// <summary>
        /// Represents the "Human Resources" menu.
        /// </summary>
        HumanResources = 43544,

        /// <summary>
        /// Represents the "Human Resources > Human Resources Reports" menu.
        /// </summary>
        HumanResourcesHumanResourcesReports = 16128,

        /// <summary>
        /// Represents the "Human Resources > Human Resources Reports > Electronic Reports" menu.
        /// </summary>
        HumanResourcesHumanResourcesReportsElectronicReports = 30356,

        /// <summary>
        /// Represents the "Project Management" menu.
        /// </summary>
        ProjectManagement = 48896,

        /// <summary>
        /// Represents the "Project Management > Project Reports" menu.
        /// </summary>
        ProjectManagementProjectReports = 49024,

        /// <summary>
        /// Represents the "Reports" menu.
        /// </summary>
        Reports = 43545,

        /// <summary>
        /// Represents the "Reports > Financials" menu.
        /// </summary>
        ReportsFinancials = 43546,

        /// <summary>
        /// Represents the "Reports > Financials > Electronic Reports" menu.
        /// </summary>
        ReportsFinancialsElectronicReports = 30338,

        /// <summary>
        /// Represents the "Reports > Financials > Accounting" menu.
        /// </summary>
        ReportsFinancialsAccounting = 43547,

        /// <summary>
        /// Represents the "Reports > Financials > Accounting > Aging" menu.
        /// </summary>
        ReportsFinancialsAccountingAging = 4096,

        /// <summary>
        /// Represents the "Reports > Financials > Accounting > Tax" menu.
        /// </summary>
        ReportsFinancialsAccountingTax = 43549,

        /// <summary>
        /// Represents the "Reports > Financials > Financial" menu.
        /// </summary>
        ReportsFinancialsFinancial = 43550,

        /// <summary>
        /// Represents the "Reports > Financials > Comparison" menu.
        /// </summary>
        ReportsFinancialsComparison = 43551,

        /// <summary>
        /// Represents the "Reports > Financials > Budget Reports" menu.
        /// </summary>
        ReportsFinancialsBudgetReports = 10240,

        /// <summary>
        /// Represents the "Reports > CRM" menu.
        /// </summary>
        ReportsCrm = 43681,

        /// <summary>
        /// Represents the "Reports > CRM > Opportunities" menu.
        /// </summary>
        ReportsCrmOpportunities = 43691,

        /// <summary>
        /// Represents the "Reports > CRM > Opportunities > Electronic Reports" menu.
        /// </summary>
        ReportsCrmOpportunitiesElectronicReports = 43692,

        /// <summary>
        /// Represents the "Reports > Opportunities" menu.
        /// </summary>
        ReportsOpportunities = 43553,

        /// <summary>
        /// Represents the "Reports > Opportunities > Electronic Reports" menu.
        /// </summary>
        ReportsOpportunitiesElectronicReports = 30339,

        /// <summary>
        /// Represents the "Reports > Sales and Purchasing" menu.
        /// </summary>
        ReportsSalesAndPurchasing = 43554,

        /// <summary>
        /// Represents the "Reports > Sales and Purchasing > Electronic Reports" menu.
        /// </summary>
        ReportsSalesAndPurchasingElectronicReports = 1559,

        /// <summary>
        /// Represents the "Reports > Sales and Purchasing > Electronic Reports > Sales Reports" menu.
        /// </summary>
        ReportsSalesAndPurchasingElectronicReportsSalesReports = 30341,

        /// <summary>
        /// Represents the "Reports > Sales and Purchasing > Electronic Reports > Purchasing Reports" menu.
        /// </summary>
        ReportsSalesAndPurchasingElectronicReportsPurchasingReports = 30344,

        /// <summary>
        /// Represents the "Reports > Business Partners" menu.
        /// </summary>
        ReportsBusinessPartners = 43555,

        /// <summary>
        /// Represents the "Reports > Business Partners > Electronic Reports" menu.
        /// </summary>
        ReportsBusinessPartnersElectronicReports = 30346,

        /// <summary>
        /// Represents the "Reports > Business Partners > Aging" menu.
        /// </summary>
        ReportsBusinessPartnersAging = 43548,

        /// <summary>
        /// Represents the "Reports > Business Partners > Internal Reconciliation" menu.
        /// </summary>
        ReportsBusinessPartnersInternalReconciliation = 51188,

        /// <summary>
        /// Represents the "Reports > Banking" menu.
        /// </summary>
        ReportsBanking = 51196,

        /// <summary>
        /// Represents the "Reports > Banking > Electronic Reports" menu.
        /// </summary>
        ReportsBankingElectronicReports = 30348,

        /// <summary>
        /// Represents the "Reports > Banking > External Reconciliation" menu.
        /// </summary>
        ReportsBankingExternalReconciliation = 51191,

        /// <summary>
        /// Represents the "Reports > Banking > Bill of Exchange" menu.
        /// </summary>
        ReportsBankingBillOfExchange = 51189,

        /// <summary>
        /// Represents the "Reports > Inventory" menu.
        /// </summary>
        ReportsInventory = 14080,

        /// <summary>
        /// Represents the "Reports > Inventory > Electronic Reports" menu.
        /// </summary>
        ReportsInventoryElectronicReports = 30350,

        /// <summary>
        /// Represents the "Reports > Production" menu.
        /// </summary>
        ReportsProduction = 43557,

        /// <summary>
        /// Represents the "Reports > Production > Electronic Reports" menu.
        /// </summary>
        ReportsProductionElectronicReports = 30352,

        /// <summary>
        /// Represents the "Reports > MRP" menu.
        /// </summary>
        ReportsMrp = 1603,

        /// <summary>
        /// Represents the "Reports > MRP > Electronic Reports" menu.
        /// </summary>
        ReportsMrpElectronicReports = 39707,

        /// <summary>
        /// Represents the "Reports > Service" menu.
        /// </summary>
        ReportsService = 43556,

        /// <summary>
        /// Represents the "Reports > Service > Electronic Reports" menu.
        /// </summary>
        ReportsServiceElectronicReports = 30354,

        /// <summary>
        /// Represents the "Reports > Human Resources" menu.
        /// </summary>
        ReportsHumanResources = 43558,

        /// <summary>
        /// Represents the "Reports > Human Resources > Electronic Reports" menu.
        /// </summary>
        ReportsHumanResourcesElectronicReports = 30356,

        /// <summary>
        /// Represents the "Reports > Project Management" menu.
        /// </summary>
        ReportsProjectManagement = 39781,

        /// <summary>
        /// Represents the "Excel Report and Interactive Analysis" menu.
        /// </summary>
        ExcelReportAndInteractiveAnalysis = 44544,
    }
}