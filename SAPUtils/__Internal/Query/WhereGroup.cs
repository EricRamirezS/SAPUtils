using System.Collections.Generic;
using SAPUtils.Query;

namespace SAPUtils.__Internal.Query {
    internal class WhereGroup : IWhere {
        public LogicalOperator Operator { get; set; } = LogicalOperator.And;
        public List<IWhereCondition> Conditions { get; } = new List<IWhereCondition>();
        public List<IWhere> SubGroups { get; } = new List<IWhere>();
    }
}