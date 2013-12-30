using System;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public abstract class JoinAttribute : Attribute
    {
        public readonly JoinType JoinType;

        protected JoinAttribute(string currentTableField, JoinType joinType)
        {
            JoinType = joinType;
            CurrentTableField = currentTableField;
        }

        public string CurrentTable { get; set; }

        public string CurrentTableField { get; set; }

        public int Order { get; set; }
    }
}