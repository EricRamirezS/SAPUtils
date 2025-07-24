using System.Collections.Generic;
using SAPUtils.Query;

namespace SAPUtils.__Internal.Query {

    internal class WhereCondition : IWhereCondition {
        public string ColumnName { get; set; }
        public SqlComparison Comparison { get; set; }
        public object ComparingValue { get; set; }
        public (object From, object To)? ComparingBetween { get; set; }
        public IEnumerable<object> ComparingIn { get; set; }
        public string SubQuery { get; set; }
    }
}