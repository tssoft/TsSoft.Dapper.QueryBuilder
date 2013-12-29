using System;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class SimpleJoinAttribute : JoinAttribute
    {
        public SimpleJoinAttribute(string joinedTable, string currentTableField, JoinType joinType) : base(currentTableField, joinType)
        {
            JoinedTable = joinedTable;
        }

        public readonly string JoinedTable;

        public string JoinedTableField { get; set; }
    }

    public abstract class JoinAttribute : Attribute
    {
        protected JoinAttribute(string currentTableField, JoinType joinType)
        {
            JoinType = joinType;
            CurrentTableField = currentTableField;
        }

        public readonly JoinType JoinType;

        public string CurrentTable { get; set; }

        public string CurrentTableField { get; set; }
    }
}