namespace SAPUtils.__Internal.SQL {
    internal abstract partial class CommonQueries {
        public abstract string GetNextCodeUserTableQuery(string tableName);
        public abstract string GetFormatInformationQuery();
    }
}