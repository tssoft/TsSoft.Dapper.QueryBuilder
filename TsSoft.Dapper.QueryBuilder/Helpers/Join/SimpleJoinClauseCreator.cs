using System;
using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public sealed class SimpleJoinClauseCreator : JoinClauseCreator
    {
        public override JoinClause Create(JoinAttribute joinAttribute)
        {
            SimpleJoinAttribute simpleJoinAttribute;
            if ((simpleJoinAttribute = joinAttribute as SimpleJoinAttribute) == null)
            {
                throw new ArgumentException("Attribute must be SimpleJoinAttribute");
            }

            string splitter = string.Format("SplitOn{0}{1}", simpleJoinAttribute.JoinedTable,
                                            simpleJoinAttribute.JoinedTableField);
            var selects = new List<string>();
            if (simpleJoinAttribute.TableSelectColumns != null && simpleJoinAttribute.TableSelectColumns.Any())
            {
                foreach (var tableSelectColumn in simpleJoinAttribute.TableSelectColumns)
                {
                    selects.AddRange(tableSelectColumn.Value.Select(column => string.Format("{0}.{1}", tableSelectColumn.Key, column)));
                }
            }
            else
            {
                selects.Add(string.Format("{0}.*", simpleJoinAttribute.JoinedTable));
            }
            string joinSql = string.Format("{0} on {0}.{1} = {2}.{3}", simpleJoinAttribute.JoinedTable,
                                           simpleJoinAttribute.JoinedTableField, simpleJoinAttribute.CurrentTable,
                                           simpleJoinAttribute.CurrentTableField);

            var result = new JoinClause
                {
                    JoinSqls = new List<string>
                        {
                            joinSql,
                        },
                    SelectsSql = selects,
                    Splitter = splitter,
                    JoinType = simpleJoinAttribute.JoinType,
                    HasJoin = true,
                };
            return result;
        }
    }
}