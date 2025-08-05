namespace SAPUtils.__Internal.SQL {
    internal sealed class HanaQueries : CommonQueries {
        public override string GetNextCodeUserTableQuery(string tableName) {
            return $@"SELECT COALESCE(MAX(CAST(""Code"" AS INTEGER)), 0) + 1 AS ""Code"" FROM ""@{tableName}""";
        }
        public override string GetFormatInformationQuery() {
            return @"SELECT ""SumDec"", ""PriceDec"", ""RateDec"", ""QtyDec"", ""PercentDec"", ""MeasureDec"", ""QueryDec"", ""DecSep"", ""ThousSep"",""TimeFormat"", ""DateFormat"", ""DateSep"" FROM ""OADM""";
        }
    }
}