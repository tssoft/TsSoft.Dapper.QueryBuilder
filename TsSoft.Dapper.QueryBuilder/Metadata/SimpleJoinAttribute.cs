using System;
using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SimpleJoinAttribute : JoinAttribute
    {
        public readonly string JoinedTable;
        private Type _parserType;

        private IDictionary<string, ICollection<SelectClause>> _tableSelectColumns;

        public SimpleJoinAttribute(string currentTableField, JoinType joinType, string joinedTable)
            : base(currentTableField, joinType)
        {
            JoinedTable = joinedTable;
        }

        public IDictionary<string, ICollection<SelectClause>> TableSelectColumns
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectColumns))
                {
                    return null;
                }
                if (_tableSelectColumns == null)
                {
                    ISelectParser selectParser;
                    if (ParserType != null)
                    {
                        selectParser = (ISelectParser) Activator.CreateInstance(ParserType);
                    }
                    else
                    {
                        selectParser = new SelectParser();
                    }
                    _tableSelectColumns = selectParser.Parse(SelectColumns);
                }
                return _tableSelectColumns;
            }
        }

        public string JoinedTableField { get; set; }

        public string SelectColumns { get; set; }

        public Type ParserType
        {
            get { return _parserType; }
            set
            {
                if (!typeof (ISelectParser).IsAssignableFrom(value))
                {
                    throw new InvalidCastException("Parser must implement ISelectParser");
                }
                _parserType = value;
            }
        }
    }
}