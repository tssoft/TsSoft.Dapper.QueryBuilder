using System;
using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public sealed class SimpleJoinClauseCreator : IJoinClauseCreator
    {
        public JoinClause Create(JoinAttribute joinAttribute)
        {
            SimpleJoinAttribute simpleJoinAttribute;
            if ((simpleJoinAttribute = joinAttribute as SimpleJoinAttribute) == null)
            {
                throw new ArgumentException("Attribute must be SimpleJoinAttribute");
            }

            string splitter = string.Format("SplitOn{0}{1}", simpleJoinAttribute.JoinedTable,
                                            simpleJoinAttribute.JoinedTableField);
            string select = string.Format("{0}.*", simpleJoinAttribute.JoinedTable);
            string joinSql = string.Format("{0} on {0}.{1} = {2}.{3}", simpleJoinAttribute.JoinedTable,
                                           simpleJoinAttribute.JoinedTableField, simpleJoinAttribute.CurrentTable,
                                           simpleJoinAttribute.CurrentTableField);

            var result = new JoinClause
                {
                    JoinSqls = new List<string>
                        {
                            joinSql,
                        },
                    SelectSql = select,
                    Splitter = splitter,
                    JoinType = simpleJoinAttribute.JoinType,
                    HasJoin = true,
                };
            return result;
        }

        public JoinClause CreateNotJoin(JoinAttribute joinAttribute)
        {
            SimpleJoinAttribute simpleJoinAttribute;
            if ((simpleJoinAttribute = joinAttribute as SimpleJoinAttribute) == null)
            {
                throw new ArgumentException("Attribute must be SimpleJoinAttribute");
            }
            string splitter = string.Format("SplitOn{0}{1}", simpleJoinAttribute.JoinedTable,
                                            simpleJoinAttribute.JoinedTableField);
            return new JoinClause
                {
                    HasJoin = false,
                    Splitter = splitter,
                    JoinType = simpleJoinAttribute.JoinType,
                };
        }
    }
}