using System.ComponentModel;

namespace SAPUtils.__Internal.SQL {
    [Localizable(false)]
    internal interface IQueries {
        string GetNextCodeUserTableQuery(string tableName);
        string GetFormatInformationQuery();
    }
}