using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
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
        }
    }
}