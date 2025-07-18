using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SAPbobsCOM;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for the <see cref="UDFLinkedSystemObjectTypesEnum"/> enumeration
    /// to retrieve information related to system object types, such as their associated table names.
    /// </summary>
    /// <remarks>
    /// This class enables functionalities related to SAP Business One UDF-linked system object types.
    /// </remarks>
    /// <seealso cref="UDFLinkedSystemObjectTypesEnum"/>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class UDFLinkedSystemObjectTypesEnumExtension {
        /// <summary>
        /// A static readonly dictionary that maps <see cref="UDFLinkedSystemObjectTypesEnum"/> enum values
        /// to corresponding SAP Business One table names as strings.
        /// </summary>
        /// <remarks>
        /// This dictionary provides a mapping between system object types and their respective
        /// SAP database table names, enabling lookup functionality for various SAP system objects.
        /// </remarks>
        /// <seealso cref="UDFLinkedSystemObjectTypesEnum"/>
        /// <seealso cref="UDFLinkedSystemObjectTypesEnumExtension.GetTableName(UDFLinkedSystemObjectTypesEnum)"/>
        /// <seealso cref="UDFLinkedSystemObjectTypesEnumExtension.GetTableName(UDFLinkedSystemObjectTypesEnum?)"/>
        private static readonly Dictionary<UDFLinkedSystemObjectTypesEnum, string> TableNames = new Dictionary<UDFLinkedSystemObjectTypesEnum, string> {
            { UDFLinkedSystemObjectTypesEnum.ulChartOfAccounts, "OACT" },
            { UDFLinkedSystemObjectTypesEnum.ulBusinessPartners, "OCRD" },
            { UDFLinkedSystemObjectTypesEnum.ulBanks, "ODSC" },
            { UDFLinkedSystemObjectTypesEnum.ulItems, "OITM" },
            { UDFLinkedSystemObjectTypesEnum.ulUsers, "OUSR" },
            { UDFLinkedSystemObjectTypesEnum.ulInvoices, "OINV" },
            { UDFLinkedSystemObjectTypesEnum.ulCreditNotes, "ORIN" },
            { UDFLinkedSystemObjectTypesEnum.ulDeliveryNotes, "ODLN" },
            { UDFLinkedSystemObjectTypesEnum.ulReturns, "ORDN" },
            { UDFLinkedSystemObjectTypesEnum.ulOrders, "ORDR" },
            { UDFLinkedSystemObjectTypesEnum.ulPurchaseInvoices, "OPCH" },
            { UDFLinkedSystemObjectTypesEnum.ulPurchaseCreditNotes, "ORPC" },
            { UDFLinkedSystemObjectTypesEnum.ulPurchaseDeliveryNotes, "OPDN" },
            { UDFLinkedSystemObjectTypesEnum.ulPurchaseReturns, "ORPD" },
            { UDFLinkedSystemObjectTypesEnum.ulPurchaseOrders, "OPOR" },
            { UDFLinkedSystemObjectTypesEnum.ulQuotations, "OQUT" },
            { UDFLinkedSystemObjectTypesEnum.ulIncomingPayments, "ORCT" },
            { UDFLinkedSystemObjectTypesEnum.ulDepositsService, "ODPS" },
            { UDFLinkedSystemObjectTypesEnum.ulJournalEntries, "OJDT" },
            { UDFLinkedSystemObjectTypesEnum.ulContacts, "OCLG" },
            { UDFLinkedSystemObjectTypesEnum.ulVendorPayments, "OVPM" },
            { UDFLinkedSystemObjectTypesEnum.ulChecksforPayment, "OCHO" },
            { UDFLinkedSystemObjectTypesEnum.ulInventoryGenEntry, "OIGN" },
            { UDFLinkedSystemObjectTypesEnum.ulInventoryGenExit, "OIGE" },
            { UDFLinkedSystemObjectTypesEnum.ulWarehouses, "OWHS" },
            { UDFLinkedSystemObjectTypesEnum.ulProductTrees, "OITT" },
            { UDFLinkedSystemObjectTypesEnum.ulStockTransfer, "OWTR" },
            { UDFLinkedSystemObjectTypesEnum.ulSalesOpportunities, "OOPR" },
            { UDFLinkedSystemObjectTypesEnum.ulDrafts, "ODRF" },
            { UDFLinkedSystemObjectTypesEnum.ulMaterialRevaluation, "OMRV" },
            { UDFLinkedSystemObjectTypesEnum.ulEmployeesInfo, "OHEM" },
            { UDFLinkedSystemObjectTypesEnum.ulCustomerEquipmentCards, "OINS" },
            { UDFLinkedSystemObjectTypesEnum.ulServiceContracts, "OCTR" },
            { UDFLinkedSystemObjectTypesEnum.ulServiceCalls, "OSCL" },
            { UDFLinkedSystemObjectTypesEnum.ulProductionOrders, "OWOR" },
            { UDFLinkedSystemObjectTypesEnum.ulProjectManagementService, "OPMG" },
            { UDFLinkedSystemObjectTypesEnum.ulReturnRequest, "ORRR" },
            { UDFLinkedSystemObjectTypesEnum.ulGoodsReturnRequest, "OPRR" },
            { UDFLinkedSystemObjectTypesEnum.ulInventoryTransferRequest, "OWTQ" },
            { UDFLinkedSystemObjectTypesEnum.ulBlanketAgreementsService, "OOAT" },
        };


        /// <summary>
        /// Gets the corresponding table name for the specified UDFLinkedSystemObjectTypesEnum value.
        /// </summary>
        /// <param name="value">The UDFLinkedSystemObjectTypesEnum value whose table name is to be retrieved.</param>
        /// <returns>The table name associated with the provided enum value, or <c>null</c> if no table name is found.</returns>
        /// <seealso cref="UDFLinkedSystemObjectTypesEnum"/>
        public static string GetTableName(this UDFLinkedSystemObjectTypesEnum value) {
            return TableNames.TryGetValue(value, out string tableName) ? tableName : null;
        }

        /// <summary>
        /// Retrieves the database table name associated with a specified <see cref="UDFLinkedSystemObjectTypesEnum"/> value.
        /// </summary>
        /// <param name="value">
        /// The <see cref="UDFLinkedSystemObjectTypesEnum"/> value for which the database table name is required.
        /// </param>
        /// <returns>
        /// The corresponding database table name as a string if the value exists in the mapping; otherwise, <c>null</c>.
        /// </returns>
        /// <seealso cref="UDFLinkedSystemObjectTypesEnum"/>
        public static string GetTableName(this UDFLinkedSystemObjectTypesEnum? value) {
            return value?.GetTableName();
        }
    }
}