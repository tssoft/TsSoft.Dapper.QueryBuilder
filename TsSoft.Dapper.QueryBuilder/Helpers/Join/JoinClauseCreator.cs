using System;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public abstract class JoinClauseCreator : IJoinClauseCreator
    {
        public abstract JoinClause Create(JoinAttribute joinAttribute);

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
                    Order = joinAttribute.Order
                };
        }
    }
}