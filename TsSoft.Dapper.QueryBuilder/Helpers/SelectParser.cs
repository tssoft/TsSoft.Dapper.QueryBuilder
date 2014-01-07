using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public class SelectParser : ISelectParser
    {
        protected virtual string TableDelimiter
        {
            get { return ";"; }
        }

        protected virtual string TableEntryDelimiter
        {
            get { return ":"; }
        }

        protected virtual string ColumnDelimiter
        {
            get { return ","; }
        }

        protected virtual string LeftExpressionDelimiter
        {
            get { return "{{"; }
        }

        protected virtual string RightExpressionDelimiter
        {
            get { return "}}"; }
        }

        public IDictionary<string, ICollection<SelectClause>> Parse(string str, bool strict = true)
        {
            var tables = str.Split(new[] {TableDelimiter}, StringSplitOptions.RemoveEmptyEntries);
            IDictionary<string, ICollection<SelectClause>> result = new Dictionary<string, ICollection<SelectClause>>();
            foreach (var tableColumnsStr in tables)
            {
                ParseOne(tableColumnsStr, strict, ref result);
            }
            return result;
        }

        public void ParseOne(string tableColumnsStr, bool strict,
            ref IDictionary<string, ICollection<SelectClause>> tableColumns)
        {
            var tableName =
                tableColumnsStr.Split(new[] {TableEntryDelimiter}, 2, StringSplitOptions.RemoveEmptyEntries)[0];
            tableColumnsStr = tableColumnsStr.Remove(0, tableName.Length + 1);
            var hasExpression = tableColumnsStr.IndexOf(LeftExpressionDelimiter, StringComparison.Ordinal) != -1;
            ICollection<SelectClause> cols;
            var splitted = tableColumnsStr.Split(new[] {LeftExpressionDelimiter, RightExpressionDelimiter},
                StringSplitOptions.RemoveEmptyEntries);
            if (tableColumns.TryGetValue(tableName, out cols))
            {
                SaveColumns(splitted, strict, hasExpression, tableName, ref cols);
            }
            else
            {
                cols = new List<SelectClause>();
                SaveColumns(splitted, strict, hasExpression, tableName, ref cols);
                tableColumns.Add(new KeyValuePair<string, ICollection<SelectClause>>(tableName, cols));
            }
        }

        private void SaveColumns(IEnumerable<string> splittedColumns, bool strict, bool hasExpression, string table,
            ref ICollection<SelectClause> clauses)
        {
            foreach (var splittedColumn in splittedColumns)
            {
                if (!hasExpression || splittedColumn.StartsWith(ColumnDelimiter) ||
                    splittedColumn.EndsWith(ColumnDelimiter))
                {
                    var splittedClause = splittedColumn.Split(new[] {ColumnDelimiter},
                        StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => new SelectClause
                        {
                            Select = x,
                            Table = table,
                            IsExpression = false,
                        });
                    foreach (var selectClause in splittedClause)
                    {
                        if (strict && clauses.Contains(selectClause))
                        {
                            throw new DuplicateNameException();
                        }
                        clauses.Add(selectClause);
                    }
                }
                else
                {
                    clauses.Add(new SelectClause
                    {
                        Select = splittedColumn,
                        IsExpression = true,
                        Table = table,
                    });
                }
            }
        }
    }
}