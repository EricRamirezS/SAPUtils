using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides utility methods for working with SAP-related operations, including fetching
    /// primary key information for SAP database tables.
    /// </summary>
    /// <remarks>
    /// This class contains predefined mappings of SAP database table names to their respective
    /// primary keys to help in retrieving primary key fields dynamically by table name.
    /// </remarks>
    /// <seealso cref="System.ArgumentException"/>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class SapUtils {

        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Represents a dictionary containing the primary key field names for various SAP Business One tables.
        /// The key of the dictionary is the table name, and the value is the corresponding primary key field name.
        /// </summary>
        /// <remarks>
        /// This dictionary is statically initialized with mappings for multiple table names
        /// commonly used in SAP Business One. It facilitates lookup of the primary key field name
        /// based on a table name.
        /// </remarks>
        /// <example>
        /// The <c>PrimaryKeys</c> dictionary includes mappings such as:
        /// - Table "OACT" -> Primary key "AcctCode"
        /// - Table "OCRD" -> Primary key "CardCode"
        /// - Table "OITM" -> Primary key "ItemCode"
        /// </example>
        /// <seealso cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
        private static readonly Dictionary<string, string> PrimaryKeys = new Dictionary<string, string> {
            ["OACT"] = "AcctCode",
            ["OCRD"] = "CardCode",
            ["ODSC"] = "BankCode",
            ["OITM"] = "ItemCode",
            ["OUSR"] = "USERID",
            ["OINV"] = "DocEntry",
            ["ORIN"] = "DocEntry",
            ["ODLN"] = "DocEntry",
            ["ORDN"] = "DocEntry",
            ["ORDR"] = "DocEntry",
            ["OPCH"] = "DocEntry",
            ["ORPC"] = "DocEntry",
            ["OPDN"] = "DocEntry",
            ["ORPD"] = "DocEntry",
            ["OPOR"] = "DocEntry",
            ["OQUT"] = "DocEntry",
            ["ORCT"] = "DocEntry",
            ["ODPS"] = "DeposId",
            ["OJDT"] = "TransId",
            ["OCLG"] = "ClgCode",
            ["OVPM"] = "DocEntry",
            ["OCHO"] = "DocEntry",
            ["OIGN"] = "DocEntry",
            ["OIGE"] = "DocEntry",
            ["OWHS"] = "WhsCode",
            ["OITT"] = "Code",
            ["OWTR"] = "DocEntry",
            ["OOPR"] = "OpprId",
            ["ODRF"] = "DocEntry",
            ["OMRV"] = "DocEntry",
            ["OHEM"] = "empID",
            ["OINS"] = "InsID",
            ["OCTR"] = "ContractID",
            ["OSCL"] = "callID",
            ["OWOR"] = "DocEntry",
            ["OPMG"] = "AbsEntry",
            ["ORRR"] = "DocEntry",
            ["OPRR"] = "DocEntry",
            ["OWTQ"] = "DocEntry",
            ["OOAT"] = "AbsID",
        };

        /// <summary>
        /// Represents a dictionary containing the readable column names for various SAP Business One tables.
        /// The key of the dictionary is the table name, and the value is the corresponding column name
        /// considered as the most representative or commonly used column for that table.
        /// </summary>
        /// <remarks>
        /// This dictionary is statically initializ ed with mappings for multiple SAP Business One table names.
        /// It is designed to facilitate easy access to the most relevant or descriptive column for each table,
        /// which can be used in operations such as data display, querying, or processing.
        /// </remarks>
        /// <seealso cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
        private static readonly Dictionary<string, string> ReadableColumns = new Dictionary<string, string> {
            ["OACT"] = "AcctCode", // Codigo de la cuenta
            ["OCRD"] = "CardCode", // Codigo del socio de negocios
            ["ODSC"] = "BankName", // Codigo del banco
            ["OITM"] = "ItemCode", // Codigo del ítem
            ["OUSR"] = "USER_CODE", // Nombre del usuario
            ["OINV"] = "DocNum", // Número de factura
            ["ORIN"] = "DocNum", // Número de nota de crédito
            ["ODLN"] = "DocNum", // Número de entrega
            ["ORDN"] = "DocNum", // Número de devolución
            ["ORDR"] = "DocNum", // Número de pedido de cliente
            ["OPCH"] = "DocNum", // Número de factura de proveedor
            ["ORPC"] = "DocNum", // Número de nota de crédito proveedor
            ["OPDN"] = "DocNum", // Número de entrada de mercancía
            ["ORPD"] = "DocNum", // Número de devolución de mercancía
            ["OPOR"] = "DocNum", // Número de pedido de compra
            ["OQUT"] = "DocNum", // Número de oferta de venta
            ["ORCT"] = "DocNum", // Número de recibo
            ["ODPS"] = "DeposNum", // Nombre de plantilla de documento
            ["OJDT"] = "Number", // Referencia contable
            ["OCLG"] = "ClgCode", // Detalles de actividad
            ["OVPM"] = "DocNum", // Número de pago efectuado
            ["OCHO"] = "DocNum", // Número de cheque
            ["OIGN"] = "DocNum", // Número de entrada de stock
            ["OIGE"] = "DocNum", // Número de salida de stock
            ["OWHS"] = "WhsCode", // Nombre del almacén
            ["OITT"] = "Code", // Nombre del producto terminado
            ["OWTR"] = "DocNum", // Número de transferencia de stock
            ["OOPR"] = "OpprId", // Nombre de la oportunidad
            ["ODRF"] = "DocNum", // Número de borrador
            ["OMRV"] = "DocNum", // Número de reconciliación
            ["OHEM"] = "firstName", // Nombre del empleado
            ["OINS"] = "itemCode", // Nombre de seguro
            ["OCTR"] = "ContractID", // Nombre del contrato
            ["OSCL"] = "DocNum", // Asunto del ticket
            ["OWOR"] = "DocNum", // Número de orden de producción
            ["OPMG"] = "NAME", // Nombre de la etapa de producción
            ["ORRR"] = "DocNum", // Número de reconciliación manual
            ["OPRR"] = "DocNum", // Nombre del recordatorio
            ["OWTQ"] = "DocNum", // Número de solicitud de traslado
            ["OOAT"] = "AbsID", // Nombre del activo
        };

        /// <summary>
        /// Retrieves the primary key associated with a given SAP table name.
        /// </summary>
        /// <param name="tableName">The name of the SAP table for which the primary key is to be retrieved. This parameter is case-insensitive and will be processed in uppercase format.</param>
        /// <returns>The primary key column name associated with the specified table name.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided table name is null, empty, or does not have a known primary key defined in the dictionary.</exception>
        /// <seealso cref="Dictionary{TKey, TValue}" />
        public static string GetPrimaryKey(string tableName) {
            tableName = tableName?.Trim().ToUpperInvariant();


            if (tableName != null && PrimaryKeys.TryGetValue(tableName, out string key))
                return key;

            throw new ArgumentException($"Clave primaria no conocida para la tabla '{tableName}'.");
        }

        /// <summary>
        /// Retrieves the readable column name associated with a specified SAP table name.
        /// </summary>
        /// <param name="tableName">The name of the SAP table for which the readable column is to be retrieved. This parameter is case-insensitive and will be processed in uppercase format.</param>
        /// <returns>The readable column name associated with the specified table name.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided table name is null, empty, or does not have a known readable column defined in the dictionary.</exception>
        /// <seealso cref="Dictionary{TKey, TValue}" />
        public static string GetReadableColumn(string tableName) {
            tableName = tableName?.Trim().ToUpperInvariant();


            if (tableName != null && ReadableColumns.TryGetValue(tableName, out string key))
                return key;

            throw new ArgumentException($"Clave primaria no conocida para la tabla '{tableName}'.");
        }

        /// <summary>
        /// Generates a unique identifier as a string in Base62 format, with a fixed length of 10 characters, derived from a GUID.
        /// </summary>
        /// <returns>A 10-character string representing the unique identifier, padded with leading zeros if necessary.</returns>
        public static string GenerateUniqueId() {
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            BigInteger bigInt = new BigInteger(bytes);
            if (bigInt < 0) bigInt = -bigInt;

            StringBuilder sb = new StringBuilder();
            while (bigInt > 0) {
                sb.Insert(0, Base62Chars[(int)(bigInt % 62)]);
                bigInt /= 62;
            }

            return sb.ToString().PadLeft(10, '0').Substring(0, 10);
        }
    }
}