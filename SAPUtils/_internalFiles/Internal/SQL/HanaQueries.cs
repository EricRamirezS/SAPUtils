namespace SAPUtils.Internal.SQL
{
    internal sealed class HanaQueries: CommonQueries
    {
        public override string GetNextCodeUserTableQuery(string tableName)
        {
            return $@"SELECT COALESCE(MAX(CAST(""Code"" AS INTEGER)), 0) + 1 AS ""Code"" FROM ""@{tableName}""";
        }
    }
}