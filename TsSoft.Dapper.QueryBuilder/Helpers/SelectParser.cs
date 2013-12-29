using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public class SelectParser : ISelectParser
    {
        private const string SelectExceptionString =
            "Format of SelectColumns: \"TableName:field1,field2,field3;TableName2:field1,field2,field3;TableName:field4\"";

        public IDictionary<string, ICollection<string>> Parse(string str)
        {
            var tableSelectColumns = new Dictionary<string, ICollection<string>>();
            string[] tables = str.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string table in tables)
            {
                string[] res = table.Split(new[] {":"}, StringSplitOptions.None);
                if (res.Length != 2)
                {
                    throw new ArgumentException(SelectExceptionString);
                }
                ICollection<string> tableColumns;
                if (tableSelectColumns.TryGetValue(res[0], out tableColumns))
                {
                    List<string> columns = res[1].Split(',').ToList();
                    foreach (string column in columns)
                    {
                        if (tableColumns.Contains(column))
                        {
                            throw new DuplicateNameException(string.Format("Column {0} already exist by table {1}",
                                                                           column, res[0]));
                        }
                        tableColumns.Add(column);
                    }
                }
                else
                {
                    tableSelectColumns.Add(res[0], res[1].Split(',').ToList());
                }
            }
            return tableSelectColumns;
        }
    }
}