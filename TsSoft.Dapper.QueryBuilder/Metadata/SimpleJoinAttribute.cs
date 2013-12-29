using System;
using System.Collections.Generic;
using System.Data;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class SimpleJoinAttribute : JoinAttribute
    {
        private const string SelectExceptionString =
            "Format of SelectColumns: \"TableName:field1,field2,field3;TableName2:field1,field2,field3\"";

        public readonly string JoinedTable;

        private IDictionary<string, string[]> tableSelectColumns;

        public SimpleJoinAttribute(string currentTableField, JoinType joinType, string joinedTable)
            : base(currentTableField, joinType)
        {
            JoinedTable = joinedTable;
        }

        public IDictionary<string, string[]> TableSelectColumns
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectColumns))
                {
                    return null;
                }
                if (tableSelectColumns == null)
                {
                    tableSelectColumns = new Dictionary<string, string[]>();
                    string[] tables = SelectColumns.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string table in tables)
                    {
                        string[] res = table.Split(new[] {":"}, StringSplitOptions.None);
                        if (res.Length != 2)
                        {
                            throw new ArgumentException(SelectExceptionString);
                        }
                        if (tableSelectColumns.ContainsKey(res[0]))
                        {
                            throw new DuplicateNameException(string.Format("Table {0} already exist", res[0]));
                        }
                        tableSelectColumns.Add(res[0], res[1].Split(','));
                    }
                }
                return tableSelectColumns;
            }
        }

        public string JoinedTableField { get; set; }

        public string SelectColumns { get; set; }
    }
}