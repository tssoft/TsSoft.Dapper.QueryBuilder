using System;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ManyToManyJoinAttribute : SimpleJoinAttribute
    {
        public readonly string CommunicationTable;
        public readonly string CommunicationTableCurrentTableField;
        public readonly string CommunicationTableJoinedTableField;

        public ManyToManyJoinAttribute(string currentTableField, JoinType joinType, string joinedTable,
            string communicationTable, string communicationTableCurrentTableField,
            string communicationTableJoinedTableField)
            : base(currentTableField, joinType, joinedTable)
        {
            CommunicationTable = communicationTable;
            CommunicationTableCurrentTableField = communicationTableCurrentTableField;
            CommunicationTableJoinedTableField = communicationTableJoinedTableField;
            AddOnType = AddOnType.ForJoined;
        }

        public AddOnType AddOnType { get; set; }
    }
}