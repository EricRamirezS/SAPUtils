using System.Collections.Generic;
using SAPUtils.Query;

namespace SAPUtils.__Internal.Query {
    internal class WhereGroup : IWhere {
        public LogicalOperator Operator { get; set; } = LogicalOperator.And;
        public List<IWhereCondition> Conditions { get; set; } = new List<IWhereCondition>();
        public List<IWhere> SubGroups { get; set; } = new List<IWhere>();
    }
}