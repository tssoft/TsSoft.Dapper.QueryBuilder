using System;
using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Select
{
    public class SelectClauseManager : IClauseManager<SelectClause>
    {
        public IEnumerable<SelectClause> Get(Criteria criteria, string tableName)
        {
            var res = new List<SelectClause>();
            if (criteria.Select != null)
            {
                res.Add(new SelectClause
                {
                    IsExpression = criteria.Select.IsExpression,
                    Select = criteria.Select.Select,
                    Table = !string.IsNullOrWhiteSpace(criteria.Select.Table) ? criteria.Select.Table : tableName,
                });
            }
            var type = criteria.GetType();
            var props = type.GetProperties().Where(pi => pi.HasAttribute<AddSelectAttribute>());
            foreach (var propertyInfo in props)
            {
                if (propertyInfo.PropertyType != typeof (bool) && propertyInfo.PropertyType != typeof (string))
                {
                    throw new NotImplementedException("Select implemented to only bool or string properties");
                }
                var addSelectAttribute = propertyInfo.GetCustomAttribute<AddSelectAttribute>();
                if (propertyInfo.PropertyType == typeof (bool))
                {
                    if (!(bool) propertyInfo.GetValue(criteria, null))
                    {
                        continue;
                    }
                    foreach (var tableSelectColumn in addSelectAttribute.TableSelectColumns)
                    {
                        res.AddRange(tableSelectColumn.Value);
                    }
                }
                else if (propertyInfo.PropertyType == typeof (string))
                {
                    var str = (string) propertyInfo.GetValue(criteria, null);
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        continue;
                    }
                    var clauses = addSelectAttribute.SelectParser.Parse(str);
                    foreach (var tableSelectColumn in clauses)
                    {
                        res.AddRange(tableSelectColumn.Value);
                    }
                }
            }
            return res;
        }
    }
}