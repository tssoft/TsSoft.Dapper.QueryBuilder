using System;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
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

        public string Including { get; set; }

        public bool NoSplit { get; set; }

        public string AddOnClause { get; set; }
    }
}