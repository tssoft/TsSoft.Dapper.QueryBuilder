using System;
using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class SimpleJoinAttribute : JoinAttribute
    {
        public readonly string JoinedTable;
        private Type parser;

        private IDictionary<string, ICollection<string>> tableSelectColumns;

        public SimpleJoinAttribute(string currentTableField, JoinType joinType, string joinedTable)
            : base(currentTableField, joinType)
        {
            JoinedTable = joinedTable;
        }

        public IDictionary<string, ICollection<string>> TableSelectColumns
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectColumns))
                {
                    return null;
                }
                if (tableSelectColumns == null)
                {
                    ISelectParser selectParser;
                    if (Parser != null)
                    {
                        selectParser = (ISelectParser) Activator.CreateInstance(Parser);
                    }
                    else
                    {
                        selectParser = new SelectParser();
                    }
                    tableSelectColumns = selectParser.Parse(SelectColumns);
                }
                return tableSelectColumns;
            }
        }

        public string JoinedTableField { get; set; }

        public string SelectColumns { get; set; }

        public Type Parser
        {
            get { return parser; }
            set
            {
                if (!typeof (ISelectParser).IsAssignableFrom(value))
                {
                    throw new InvalidCastException("Parser must implement ISelectParser");
                }
                parser = value;
            }
        }
    }
}