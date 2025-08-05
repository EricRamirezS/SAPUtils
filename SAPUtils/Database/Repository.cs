using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using SAPbobsCOM;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.SQL;
using SAPUtils.Models.UserTables;
using SAPUtils.Query;

// ReSharper disable MemberCanBePrivate.Global

namespace SAPUtils.Database {
    /// <summary>
    /// Provides methods for managing user table records and obtaining the next available code
    /// for user-defined tables. Implements the <see cref="IRepository"/> interface.
    /// This class is designed to be used internally within the system for handling SAP Business One
    /// table record management and primary key generation.
    /// </summary>
    public class Repository : IRepository {
        /// <summary>
        /// Represents the queries interface implementation used for database query execution.
        /// </summary>
        /// <remarks>
        /// This variable determines which specific query implementation to use
        /// (e.g., HanaQueries for HANA databases or MssqlQueries for Microsoft SQL databases),
        /// based on the database type.
        /// </remarks>
        private readonly IQueries _queries;

        /// <summary>
        /// Indicates whether the database server in use is SAP HANA.
        /// </summary>
        /// <remarks>
        /// This variable is derived from <see cref="SapAddon.IsHana"/>, which checks the database type
        /// of the connected SAP Business One company. It is utilized to optimize database operations
        /// and implement logic specific to HANA databases.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon"/>
        protected readonly bool IsHana;

        /// <summary>
        /// Represents an instance of the SAP Business One Recordset object used for executing SQL queries and data
        /// retrieval within the Business One system.
        /// </summary>
        /// <remarks>
        /// The <see cref="Recordset"/> variable is initialized as a SAP Business One Recordset object,
        /// used to interact with the company database and execute SQL-based operations.
        /// It is instantiated based on the SAP Company connection provided by the <see cref="SapAddon"/> instance.
        /// It is disposed of when the Repository instance is disposed to release unmanaged resources.
        /// </remarks>
        protected readonly Recordset Recordset;

        /// <summary>
        /// Provides an implementation of the repository layer for managing
        /// data-related operations within the SAP Business One integration framework.
        /// </summary>
        /// <remarks>
        /// The <c>Repository</c> class abstracts data operations using database-specific
        /// query objects and enables interaction with the SAP Business One `Recordset` object.
        /// Depending on the database type (HANA or MSSQL), the appropriate query implementation is instantiated.
        /// </remarks>
        protected Repository() {
            IsHana = SapAddon.Instance().IsHana;
            if (IsHana) {
                _queries = new HanaQueries();
            }
            else {
                _queries = new MssqlQueries();
            }

            Recordset = Company.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
        }

        /// <summary>
        /// Represents a connection to the SAP Business One company.
        /// </summary>
        /// <remarks>
        /// The <c>Company</c> object is a part of the SAP Business One SDK and provides
        /// methods and properties for interacting with the database and various components
        /// of the SAP Business One application. It is typically used to handle company-level
        /// configurations and data operations.
        /// </remarks>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private Company Company => SapAddon.Instance().Company;

        /// <inheritdoc />
        public IList<IUserFieldValidValue> GetValidValuesFromUserTable(string userTableName) {
            IList<IUserFieldValidValue> data = new List<IUserFieldValidValue>();
            try {
                Type tableType = UserTableMetadataCache.GetTableType(userTableName);
                if (tableType != null) {

                    MethodInfo getAllMethod = FindStaticGetAllMethod(tableType);
                    if (getAllMethod != null) {
                        IWhere where = null;
                        if (typeof(ISoftDeletable).IsAssignableFrom(tableType)) {
                            where = Where.Builder().Equals("Active", true).Build();
                        }

                        object result = getAllMethod.Invoke(null, new object[] {
                            where,
                        });
                        IEnumerable<object> items = (IEnumerable<object>)result;
                        foreach (object item in items) {
                            string code = item.GetType().GetProperty("Code")?.GetValue(item)?.ToString();
                            string name = item.GetType().GetProperty("DisplayName")?.GetValue(item)?.ToString();

                            if (code != null && name != null)
                                data.Add(new UserFieldValidValue(code, name));
                        }
                        return data;
                    }
                }
                Recordset.DoQuery($"SELECT \"Code\", \"Name\" FROM \"@{userTableName}\"");

                while (!Recordset.EoF) {
                    data.Add(new UserFieldValidValue(Recordset.Fields.Item("Code").Value.ToString(),
                        Recordset.Fields.Item("Name").Value.ToString()));
                    Recordset.MoveNext();
                }
            }
            catch (Exception ex) {
                SapAddon.Instance().Logger.Error(ex);
            }
            return data;
        }


        /// <summary>
        /// Releases all resources used by the <see cref="Repository"/> class.
        /// </summary>
        /// <remarks>
        /// This method releases the Recordset COM object and forces garbage collection.
        /// It should be called when the object is no longer needed to ensure proper resource management
        /// and to avoid memory leaks, especially when working with unmanaged resources.
        /// </remarks>
        public void Dispose() {
            Marshal.ReleaseComObject(Recordset);
            GC.Collect();
        }

        internal int GetNextCodeUserTable(string tableName) {
            Recordset.DoQuery(_queries.GetNextCodeUserTableQuery(tableName));
            return Convert.ToInt32(Recordset.Fields.Item("Code").Value);
        }
        internal FormatInfo GetFormatInformation() {
            Recordset.DoQuery(_queries.GetFormatInformationQuery());
            FormatInfo formatInfo = new FormatInfo {
                SumDec = Convert.ToInt32(Recordset.Fields.Item("SumDec").Value),
                PriceDec = Convert.ToInt32(Recordset.Fields.Item("PriceDec").Value),
                RateDec = Convert.ToInt32(Recordset.Fields.Item("RateDec").Value),
                QtyDec = Convert.ToInt32(Recordset.Fields.Item("QtyDec").Value),
                PercentDec = Convert.ToInt32(Recordset.Fields.Item("PercentDec").Value),
                MeasureDec = Convert.ToInt32(Recordset.Fields.Item("MeasureDec").Value),
                QueryDec = Convert.ToInt32(Recordset.Fields.Item("QueryDec").Value),
                DecSep = Convert.ToString(Recordset.Fields.Item("DecSep").Value),
                ThousSep = Convert.ToString(Recordset.Fields.Item("ThousSep").Value),
                TimeFormat = Convert.ToInt32(Recordset.Fields.Item("TimeFormat").Value),
                DateFormat = Convert.ToInt32(Recordset.Fields.Item("DateFormat").Value),
                DateSep = Convert.ToString(Recordset.Fields.Item("DateSep").Value),
            };

            return formatInfo;

        }

        private static MethodInfo FindStaticGetAllMethod(Type type) {
            while (type != null) {
                MethodInfo method = type.GetMethod(
                    "GetAll",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] {
                        typeof(IWhere),
                    },
                    null
                );

                if (method != null)
                    return method;

                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Provides a static method to retrieve an implementation of the <see cref="IRepository"/> interface.
        /// This method returns an instance of the <see cref="Repository"/> class, which implements
        /// the necessary functionalities for repository pattern operations.
        /// </summary>
        /// <returns>An instance of the <see cref="IRepository"/> interface, specifically implemented by <see cref="Repository"/>.</returns>
        internal static IRepository Get() => new Repository();
    }

    /// <summary>
    /// Defines the contract for a repository that provides operations for interacting with user-defined tables.
    /// </summary>
    public interface IRepository : IDisposable {
        /// <summary>
        /// Retrieves a collection of valid values for a specified user-defined table.
        /// </summary>
        /// <param name="userTableName">
        /// The name of the user-defined table for which valid values are to be retrieved.
        /// </param>
        /// <returns>
        /// A list of <see cref="IUserFieldValidValue"/> objects representing the valid values
        /// for fields in the specified user-defined table.
        /// </returns>
        /// <remarks>
        /// This method is responsible for querying the user-defined table to fetch
        /// its valid values. It utilizes the internal repository logic to ensure
        /// proper interaction with the underlying database.
        /// </remarks>
        /// <seealso cref="IUserFieldValidValue"/>
        IList<IUserFieldValidValue> GetValidValuesFromUserTable(string userTableName);
    }
}