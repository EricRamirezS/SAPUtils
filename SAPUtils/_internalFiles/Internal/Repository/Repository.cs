using System;
using SAPbobsCOM;
using SAPUtils.Internal.SQL;

namespace SAPUtils.Internal.Repository {
    public class Repository : IRepository {
        private readonly IQueries _queries;
        private readonly Recordset _recordset;
        private Company Company => SapAddon.Instance().Company;

        public static IRepository Get() => new Repository();

        private Repository() {
            if (SapAddon.Instance().IsHana) {
                _queries = new HanaQueries();
            }
            else {
                _queries = new MssqlQueries();
            }

            _recordset = Company.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
        }

        public int GetNextCodeUserTable(string tableName) {
            _recordset.DoQuery(_queries.GetNextCodeUserTableQuery(tableName));
            return Convert.ToInt32(_recordset.Fields.Item("Code").Value);
        }

        public void Dispose() {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_recordset);
            GC.Collect();
        }
    }

    public interface IRepository : IDisposable {
        int GetNextCodeUserTable(string tableName);
    }
}