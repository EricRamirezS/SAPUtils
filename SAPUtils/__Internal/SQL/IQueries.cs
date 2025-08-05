namespace SAPUtils.__Internal.SQL {
    internal interface IQueries {
        string GetNextCodeUserTableQuery(string tableName);
        string GetFormatInformationQuery();
    }
}