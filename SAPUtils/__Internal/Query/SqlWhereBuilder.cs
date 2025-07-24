using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SAPUtils.Query;

namespace SAPUtils.__Internal.Query {
    public class SqlWhereBuilder {
        private readonly IWhere _rootGroup;

        public SqlWhereBuilder(IWhere rootGroup) {
            _rootGroup = rootGroup;
        }

        public string Build() {
            string clause = BuildGroup(_rootGroup);
            return string.IsNullOrWhiteSpace(clause) ? string.Empty : $"WHERE {clause}";
        }

        private static string Quote(string identifier) {
            return SapAddon.Instance().IsHana ? $"\"{identifier}\"" : $"[{identifier}]";
        }

        private static string BuildGroup(IWhere group) {
            List<string> expressions = new List<string>();

            foreach (WhereCondition cond in group.Conditions) {
                string col = Quote(cond.ColumnName);
                string clause;
                switch (cond.Comparison) {
                    case SqlComparison.Equals:
                        clause = $"{col} = {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.NotEquals:
                        clause = $"{col} <> {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.GreaterThan:
                        clause = $"{col} > {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.LessThan:
                        clause = $"{col} < {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.GreaterThanOrEqual:
                        clause = $"{col} >= {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.LessThanOrEqual:
                        clause = $"{col} <= {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.Like:
                        clause = $"{col} LIKE {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.In:
                        clause = $"{col} IN ({string.Join(", ", cond.ComparingIn.Select(FormatValue))})";
                        break;
                    case SqlComparison.Between:
                        clause = $"{col} BETWEEN {FormatValue(cond.ComparingBetween?.From)} AND {FormatValue(cond.ComparingBetween?.To)}";
                        break;
                    case SqlComparison.IsNull:
                        clause = $"{col} IS NULL";
                        break;
                    case SqlComparison.IsNotNull:
                        clause = $"{col} IS NOT NULL";
                        break;
                    case SqlComparison.NotLike:
                        clause = $"{col} NOT LIKE {FormatValue(cond.ComparingValue)}";
                        break;
                    case SqlComparison.NotIn:
                        clause = $"{col} NOT IN ({string.Join(", ", cond.ComparingIn.Select(FormatValue))})";
                        break;
                    case SqlComparison.NotBetween:
                        clause = $"{col} NOT BETWEEN {FormatValue(cond.ComparingBetween?.From)} AND {FormatValue(cond.ComparingBetween?.To)}";
                        break;
                    case SqlComparison.Exists:
                        clause = $"EXISTS ({cond.SubQuery})";
                        break;
                    case SqlComparison.NotExists:
                        clause = $"NOT EXISTS ({cond.SubQuery})";
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported comparison: {cond.Comparison}");
                }

                expressions.Add(clause);
            }

            expressions.AddRange(from sub in @group.SubGroups
                select BuildGroup(sub)
                into subClause
                where !string.IsNullOrWhiteSpace(subClause)
                select $"({subClause})");

            return string.Join(group.Operator == LogicalOperator.And ? " AND " : " OR ", expressions);
        }

        private static string FormatValue(object value) {
            switch (value) {
                case null:
                    return "NULL";
                case string s:
                    return $"'{s.Replace("'", "''")}'";
                case bool b:
                    return b ? "'Y'" : "'N'";
                case Guid g:
                    return $"'{g}'";
                case DateTime dt:
                    return $"'{dt:yyyyMMdd}'";
                case Enum e:
                    return Convert.ToInt32(e).ToString(CultureInfo.InvariantCulture);
                case float f:
                    return f.ToString(CultureInfo.InvariantCulture);
                case double d:
                    return d.ToString(CultureInfo.InvariantCulture);
                case decimal m:
                    return m.ToString(CultureInfo.InvariantCulture);
                case sbyte _:
                case byte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }
    }
}