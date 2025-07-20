using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace SAPUtils.Enums {
    /// <summary>
    /// Represents the types of forms available in SAP Business One.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to identify different types of forms that can be utilized
    /// within the SAP Business One environment. The form types correspond to the unique
    /// identifiers for SAP Business One UI objects.
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum FormTypes {
        DragRelate = 7,

        QueryWizard = 10,

        EUSalesReportSelectionCriteria = 19,

        DefineandUpdateSerialNumbersSelectionCriteria = 20,

        DefineandUpdateSerialNumbers = 21,

        SerialNumberTransactionsReportSelectionCriteria = 22,

        SerialNumberTransactionsReport = 23,

        SerialNumbersSelectionCriteria = 25,

        SerialNumberCompletion = 26,

        SerialNumberCompletionSelectionCriteria = 27,

        ManageUserFields = 31,

        DocumentJournal = 39,

        BatchNoforReceiptSelectionCriteria = 40,

        DefineandUpdateBatchNumbers = 41,

        BatchNumberSelectionCriteria = 42,

        BatchNumberCompletion = 43,

        BatchNumberCompletion2 = 44,

        BatchNumberTransactionsReport = 45,

        BatchNumberTransactionsReportSelectionCriteria = 46,

        TaxReport = 47,

        EUSalesReport = 48,

        DefineSalesTaxJurisdictionsSelectionCriteria = 50,

        DefineSalesTaxCodes = 51,

        TaxReportSelectionCriteria = 53,

        DefineWarehouses = 62,

        DefineItemGroups = 63,

        DefineAccountSegmentation = 64,

        PurchaseOrderConfirmation = 65,

        DefinePercentsSegmentCodes = 66,

        DefineInventoryCycles = 67,

        TaxReportSelectionCriteria2 = 71,

        TaxReport2 = 72,

        ReverseTransactions = 75,

        TaxJurisdiction = 76,

        ReleaseListSelectionCriteria = 80,

        ReleaseList = 81,

        LinkInvoicestoPaymentsSelectionCriteria = 83,

        LinkInvoicestoPayments = 84,

        PickList = 85,

        InactiveItems = 90,

        InactiveItemsSelectionCriteria = 91,

        InactiveCustomersSelectionCriteria = 92,

        InactiveCustomers = 93,

        LastPricesReport = 96,

        TrialBalance = 120,

        BP = 129,

        ARInvoice = 133,

        BusinessPartnerMasterData = 134,

        CompanyDetails = 136,

        CorrectionInvoice = 137,

        GeneralSettings = 138,

        Order = 139,

        Delivery = 140,

        APInvoice = 141,

        PurchaseOrder = 142,

        GoodsReceiptPO = 143,

        PaymentMeans = 146,

        DefineCurrencies = 148,

        Quotation = 149,

        ItemMasterData = 150,

        OpenItemsList = 152,

        InventoryStatus = 154,

        PriceLists = 155,

        PriceList = 157,

        TransactionJournalReport = 161,

        BalanceSheetSelectionCriteria = 165,

        TrialBalanceSelectionCriteria = 166,

        GLAccountsOpeningBalance = 168,

        CommandCentre = 169,

        IncomingPayments = 170,

        CycleCountRecommendations = 171,

        DocumentNumbering = 172,

        DefineCustomerGroups = 174,

        DefinePaymentTerms = 177,

        ARCreditMemo = 179,

        Returns = 180,

        APCreditMemo = 181,

        GoodsReturns = 182,

        PrintPreferences = 183,

        DocumentPrintingSelectionCriteria = 184,

        SendMessage = 188,

        TestReportBalanceSettings = 189,

        SaveasDistributionList = 190,

        Print = 191,

        ExecuteCommands = 194,

        AlertsManagement = 202,

        SaveReport = 210,

        DocumentSettings = 228,

        JournalVouchers = 229,

        GrossProfitofOrder = 239,

        GrossProfitofDelivery = 240,

        GrossProfitofOutgoingInvoice = 241,

        TrialBalanceBudgetReportSelectionCriteria = 245,

        DefineVendorGroups = 247,

        DefineFormattedSearch = 251,

        BalanceSheetBudgetReportSelectionCriteria = 260,

        BalanceSheetComparisonSelectionCriteria = 265,

        ProfitandLossStatementComparisonSelectionCriteria = 267,

        TrialBalanceComparisonSelectionCriteria = 280,

        ShowHistory = 285,

        PriceListItemDetails = 290,

        DefineHierarchiesforPriceList = 291,

        OrganizeUserMenu = 296,

        DynamicOpportunityAnalysis = 305,

        OpportunityAnalysisSelectionCriteria = 307,

        OpportunitiesPipeline = 309,

        OpportunitiesWon = 313,

        OpportunityList = 314,

        OpportunitiesWonSelectionCriteria = 315,

        StageAnalysisSelectionCriteria = 319,

        SalesOpportunity = 320,

        SpecialPricesItemDetails = 333,

        SpecialPricesHierarchies = 335,

        CopySpecialPricestoSelectionCriteria = 336,

        Unknown = 337,

        UpdateSpecialPricesGlobally = 339,

        GLAccountDetermination = 350,

        DefineSalesStages = 352,

        ExchangeRateDifferences = 369,

        ExchangeRateDifferencesSelectionCriteria = 370,

        ConversionDifferences = 371,

        ConversionDifferencesSelectionCriteria = 372,

        ProcessExternalBankStatement = 385,

        ReconciliationSelectionCriteria = 386,

        Reconciliation = 387,

        ManagePreviousReconciliationsSelectionCriteria = 388,

        ManagePreviousReconciliations = 389,

        JournalEntry = 390,

        JournalEntry2 = 392,

        TaxReporting = 401,

        AdvancesonCorporateIncomeTaxonSalesReport = 402,

        TaxReportDetailedbyMonths = 403,

        PeriodEndClosingSelectionCriteria = 411,

        DefineAddressFormats = 419,

        ProfitandLossStatementSelectionCriteria = 420,

        ProfitandLossStatementBudgetReportSelectionCriteria = 421,

        DefineBusinessPartnerProperties = 422,

        DefineCreditCards = 424,

        PaymentstoVendors = 426,

        DefineItemProperties = 429,

        BusinessPartnersOpeningBalance = 430,

        DefineCreditCardPaymentMethods = 435,

        DefineCreditCardPayment = 436,

        CreditCardManagement = 437,

        CreditCardManagementSelectionCriteria = 440,

        DefineBudgetDistributionMethods = 441,

        DefineBudget = 443,

        DefineBudget2 = 444,

        BudgetReportSelectionCriteria = 446,

        BudgetReport = 447,

        RestoreBudgetBalances = 448,

        BudgetScenarios = 452,

        ImportBudgetScenarios = 453,

        GeneralLedger = 501,

        TransactionReportbyProjectsSelectionCriteria = 502,

        PaymentWizard = 504,

        DefinePaymentMethods = 505,

        VendorWithholdingTaxSelectionCriteria = 510,

        WithholdingTaxReport = 515,

        CheckDocumentNumbering = 520,

        CreateEditCategories = 521,

        Documentsserialnumberinglist = 530,

        ItemQuery = 540,

        InventoryPostingListSelectionCriteria = 550,

        ItemsListSelectionCriteria = 600,

        ItemsList = 601,

        CheckFundSelectionCriteria = 603,

        GeneralLedgerSelectionCriteria = 604,

        Deposit = 606,

        CheckFund = 607,

        DocumentJournalSelectionCriteria = 609,

        PostdatedCheckDeposit = 612,

        PostdatedCreditVoucherDeposit = 614,

        PostingPeriod = 636,

        DefineHierarchiesandExpansions = 640,

        ContactswithBusinessPartners = 651,

        DefineCashDiscount = 653,

        PaymentDraftsReport = 655,

        DefineCommissionGroups = 664,

        DefineSalesEmployees = 666,

        SpecialPricesforBusinessPartners = 668,

        RecurringPostings = 670,

        DefineBillofMaterials = 672,

        OpenWorkOrdersReport = 673,

        BillofMaterialsReportSelectionCriteria = 674,

        ProductionRecommendations = 675,

        WorkOrder = 677,

        BillofMaterialsReport = 679,

        Confirmationforrecurringpostings = 680,

        CycleCountRecommendationsSelectionCriteria = 681,

        RestoreItemBalances = 682,

        InventoryStatusSelectionCriteria = 689,

        FinancialReportTemplateExpansion = 703,

        FinancialReportTemplates = 704,

        DefineBanks = 705,

        FinancialReportTemplate = 706,

        FormulaforProfitandLossTemplate = 708,

        DefineTransactionCodes = 710,

        DefineProjects = 711,

        DefinePercentsRates = 712,

        DefineSalesTaxJurisdictionTypes = 713,

        GoodsIssue = 720,

        GoodsReceipt = 721,

        DefineSalesStages2 = 733,

        DefinePartners = 735,

        DefineCompetitors = 736,

        EditChartofAccounts = 750,

        EditChartofAccounts2 = 751,

        TransactionJournalReportSelectionCriteria = 752,

        AccountCodeGenerator = 753,

        InventoryinWarehouseReport = 771,

        InventoryinWarehouseReportSelectionCriteria = 772,

        DefineLocations = 776,

        Define1099Table = 779,

        PostingTemplates = 800,

        ChartofAccounts = 804,

        ChartofAccounts2 = 806,

        DefineProfitCenters = 810,

        DefineDistributionRules = 811,

        TableofProfitCentersandDistributionRules = 812,

        ProfitCenterReportSelectionCriteria = 819,

        ChooseCompany = 820,

        ProfitCenterReport = 823,

        InventoryTracking = 840,

        CheckandRestoreFormerReconciliations = 841,

        ChecksforPaymentDrafts = 850,

        DefineUsers = 852,

        ChecksforPayment = 854,

        GLAccountsandBusinessPartners = 855,

        VoidingChecksforPayment = 856,

        SalesAnalysisReportSelectionCriteria = 857,

        VoidingChecksforPayment2 = 858,

        RestoreGLAccountandBusinessPartnerBalances = 864,

        DefineIndexes = 865,

        Defineforeigncurrencyexchangerates = 866,

        Unknown2 = 869,

        PurchaseAnalysisSelectionCriteria = 870,

        DefineInterestRates = 876,

        DefineCreditVendors = 878,

        GLAccountsandBusinessPartnersSelectionCriteria = 892,

        DefineLengthandWidthUoM = 893,

        DefineWeightUoM = 894,

        DefineTaxGroups = 895,

        DefineCustomsGroups = 896,

        DefineManufacturers = 897,

        DefineLandedCosts = 898,

        DefineShippingTypes = 899,

        InventoryValuationSelectionCriteria = 900,

        InventoryValuationReport = 901,

        UpdateAfterInventory = 902,

        Enteringstockbalancesheet = 906,

        BeginningQuantitiesandCycleCounting = 907,

        YearTransfer = 916,

        BusinessPartnersOpeningBalanceSelectionCriteria = 922,

        GLAccountsOpeningBalanceSelectionCriteria = 923,

        StockTransfer = 940,

        DefineCountries = 941,

        AutomaticSummaryWizard = 953,

        DefineDiscountGroups = 958,

        SearchResults = 959,

        CashFlowSelectionCriteria = 960,

        CashFlow = 961,

        CustomerReceivablesAgingSelectionCriteria = 962,

        VendorLiabilitiesAgingSelectionCriteria = 963,

        CustomerReceivablesAging = 964,

        VendorLiabilitiesAging = 965,

        Restore = 968,

        RestoreWizard = 969,

        RestoreOpenCheckBalances = 971,

        ContactOverviewSelectionCriteria = 975,

        ContactOverview = 976,

        DataMigrationPackages = 977,

        Chooselanguage = 980,

        Createnewlanguage = 981,

        LandedCosts = 992,

        DefineBusinessPartnerCatalogNumbers = 993,

        GlobalUpdatetoBusinessPartnerCatalogNumbers = 994,

        Settings = 998,

        SalesJournal = 1011,

        CashReport = 1012,

        TaxReportPurchasingPreferences = 1020,

        TaxReport3 = 1021,

        DocumentDraftsSelectionCriteria = 3001,

        DocumentDrafts = 3002,

        EditingSelectionCriteria1099 = 3900,

        Editing1099 = 3901,

        ReportSelectionCriteria1099 = 3904,

        Report1099 = 3905,

        DetailedReportperVendor1099 = 3906,

        OpeningBalanceSelectionCriteria1099 = 3907,

        OpeningBalance1099 = 3908,

        DefineLatePaymentsFees = 3910,

        EST = 3915,

        UserReports = 4666,

        UpdateControlReport = 5003,

        DefineBusinessPartnerPriorities = 8001,

        DefineDunningLevels = 8002,

        DefinePaymentBlocks = 8004,

        DefineServiceCallStatuses = 8008,

        DefineDoubtfulReceivables = 8018,

        SalesAnalysisforCustomer = 20209,

        SalesAnalysisforCustomer2 = 20210,

        PurchasesAnalysisforVendors = 20221,

        SalesAnalysisReportbyCustomerDetailed = 20232,

        SalesAnalysisReportbyItemDetailed = 20240,

        PurchaseAnalysisRepbyVendorDetailed = 20251,

        PurchaseAnalysisRepbyVendorDetailed2 = 20259,

        DeliveryNotesSelectionCriteria = 20302,

        DeliverySummaryReportSelectionCriteria = 20303,

        InvoicesSummaryReportSelectionCriteria = 20306,

        Exportgeneraldatafile = 20307,

        Unknown3 = 20310,

        DefineRetailStores = 20311,

        InterestCalculationReportSelectionCriteria = 20320,

        InterestReport = 20321,

        PaymentstoVendors2 = 20330,

        UnapprovedPaymentstoVendorsReport = 20331,

        CreditCardSummarySelectionCriteria = 20450,

        CreditCardSummary = 20451,

        DefineUsers2 = 20700,

        DefinePaymentRunDefaults = 20702,

        DefineAlternativeItems = 40014,

        DefineApprovalStages = 50101,

        DefineApprovalTemplates = 50102,

        ApprovalStatusReportSelectionCriteria = 50104,

        ApprovalStatusReport = 50105,

        ApprovalDecisionReportSelectionCriteria = 50107,

        ApprovalDecisionReport = 50108,

        MachineID = 60000,

        Properties = 60001,

        ConnectedUsers = 60002,

        LicenseRemoval = 60003,

        BillofExchangeManagment = 60050,

        BillofExchangeManagment2 = 60051,

        BillofExchangeTransactions = 60052,

        BillofExchangeReceivables = 60053,

        BillofExchangePayables = 60056,

        ARInvoice2 = 60090,

        ARReserveInvoice = 60091,

        APReserveInvoice = 60092,

        EmployeeMasterData = 60100,

        EmployeePhoneBookReportSelectionCriteria = 60105,

        EmployeesList = 60106,

        PhoneBook = 60107,

        EmployeesAbsenceReport = 60109,

        ServiceCall = 60110,

        KnowledgeBaseSolution = 60120,

        ContractTemplates = 60125,

        ServiceContract = 60126,

        ServiceCallsReportSelectionCriteria = 60130,

        ServiceCalls = 60131,

        ServiceMonitor = 60133,

        ServiceContractsReportSelectionCriteria = 60135,

        ServiceContracts = 60136,

        AverageClosureTime = 60138,

        AverageClosureTimeReportSelectionCriteria = 60139,

        MyOpenServiceCalls = 60140,

        MyServiceCalls = 60141,

        MyOverdueServiceCalls = 60142,

        CustomerEquipmentCard = 60150,

        CustomerEquipmentReportSelectionCriteria = 60151,

        CustomerEquipmentReport = 60152,

        BalanceSheetComparisonSelectionCriteria2 = 60265,

        TaxReportSelectionCriteria3 = 60300,

        WithholdingTaxReportSelectionCriteria = 60301,

        TaxSummaryReport = 60350,

        PeriodEndClosingSelectionCriteria2 = 60410,

        PeriodEndClosing = 60414,

        BillofExchangeFund = 60501,

        BillofExchangeFundSelectionCriteria = 60502,

        DocumentJournal2 = 60555,

        WTMode = 65000,

        Report347 = 65011,

        Report349 = 65014,

        DefineWithholdingTaxCodes = 65015,

        UpdateParentItemPricesSelectionCriteria = 65018,
    }

    public static class FormTypesExtensions {
        public static bool Is(this FormTypes enumVal, string str) {
            return Convert.ToInt32(enumVal).ToString() == str;
        }
    }
}