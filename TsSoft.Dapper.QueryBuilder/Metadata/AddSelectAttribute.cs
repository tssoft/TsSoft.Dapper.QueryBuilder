using System;
using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class AddSelectAttribute : Attribute
    {
        private Type _parserType;

        private ISelectParser _selectParser;
        private IDictionary<string, ICollection<SelectClause>> _tableSelectColumns;

        public ISelectParser SelectParser
        {
            get
            {
                if (ParserType != null)
                {
                    _selectParser = (ISelectParser) Activator.CreateInstance(ParserType);
                }
                else
                {
                    _selectParser = new SelectParser();
                }
                return _selectParser;
            }
        }

        public IDictionary<string, ICollection<SelectClause>> TableSelectColumns
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectColumns))
                {
                    return null;
                }
                return _tableSelectColumns ?? (_tableSelectColumns = SelectParser.Parse(SelectColumns));
            }
        }

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