using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SAPbobsCOM;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.SQL;
using SAPUtils.Models.UserTables;

namespace SAPUtils.__Internal.Repository {
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
        /// Represents an instance of the SAP Business One Recordset object used for executing SQL queries and data
        /// retrieval within the Business One system.
        /// </summary>
        /// <remarks>
        /// The <see cref="_recordset"/> variable is initialized as a SAP Business One Recordset object,
        /// used to interact with the company database and execute SQL-based operations.
        /// It is instantiated based on the SAP Company connection provided by the <see cref="SapAddon"/> instance.
        /// It is disposed of when the Repository instance is disposed to release unmanaged resources.
        /// </remarks>
        private readonly Recordset _recordset;

        /// <summary>
        /// Provides an implementation of the repository layer for managing
        /// data-related operations within the SAP Business One integration framework.
        /// </summary>
        /// <remarks>
        /// The <c>Repository</c> class abstracts data operations using database-specific
        /// query objects and enables interaction with the SAP Business One `Recordset` object.
        /// Depending on the database type (HANA or MSSQL), the appropriate query implementation is instantiated.
        /// </remarks>
        private Repository() {
            if (SapAddon.Instance().IsHana) {
                _queries = new HanaQueries();
            }
            else {
                _queries = new MssqlQueries();
            }

            _recordset = Company.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
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
        public int GetNextCodeUserTable(string tableName) {
            _recordset.DoQuery(_queries.GetNextCodeUserTableQuery(tableName));
            return Convert.ToInt32(_recordset.Fields.Item("Code").Value);
        }

        /// <inheritdoc />
        public IList<IUserFieldValidValue> GetValidValues(string table) {
            IList<IUserFieldValidValue> data = new List<IUserFieldValidValue>();
            try {
                try {
                    _recordset.DoQuery($"SELECT \"Code\", \"Name\" FROM \"@{table}\" WHERE \"U_Active\" = 'Y'");
                }
                catch {
                    _recordset.DoQuery($"SELECT \"Code\", \"Name\" FROM \"@{table}\"");
                }
                while (!_recordset.EoF) {
                    data.Add(new UserFieldValidValue(_recordset.Fields.Item("Code").Value.ToString(),
                        _recordset.Fields.Item("Name").Value.ToString()));
                    _recordset.MoveNext();
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
            Marshal.ReleaseComObject(_recordset);
            GC.Collect();
        }

        /// <summary>
        /// Provides a static method to retrieve an implementation of the <see cref="IRepository"/> interface.
        /// This method returns an instance of the <see cref="Repository"/> class, which implements
        /// the necessary functionalities for repository pattern operations.
        /// </summary>
        /// <returns>An instance of the <see cref="IRepository"/> interface, specifically implemented by <see cref="Repository"/>.</returns>
        public static IRepository Get() => new Repository();
    }

    /// <summary>
    /// Defines the contract for a repository that provides operations for interacting with user-defined tables.
    /// </summary>
    public interface IRepository : IDisposable {
        /// <summary>
        /// Retrieves the next available code for a user table based on its name.
        /// </summary>
        /// <param name="tableName">The name of the user table for which the next code is to be retrieved.</param>
        /// <returns>The next available code as an integer for the specified user table.</returns>
        int GetNextCodeUserTable(string tableName);
        /// <summary>
        /// Retrieves a collection of valid values for a specified user-defined table.
        /// </summary>
        /// <param name="fieldLinkedTable">
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
        IList<IUserFieldValidValue> GetValidValues(string fieldLinkedTable);
    }
}