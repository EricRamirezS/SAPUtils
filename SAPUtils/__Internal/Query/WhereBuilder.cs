using System;
using System.Collections.Generic;
using System.Linq;
using SAPUtils.Query;

namespace SAPUtils.__Internal.Query {

    internal class WhereBuilder : IWhereBuilder {
        private readonly Stack<WhereGroup> _groupStack = new Stack<WhereGroup>();

        internal WhereBuilder(LogicalOperator op = LogicalOperator.And) {
            WhereGroup root = new WhereGroup { Operator = op };
            _groupStack.Push(root);
        }


        public IWhereBuilder Group(LogicalOperator op, Action<IWhereBuilder> groupAction) {
            WhereGroup subgroup = new WhereGroup { Operator = op };
            _groupStack.Peek().SubGroups.Add(subgroup);
            _groupStack.Push(subgroup);
            groupAction(this);
            _groupStack.Pop();
            return this;
        }

        public IWhereBuilder Equals(string column, object value) =>
            AddCondition(column, SqlComparison.Equals, value);

        public IWhereBuilder NotEquals(string column, object value) =>
            AddCondition(column, SqlComparison.NotEquals, value);

        public IWhereBuilder GreaterThan(string column, object value) =>
            AddCondition(column, SqlComparison.GreaterThan, value);

        public IWhereBuilder GreaterThanOrEqual(string column, object value) =>
            AddCondition(column, SqlComparison.GreaterThanOrEqual, value);

        public IWhereBuilder LessThan(string column, object value) =>
            AddCondition(column, SqlComparison.LessThan, value);

        public IWhereBuilder LessThanOrEqual(string column, object value) =>
            AddCondition(column, SqlComparison.LessThanOrEqual, value);

        public IWhereBuilder Like(string column, string pattern) =>
            AddCondition(column, SqlComparison.Like, pattern);

        public IWhereBuilder NotLike(string column, string pattern) =>
            AddCondition(column, SqlComparison.NotLike, pattern);

        public IWhereBuilder IsNull(string column) =>
            AddCondition(column, SqlComparison.IsNull, null);

        public IWhereBuilder IsNotNull(string column) =>
            AddCondition(column, SqlComparison.IsNotNull, null);

        public IWhereBuilder Between(string column, object from, object to) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                ColumnName = column,
                Comparison = SqlComparison.Between,
                ComparingBetween = (from, to),
            });
            return this;
        }

        public IWhereBuilder NotBetween(string column, object from, object to) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                ColumnName = column,
                Comparison = SqlComparison.NotBetween,
                ComparingBetween = (from, to),
            });
            return this;
        }

        public IWhereBuilder In(string column, IEnumerable<object> values) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                ColumnName = column,
                Comparison = SqlComparison.In,
                ComparingIn = values,
            });
            return this;
        }

        public IWhereBuilder NotIn(string column, IEnumerable<object> values) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                ColumnName = column,
                Comparison = SqlComparison.NotIn,
                ComparingIn = values,
            });
            return this;
        }

        public IWhereBuilder Exists(string subquery) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                Comparison = SqlComparison.Exists,
                SubQuery = subquery,
            });
            return this;
        }

        public IWhereBuilder NotExists(string subquery) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                Comparison = SqlComparison.NotExists,
                SubQuery = subquery,
            });
            return this;
        }

        public IWhere Build() {
            return _groupStack.Last();
        }


        private IWhereBuilder AddCondition(string column, SqlComparison comparison, object value) {
            _groupStack.Peek().Conditions.Add(new WhereCondition {
                ColumnName = column,
                Comparison = comparison,
                ComparingValue = value,
            });
            return this;
        }
    }
}