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
            var splitter = GetSplitter(simpleJoinAttribute);
            return new JoinClause
                {
                    HasJoin = false,
                    Splitter = splitter,
                    JoinType = simpleJoinAttribute.JoinType,
                    Order = joinAttribute.Order,
                };
        }

        protected string GetSplitter(SimpleJoinAttribute joinAttribute)
        {
            return joinAttribute.NoSplit
                       ? string.Empty
                       : string.Format("SplitOn{0}{1}", joinAttribute.JoinedTable,
                                       joinAttribute.JoinedTableField);
        }

        protected virtual string GetAddOnClauses(JoinAttribute joinAttribute)
        {
            return string.IsNullOrWhiteSpace(joinAttribute.AddOnClause)
                       ? string.Empty
                       : string.Format(" AND {0}", joinAttribute.AddOnClause);
        }
    }
}