using System;
using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Select
{
    public class SelectClauseManager : IClauseManager<SelectClause>
    {
        public IEnumerable<SelectClause> Get(Criteria criteria, string tableName)
        {
            var res = new List<SelectClause>();
            if (criteria.SelectClause != null)
            {
                res.Add(new SelectClause
                {
                    IsExpression = criteria.SelectClause.IsExpression,
                    Select = criteria.SelectClause.Select,
                    Table =
                        !string.IsNullOrWhiteSpace(criteria.SelectClause.Table)
                            ? criteria.SelectClause.Table
                            : tableName,
                });
            }
            if (criteria.QueryType == QueryType.Sum)
            {
                return res;
            }
            var type = criteria.GetType();
            var props = type.GetProperties().Where(pi => pi.HasAttribute<AddSelectAttribute>());
            foreach (var propertyInfo in props)
            {
                var propIsBool = propertyInfo.PropertyType == typeof (bool);
                var propIsNullable = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>);
                var propIsString = propertyInfo.PropertyType == typeof (string);
                if (!propIsBool && !propIsString && !propIsNullable)
                {
                    throw new NotImplementedException("Select implemented to only bool or string or nullable properties");
                }
                var addSelectAttribute = propertyInfo.GetCustomAttribute<AddSelectAttribute>();
                if (propIsBool)
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
                else if (propIsNullable)
                {
                    if (propertyInfo.GetValue(criteria, null) == null)
                    {
                        continue;
                    }
                    foreach (var tableSelectColumn in addSelectAttribute.TableSelectColumns)
                    {
                        res.AddRange(tableSelectColumn.Value);
                    }
                }
                else if (propIsString)
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